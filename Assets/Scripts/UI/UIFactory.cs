using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Tüm arayüz kod ile kurulur; prefab veya sprite dosyası gerekmez.
    /// İkon sprite'ları (daire, üçgen) çalışma zamanında çizilir.
    /// </summary>
    public static class UIFactory
    {
        static Font _font;
        static Sprite _circleSprite;
        static Sprite _triangleSprite;

        public static Font DefaultFont
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static Sprite CircleSprite
        {
            get
            {
                if (_circleSprite == null)
                    _circleSprite = BuildCircleSprite(96);
                return _circleSprite;
            }
        }

        /// <summary>Yukarı bakan üçgen; döndürerek yön oku olarak kullanılır.</summary>
        public static Sprite TriangleSprite
        {
            get
            {
                if (_triangleSprite == null)
                    _triangleSprite = BuildTriangleSprite(96);
                return _triangleSprite;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform CreateRect(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        public static Image CreatePanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Image CreateIcon(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Sprite sprite, Color color)
        {
            var image = CreatePanel(name, parent, anchorMin, anchorMax, position, size, color);
            image.sprite = sprite;
            return image;
        }

        public static Text CreateText(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
            string content, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, bool withOutline = true)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, position, size);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            if (withOutline)
            {
                var outline = rect.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
                outline.effectDistance = new Vector2(2f, -2f);
            }
            return text;
        }

        public static Button CreateButton(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
            string label, int fontSize, Color background, UnityEngine.Events.UnityAction onClick)
        {
            var image = CreatePanel(name, parent, anchorMin, anchorMax, position, size, background);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
                button.onClick.AddListener(onClick);

            CreateText(name + "Label", image.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                label, fontSize, Color.white);
            return button;
        }

        static Sprite BuildCircleSprite(int size)
        {
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            var center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - distance + 1f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        static Sprite BuildTriangleSprite(int size)
        {
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                float y01 = (y + 0.5f) / size;
                float halfWidth = (1f - y01) * 0.5f; // Tepe noktası üstte
                for (int x = 0; x < size; x++)
                {
                    float x01 = (x + 0.5f) / size;
                    float alpha = Mathf.Clamp01((halfWidth - Mathf.Abs(x01 - 0.5f)) * size);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }
    }
}

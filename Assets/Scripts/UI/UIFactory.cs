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
        static Sprite _chevronSprite;
        static Sprite _coinSprite;
        static Sprite _starSprite;

        /// <summary>Kalın, gövdeli yukarı ok — görev yönü için.</summary>
        public static Sprite ChevronSprite
        {
            get
            {
                if (_chevronSprite == null)
                    _chevronSprite = BuildChevronSprite(128);
                return _chevronSprite;
            }
        }

        /// <summary>Renkli altın sikke (tint etme — Image.color = beyaz bırak).</summary>
        public static Sprite CoinSprite
        {
            get
            {
                if (_coinSprite == null)
                    _coinSprite = BuildCoinSprite(128);
                return _coinSprite;
            }
        }

        /// <summary>Beş köşeli sarı yıldız.</summary>
        public static Sprite StarSprite
        {
            get
            {
                if (_starSprite == null)
                    _starSprite = BuildStarSprite(128);
                return _starSprite;
            }
        }

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
            text.raycastTarget = false;

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

        static Sprite BuildChevronSprite(int size)
        {
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                float y01 = (y + 0.5f) / size;
                for (int x = 0; x < size; x++)
                {
                    float x01 = (x + 0.5f) / size;
                    float dx = Mathf.Abs(x01 - 0.5f);

                    // Gövde (alt yarı) + geniş üçgen kafa (üst).
                    bool shaft = y01 < 0.48f && dx < 0.16f;
                    float headHalf = Mathf.Lerp(0.48f, 0.02f, Mathf.InverseLerp(0.32f, 1f, y01));
                    bool head = y01 >= 0.32f && dx < headHalf;
                    float alpha = (shaft || head) ? 1f : 0f;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        static Sprite BuildCoinSprite(int size)
        {
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            var center = new Vector2(size / 2f, size / 2f);
            float r = size / 2f - 2f;
            var rim = new Color32(176, 110, 18, 255);
            var gold = new Color32(255, 205, 50, 255);
            var face = new Color32(255, 224, 96, 255);
            var shine = new Color32(255, 255, 230, 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Vector2.Distance(p, center);
                    float a = Mathf.Clamp01(r - d + 1.5f);
                    if (a <= 0.01f)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    Color32 c;
                    if (d > r * 0.84f)
                        c = rim;
                    else if (d > r * 0.72f)
                        c = gold;
                    else
                        c = face;

                    var shineCenter = center + new Vector2(-r * 0.22f, r * 0.28f);
                    float sd = Vector2.Distance(p, shineCenter);
                    if (sd < r * 0.22f)
                        c = (Color32)Color.Lerp(c, shine, 0.55f);

                    c.a = (byte)Mathf.RoundToInt(255f * a);
                    pixels[y * size + x] = c;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        static Sprite BuildStarSprite(int size)
        {
            var texture = NewTexture(size);
            var pixels = new Color32[size * size];
            float cx = size / 2f;
            float cy = size / 2f;
            var poly = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float ang = -Mathf.PI / 2f + i * Mathf.PI / 5f;
                float rad = (i % 2 == 0) ? size * 0.46f : size * 0.18f;
                poly[i] = new Vector2(cx + Mathf.Cos(ang) * rad, cy + Mathf.Sin(ang) * rad);
            }

            var fill = new Color32(255, 214, 40, 255);
            var edge = new Color32(180, 90, 12, 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (!PointInPolygon(p, poly))
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    bool border = false;
                    for (int oy = -2; oy <= 2 && !border; oy++)
                    {
                        for (int ox = -2; ox <= 2; ox++)
                        {
                            if (!PointInPolygon(p + new Vector2(ox, oy), poly))
                            {
                                border = true;
                                break;
                            }
                        }
                    }

                    pixels[y * size + x] = border ? edge : fill;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        static bool PointInPolygon(Vector2 point, Vector2[] poly)
        {
            bool inside = false;
            int j = poly.Length - 1;
            for (int i = 0; i < poly.Length; i++)
            {
                if ((poly[i].y > point.y) != (poly[j].y > point.y))
                {
                    float den = poly[j].y - poly[i].y;
                    if (Mathf.Abs(den) < 0.0001f)
                        den = 0.0001f;
                    float x = (poly[j].x - poly[i].x) * (point.y - poly[i].y) / den + poly[i].x;
                    if (point.x < x)
                        inside = !inside;
                }
                j = i;
            }
            return inside;
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

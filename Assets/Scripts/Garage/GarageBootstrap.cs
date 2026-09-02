using MeteGame.Core;
using MeteGame.UI;
using MeteGame.Vehicle;
using UnityEngine;
using UnityEngine.UI;

namespace MeteGame.Garage
{
    /// <summary>
    /// Garaj sahnesi: podyumda dönen araç, satın al / seç / boya.
    /// Sahne dosyası yoksa SceneFlow burayı yerinde de kurar.
    /// </summary>
    public class GarageBootstrap : MonoBehaviour
    {
        int _index;
        GameObject _preview;
        Transform _podium;
        Text _nameText;
        Text _blurbText;
        Text _walletText;
        Text _actionLabel;
        Image _actionImage;
        Text _toastText;
        Image[] _swatches;
        Image[] _swatchLocks;
        float _toastTimer;

        void Awake()
        {
            Application.targetFrameRate = 60;
            GarageShop.Normalize(SaveManager.Data);
            _index = VehicleCatalog.IndexOf(SaveManager.Data.selectedVehicleId);
            BuildWorld();
            BuildUi();
            RefreshPreview();
            RefreshUi();
        }

        void BuildWorld()
        {
            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -30f, 0f);

            var camGo = new GameObject("GarageCamera");
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.55f, 0.78f, 0.92f);
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 80f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.SetPositionAndRotation(
                new Vector3(5.6f, 3.4f, -7.2f),
                Quaternion.Euler(18f, -38f, 0f));

            PartFactory.Create(PrimitiveType.Cube, "Floor", transform,
                new Vector3(0f, -0.05f, 0f), new Vector3(40f, 0.1f, 40f),
                new Color(0.78f, 0.8f, 0.82f), castShadows: false);
            PartFactory.Create(PrimitiveType.Cylinder, "Podium", transform,
                new Vector3(0f, 0.18f, 0f), new Vector3(5.2f, 0.18f, 5.2f),
                new Color(0.92f, 0.78f, 0.28f));
            _podium = new GameObject("PreviewRoot").transform;
            _podium.SetParent(transform, false);
            _podium.localPosition = new Vector3(0f, 0.45f, 0f);
        }

        void BuildUi()
        {
            var canvas = UIFactory.CreateCanvas("GarageHUD");
            var root = canvas.transform;

            UIFactory.CreateText("Title", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -70f), new Vector2(800f, 90f),
                "GARAJ", 72, GameConfig.Gold);

            _walletText = UIFactory.CreateText("Wallet", root,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(280f, -70f), new Vector2(480f, 70f),
                "", 40, Color.white, TextAnchor.MiddleLeft);

            _nameText = UIFactory.CreateText("Name", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -160f), new Vector2(1200f, 80f),
                "", 56, Color.white);

            _blurbText = UIFactory.CreateText("Blurb", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -230f), new Vector2(1400f, 60f),
                "", 34, new Color(1f, 1f, 1f, 0.9f));

            var prev = UIFactory.CreateIcon("Prev", root,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(110f, 20f), new Vector2(130f, 130f),
                UIFactory.ChevronSprite, new Color(1f, 1f, 1f, 0.85f));
            prev.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
            var prevBtn = prev.gameObject.AddComponent<Button>();
            prevBtn.targetGraphic = prev;
            prevBtn.onClick.AddListener(() => Step(-1));

            var next = UIFactory.CreateIcon("Next", root,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-110f, 20f), new Vector2(130f, 130f),
                UIFactory.ChevronSprite, new Color(1f, 1f, 1f, 0.85f));
            next.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
            var nextBtn = next.gameObject.AddComponent<Button>();
            nextBtn.targetGraphic = next;
            nextBtn.onClick.AddListener(() => Step(1));

            var action = UIFactory.CreateButton("Action", root,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 210f), new Vector2(520f, 130f),
                "SEÇ", 52, new Color(0.24f, 0.72f, 0.34f), OnAction);
            _actionImage = action.GetComponent<Image>();
            _actionLabel = action.GetComponentInChildren<Text>();

            BuildSwatches(root);

            UIFactory.CreateButton("Back", root,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(160f, 80f), new Vector2(240f, 90f),
                "GERİ", 40, new Color(0.35f, 0.4f, 0.5f), SceneFlow.LeaveGarage);

            UIFactory.CreateButton("Drive", root,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-200f, 80f), new Vector2(280f, 90f),
                "SÜR!", 42, new Color(0.24f, 0.62f, 0.85f), SceneFlow.OpenCity);

            _toastText = UIFactory.CreateText("Toast", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -280f), new Vector2(1400f, 80f),
                "", 42, GameConfig.Gold);
        }

        void BuildSwatches(Transform root)
        {
            _swatches = new Image[GameConfig.CarPalette.Length];
            _swatchLocks = new Image[GameConfig.CarPalette.Length];
            float startX = -(GameConfig.CarPalette.Length - 1) * 0.5f * 92f;
            for (int i = 0; i < GameConfig.CarPalette.Length; i++)
            {
                int colorIndex = i;
                var swatch = UIFactory.CreateIcon("Paint" + i, root,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(startX + i * 92f, 360f), new Vector2(78f, 78f),
                    UIFactory.CircleSprite, GameConfig.CarPalette[i]);
                var button = swatch.gameObject.AddComponent<Button>();
                button.targetGraphic = swatch;
                button.onClick.AddListener(() => OnPaint(colorIndex));
                _swatches[i] = swatch;

                var ring = UIFactory.CreateIcon("Lock" + i, swatch.transform,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(28f, 28f),
                    UIFactory.CircleSprite, new Color(0f, 0f, 0f, 0.55f));
                ring.raycastTarget = false;
                _swatchLocks[i] = ring;
            }
        }

        void Step(int delta)
        {
            _index = (_index + delta + VehicleCatalog.All.Length) % VehicleCatalog.All.Length;
            RefreshPreview();
            RefreshUi();
        }

        void OnAction()
        {
            var def = VehicleCatalog.All[_index];
            string message;
            bool ok;
            if (!GarageShop.IsUnlocked(def.Id))
                ok = GarageShop.TryBuyVehicle(def.Id, out message);
            else if (SaveManager.Data.selectedVehicleId == def.Id)
            {
                message = "Zaten seçili";
                ok = true;
            }
            else
                ok = GarageShop.TrySelect(def.Id, out message);

            if (ok)
            {
                Sfx.Success(_podium.position);
                RefreshPreview();
            }
            else
                Sfx.Go(_podium.position);
            Toast(message);
            RefreshUi();
        }

        void OnPaint(int colorIndex)
        {
            var def = VehicleCatalog.All[_index];
            if (!GarageShop.IsUnlocked(def.Id))
            {
                Toast("Önce aracı al");
                return;
            }

            if (GarageShop.TryPaint(def.Id, colorIndex, out string message))
            {
                if (!string.IsNullOrEmpty(message))
                    Sfx.Ding(_podium.position);
                RefreshPreview();
                RefreshUi();
                if (!string.IsNullOrEmpty(message))
                    Toast(message);
            }
            else
            {
                Sfx.Go(_podium.position);
                Toast(message);
            }
        }

        void RefreshPreview()
        {
            if (_preview != null)
                Destroy(_preview);

            var def = VehicleCatalog.All[_index];
            bool locked = !GarageShop.IsUnlocked(def.Id);
            Color color = GarageShop.BodyColor(def.Id, locked);
            _preview = VehicleFactory.CreatePreview(def, color, _podium);
            _preview.transform.localRotation = Quaternion.identity;
        }

        void RefreshUi()
        {
            var def = VehicleCatalog.All[_index];
            var data = SaveManager.Data;
            bool unlocked = GarageShop.IsUnlocked(def.Id);
            bool selected = unlocked && data.selectedVehicleId == def.Id;

            _nameText.text = def.Name;
            _walletText.text = "Altın: " + data.coins;
            if (unlocked)
            {
                _blurbText.text = def.Blurb;
                if (selected)
                {
                    _actionLabel.text = "SEÇİLİ";
                    _actionImage.color = new Color(0.45f, 0.5f, 0.55f);
                }
                else
                {
                    _actionLabel.text = "SEÇ";
                    _actionImage.color = new Color(0.24f, 0.72f, 0.34f);
                }
            }
            else
            {
                _blurbText.text = def.Blurb + "   •   " + def.Price + " altın";
                _actionLabel.text = data.coins >= def.Price ? "SATIN AL  " + def.Price : "ALTIN YETMİYOR";
                _actionImage.color = data.coins >= def.Price
                    ? new Color(0.95f, 0.55f, 0.18f)
                    : new Color(0.45f, 0.35f, 0.3f);
            }

            int currentColor = GarageShop.ColorIndex(def.Id);
            for (int i = 0; i < _swatches.Length; i++)
            {
                bool paintOn = unlocked && GarageShop.IsPaintUnlocked(def.Id, i);
                _swatchLocks[i].gameObject.SetActive(!paintOn);
                _swatches[i].transform.localScale = Vector3.one * (unlocked && currentColor == i ? 1.18f : 1f);
                var c = GameConfig.CarPalette[i];
                if (!unlocked)
                    c = Color.Lerp(c, Color.gray, 0.55f);
                _swatches[i].color = c;
            }
        }

        void Toast(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            _toastText.text = message;
            _toastTimer = 1.6f;
        }

        void Update()
        {
            if (_podium != null)
                _podium.Rotate(0f, 22f * Time.deltaTime, 0f);

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f)
                    _toastText.text = "";
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                Step(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                Step(1);
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneFlow.LeaveGarage();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                OnAction();
        }
    }
}

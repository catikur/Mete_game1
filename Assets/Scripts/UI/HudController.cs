using System.Collections;
using MeteGame.Controls;
using MeteGame.Core;
using MeteGame.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Oyun içi arayüz: sayaçlar, görev oku, iki aşamalı süre,
    /// sol gaz/geri/bip, sağ yön joystick'i, görev teklifi ve kutlama.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        Transform _vehicle;

        Text _coinText;
        Text _starText;
        Text _dailyText;
        Text _comboText;
        Text _missionText;
        Text _distanceText;
        Text _celebrationText;
        RectTransform _arrow;
        RectTransform _arrowShadow;
        Text _hintText;
        float _hintTimer = 10f;

        GameObject _offerPanel;
        Text _offerTitle;
        Text _offerDescription;
        Text _offerMeta;
        Text _offerReward;
        System.Action _pendingStart;

        GameObject _timerPanel;
        Text _timerPhase;
        Text _timerTime;
        Text _timerOver;
        Image _dot1;
        Image _dot2;
        bool _timerPulse;

        Coroutine _toast;

        Vector3? _target;
        System.Action _onGarage;

        public void SetGarageHandler(System.Action onGarage) => _onGarage = onGarage;

        static readonly Color DotOn = new Color(1f, 0.92f, 0.25f, 1f);
        static readonly Color DotOff = new Color(1f, 1f, 1f, 0.28f);

        public static HudController Build(Transform vehicle)
        {
            var canvas = UIFactory.CreateCanvas("HUD");
            var hud = canvas.gameObject.AddComponent<HudController>();
            hud._vehicle = vehicle;
            hud.BuildWidgets(canvas.transform);
            return hud;
        }

        void BuildWidgets(Transform root)
        {
            BuildTopBar(root);
            BuildTargetIndicator(root);
            BuildTimer(root);
            BuildDriveButtons(root);
            BuildOfferPanel(root);
            BuildCelebration(root);
        }

        void BuildTopBar(Transform root)
        {
            var panel = UIFactory.CreatePanel("CurrencyPanel", root,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(390f, -72f), new Vector2(740f, 110f),
                new Color(0f, 0f, 0f, 0.42f));
            panel.raycastTarget = false;

            _coinText = BuildStatChip(panel.transform, -175f, UIFactory.CoinSprite, "ALTIN");
            _starText = BuildStatChip(panel.transform, 175f, UIFactory.StarSprite, "YILDIZ");

            _missionText = UIFactory.CreateText("MissionText", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -148f), new Vector2(1200f, 74f),
                "", 46, Color.white);

            _dailyText = UIFactory.CreateText("DailyText", root,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-240f, -56f), new Vector2(440f, 58f),
                "", 32, Color.white);

            _comboText = UIFactory.CreateText("ComboText", root,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-240f, -108f), new Vector2(440f, 48f),
                "", 30, GameConfig.Gold);
            _comboText.gameObject.SetActive(false);

            UIFactory.CreateButton("GarageButton", root,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-240f, -172f), new Vector2(220f, 64f),
                "GARAJ", 32, new Color(0.95f, 0.55f, 0.18f), OnGarageClicked);
        }

        static Text BuildStatChip(Transform parent, float x, Sprite icon, string label)
        {
            var iconImage = UIFactory.CreateIcon(label + "Icon", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x - 110f, 0f), new Vector2(64f, 64f),
                icon, Color.white);
            iconImage.raycastTarget = false;

            UIFactory.CreateText(label + "Caption", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x + 8f, 22f), new Vector2(180f, 32f),
                label, 22, new Color(1f, 0.92f, 0.55f), TextAnchor.MiddleLeft);

            return UIFactory.CreateText(label + "Value", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x + 8f, -18f), new Vector2(180f, 48f),
                "0", 44, Color.white, TextAnchor.MiddleLeft);
        }

        void BuildTargetIndicator(Transform root)
        {
            var shadow = UIFactory.CreateIcon("TargetArrowShadow", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(6f, -258f), new Vector2(150f, 150f),
                UIFactory.ChevronSprite, new Color(0f, 0f, 0f, 0.85f));
            shadow.raycastTarget = false;
            _arrowShadow = shadow.rectTransform;

            var arrow = UIFactory.CreateIcon("TargetArrow", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -250f), new Vector2(140f, 140f),
                UIFactory.ChevronSprite, new Color(1f, 0.92f, 0.15f));
            arrow.raycastTarget = false;
            _arrow = arrow.rectTransform;

            _distanceText = UIFactory.CreateText("DistanceText", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -348f), new Vector2(400f, 70f),
                "", 52, new Color(1f, 0.95f, 0.25f));

            _arrow.gameObject.SetActive(false);
            _arrowShadow.gameObject.SetActive(false);
            _distanceText.gameObject.SetActive(false);
        }

        void BuildTimer(Transform root)
        {
            var panel = UIFactory.CreatePanel("TimerPanel", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -440f), new Vector2(620f, 96f),
                new Color(0f, 0f, 0f, 0.55f));
            panel.raycastTarget = false;
            _timerPanel = panel.gameObject;

            _timerPhase = UIFactory.CreateText("TimerPhase", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(110f, 10f), new Vector2(200f, 48f),
                "AL", 34, Color.white);

            _dot1 = UIFactory.CreateIcon("Dot1", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(70f, -26f), new Vector2(22f, 22f),
                UIFactory.CircleSprite, DotOn);
            _dot1.raycastTarget = false;

            _dot2 = UIFactory.CreateIcon("Dot2", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(100f, -26f), new Vector2(22f, 22f),
                UIFactory.CircleSprite, DotOff);
            _dot2.raycastTarget = false;

            _timerTime = UIFactory.CreateText("TimerTime", panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(50f, 0f), new Vector2(280f, 80f),
                "0:00", 62, Color.white);

            _timerOver = UIFactory.CreateText("TimerOver", panel.transform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-70f, 0f), new Vector2(110f, 56f),
                "GEÇ", 34, new Color(1f, 0.45f, 0.4f));
            _timerOver.gameObject.SetActive(false);

            _timerPanel.SetActive(false);
        }

        void BuildDriveButtons(Transform root)
        {
            _hintText = UIFactory.CreateText("Hint", root,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 430f), new Vector2(1500f, 70f),
                "Sol: gaz / geri / bip    •    Sağ: yön", 36,
                new Color(1f, 1f, 1f, 0.9f));

            // Sol alt küme: GERİ + BİP üstte, büyük GAZ altta.
            var reverse = UIFactory.CreateIcon("ReverseButton", root,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(108f, 360f), new Vector2(150f, 150f),
                UIFactory.CircleSprite, new Color(0.95f, 0.35f, 0.3f, 0.58f));
            UIFactory.CreateText("ReverseLabel", reverse.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "GERİ", 32, Color.white);
            var reverseHold = reverse.gameObject.AddComponent<HoldButton>();
            reverseHold.StateChanged = pressed => DriveInput.TouchReverse = pressed;

            var honk = UIFactory.CreateIcon("HonkButton", root,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(278f, 360f), new Vector2(150f, 150f),
                UIFactory.CircleSprite, new Color(1f, 0.82f, 0.2f, 0.72f));
            UIFactory.CreateText("HonkLabel", honk.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "BİP", 34, new Color(0.2f, 0.15f, 0.05f));
            var honkHold = honk.gameObject.AddComponent<HoldButton>();
            honkHold.StateChanged = pressed => DriveInput.HonkHeld = pressed;

            var gas = UIFactory.CreateIcon("GasButton", root,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(192f, 158f), new Vector2(260f, 260f),
                UIFactory.CircleSprite, new Color(0.24f, 0.78f, 0.38f, 0.62f));
            UIFactory.CreateText("GasLabel", gas.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "GAZ", 56, Color.white);
            var gasHold = gas.gameObject.AddComponent<HoldButton>();
            gasHold.StateChanged = pressed => DriveInput.TouchThrottle = pressed;

            SteerJoystick.Build(root);
        }

        void BuildOfferPanel(Transform root)
        {
            var panel = UIFactory.CreatePanel("OfferPanel", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(960f, 680f),
                new Color(0.12f, 0.16f, 0.24f, 0.93f));
            _offerPanel = panel.gameObject;

            _offerTitle = UIFactory.CreateText("OfferTitle", panel.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -80f), new Vector2(880f, 90f),
                "", 58, new Color(1f, 0.85f, 0.2f));

            _offerDescription = UIFactory.CreateText("OfferDescription", panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 80f), new Vector2(880f, 110f),
                "", 40, Color.white);
            _offerDescription.horizontalOverflow = HorizontalWrapMode.Wrap;

            _offerMeta = UIFactory.CreateText("OfferMeta", panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(880f, 80f),
                "", 36, Color.white);

            _offerReward = UIFactory.CreateText("OfferReward", panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -95f), new Vector2(880f, 70f),
                "", 34, GameConfig.Gold);

            UIFactory.CreateButton("StartButton", panel.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 110f), new Vector2(430f, 130f),
                "BAŞLA!", 58, new Color(0.24f, 0.72f, 0.34f), OnStartClicked);

            _offerPanel.SetActive(false);
        }

        void BuildCelebration(Transform root)
        {
            _celebrationText = UIFactory.CreateText("Celebration", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 130f), new Vector2(1600f, 160f),
                "", 72, GameConfig.Gold);
            _celebrationText.gameObject.SetActive(false);
        }

        void OnStartClicked()
        {
            DriveInput.Locked = false;
            _offerPanel.SetActive(false);
            var start = _pendingStart;
            _pendingStart = null;
            start?.Invoke();
        }

        void OnGarageClicked() => _onGarage?.Invoke();

        void OnDisable()
        {
            DriveInput.Locked = false;
            DriveInput.ResetTouch();
        }

        void Update()
        {
            if (_hintText != null && _hintTimer > 0f)
            {
                _hintTimer -= Time.deltaTime;
                if (_hintTimer <= 0f)
                    _hintText.gameObject.SetActive(false);
            }

            bool hasTarget = _target.HasValue && _vehicle != null;
            float meters = 0f;
            if (hasTarget)
            {
                Vector3 direction = _target.Value - _vehicle.position;
                direction.y = 0f;
                meters = direction.magnitude;

                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                var euler = new Vector3(0f, 0f, -angle);
                _arrow.localEulerAngles = euler;
                _arrowShadow.localEulerAngles = euler;

                float near = meters < 18f ? 1.18f : 1f;
                float pulse = (1f + Mathf.Sin(Time.time * 5f) * 0.08f) * near;
                _arrow.localScale = Vector3.one * pulse;
                _arrowShadow.localScale = Vector3.one * pulse;
                _distanceText.text = meters < 12f
                    ? "HEMEN YANINDA!"
                    : Mathf.RoundToInt(meters) + " m";
            }

            if (_arrow.gameObject.activeSelf != hasTarget)
            {
                _arrow.gameObject.SetActive(hasTarget);
                _arrowShadow.gameObject.SetActive(hasTarget);
                _distanceText.gameObject.SetActive(hasTarget);
            }

            if (_timerPanel != null && _timerPanel.activeSelf)
            {
                float scale = _timerPulse ? 1f + Mathf.Sin(Time.time * 8f) * 0.05f : 1f;
                _timerPanel.transform.localScale = Vector3.one * scale;
            }
        }

        public void SetTarget(Vector3? target)
        {
            _target = target;
        }

        public void SetMissionText(string text) => _missionText.text = text;

        public void SetCurrency(int coins, int stars)
        {
            _coinText.text = coins.ToString();
            _starText.text = stars.ToString();
        }

        public void SetDailyProgress(int completed, int target)
        {
            _dailyText.text = completed >= target
                ? "Bugün görevler bitti!"
                : "Bugün  " + completed + " / " + target + "  görev";
        }

        public void SetCombo(int streak)
        {
            if (streak >= 2)
            {
                _comboText.text = "SERİ  ×" + streak;
                _comboText.gameObject.SetActive(true);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void ShowMissionOffer(Mission mission, System.Action onStart)
        {
            DriveInput.Locked = true;
            DriveInput.ResetTouch();
            HideTimer();
            _pendingStart = onStart;
            _offerTitle.text = mission.Title;
            _offerDescription.text = mission.PickupText + "  →  " + mission.DropoffText;
            _offerMeta.text = mission.DifficultyLabel
                              + "   •   AL " + MissionClock.Format(mission.PickupSeconds)
                              + "   •   TESLİM " + MissionClock.Format(mission.DropoffSeconds);
            _offerMeta.color = MissionClock.DifficultyColor(mission.Difficulty);
            _offerReward.text = "Ödül: " + mission.RewardCoins + " altın"
                                + "   •   Zamanında her durakta +1 yıldız";
            _offerPanel.SetActive(true);
        }

        public void SetTimer(string phaseLabel, int step, float remaining, float duration)
        {
            _timerPanel.SetActive(true);
            _timerPhase.text = phaseLabel;
            _timerTime.text = MissionClock.Format(remaining);
            _timerTime.color = MissionClock.Tint(remaining, duration);
            _timerOver.gameObject.SetActive(remaining < 0f);
            _timerPulse = MissionClock.IsUrgent(remaining, duration);
            _dot1.color = step >= 1 ? DotOn : DotOff;
            _dot2.color = step >= 2 ? DotOn : DotOff;
        }

        public void HideTimer()
        {
            if (_timerPanel != null)
            {
                _timerPanel.transform.localScale = Vector3.one;
                _timerPanel.SetActive(false);
            }
            _timerPulse = false;
        }

        public void ShowToast(string message)
        {
            PlayBanner(message, 1.35f);
        }

        public void ShowCelebration(int coins, int stars, bool perfect, int streak)
        {
            string message = "+" + coins + " ALTIN   +" + stars + " YILDIZ!";
            if (perfect && streak >= 2)
                message += "\nSERİ ×" + streak + "!";
            else if (perfect)
                message += "\nİKİSİ DE ZAMANINDA!";
            PlayBanner(message, 2.1f);
        }

        void PlayBanner(string message, float hold)
        {
            if (_toast != null)
                StopCoroutine(_toast);
            _toast = StartCoroutine(CelebrationRoutine(message, hold));
        }

        IEnumerator CelebrationRoutine(string message, float hold)
        {
            _celebrationText.text = message;
            _celebrationText.gameObject.SetActive(true);

            var rect = _celebrationText.rectTransform;
            float duration = 0.35f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                rect.localScale = Vector3.one * Mathf.LerpUnclamped(0.3f, 1f, t);
                yield return null;
            }
            rect.localScale = Vector3.one;

            yield return new WaitForSeconds(hold);
            _celebrationText.gameObject.SetActive(false);
            _toast = null;
        }
    }
}

using System.Collections;
using MeteGame.Controls;
using MeteGame.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Oyun içi arayüz: altın/yıldız sayaçları, görev metni, hedefe dönen ok,
    /// günlük ilerleme, direksiyon/geri butonları, görev teklifi ve kutlama.
    /// Tamamı kod ile kurulur.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        Transform _vehicle;

        Text _coinText;
        Text _starText;
        Text _dailyText;
        Text _missionText;
        Text _distanceText;
        Text _celebrationText;
        RectTransform _arrow;

        GameObject _offerPanel;
        Text _offerTitle;
        Text _offerDescription;
        Text _offerReward;
        System.Action _pendingStart;

        Vector3? _target;
        bool _leftHeld;
        bool _rightHeld;

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
            BuildDriveButtons(root);
            BuildOfferPanel(root);
            BuildCelebration(root);
        }

        void BuildTopBar(Transform root)
        {
            // Sol üst: altın ve yıldız
            var panel = UIFactory.CreatePanel("CurrencyPanel", root,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(220f, -65f), new Vector2(400f, 92f),
                new Color(0f, 0f, 0f, 0.35f));

            UIFactory.CreateIcon("CoinIcon", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(55f, 0f), new Vector2(52f, 52f),
                UIFactory.CircleSprite, Core.GameConfig.Gold);
            _coinText = UIFactory.CreateText("CoinText", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(135f, 0f), new Vector2(120f, 70f),
                "0", 44, Color.white, TextAnchor.MiddleLeft);

            // Yıldız ikonu: 45° dönmüş kare = pırlanta/yıldız hissi
            var star = UIFactory.CreatePanel("StarIcon", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(255f, 0f), new Vector2(42f, 42f),
                new Color(0.55f, 0.78f, 1f));
            star.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);
            _starText = UIFactory.CreateText("StarText", panel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(330f, 0f), new Vector2(120f, 70f),
                "0", 44, Color.white, TextAnchor.MiddleLeft);

            // Üst orta: görev metni
            _missionText = UIFactory.CreateText("MissionText", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -55f), new Vector2(1200f, 74f),
                "", 46, Color.white);

            // Sağ üst: günlük ilerleme
            _dailyText = UIFactory.CreateText("DailyText", root,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-190f, -65f), new Vector2(340f, 70f),
                "", 40, Color.white);
        }

        void BuildTargetIndicator(Transform root)
        {
            _arrow = UIFactory.CreateIcon("TargetArrow", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-70f, -145f), new Vector2(72f, 72f),
                UIFactory.TriangleSprite, new Color(1f, 0.85f, 0.2f)).rectTransform;

            _distanceText = UIFactory.CreateText("DistanceText", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(70f, -145f), new Vector2(240f, 64f),
                "", 42, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleLeft);

            _arrow.gameObject.SetActive(false);
            _distanceText.gameObject.SetActive(false);
        }

        void BuildDriveButtons(Transform root)
        {
            // Sol alt: sola dönüş
            var left = UIFactory.CreateIcon("SteerLeft", root,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(180f, 180f), new Vector2(250f, 250f),
                UIFactory.CircleSprite, new Color(1f, 1f, 1f, 0.32f));
            var leftIcon = UIFactory.CreateIcon("SteerLeftIcon", left.transform,
                Vector2.one * 0.5f, Vector2.one * 0.5f,
                Vector2.zero, new Vector2(110f, 110f),
                UIFactory.TriangleSprite, new Color(0.15f, 0.2f, 0.3f, 0.85f));
            leftIcon.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
            var leftHold = left.gameObject.AddComponent<HoldButton>();
            leftHold.StateChanged = pressed => { _leftHeld = pressed; PushSteer(); };

            // Sağ alt: sağa dönüş
            var right = UIFactory.CreateIcon("SteerRight", root,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-180f, 180f), new Vector2(250f, 250f),
                UIFactory.CircleSprite, new Color(1f, 1f, 1f, 0.32f));
            var rightIcon = UIFactory.CreateIcon("SteerRightIcon", right.transform,
                Vector2.one * 0.5f, Vector2.one * 0.5f,
                Vector2.zero, new Vector2(110f, 110f),
                UIFactory.TriangleSprite, new Color(0.15f, 0.2f, 0.3f, 0.85f));
            rightIcon.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
            var rightHold = right.gameObject.AddComponent<HoldButton>();
            rightHold.StateChanged = pressed => { _rightHeld = pressed; PushSteer(); };

            // Alt orta: geri vites
            var reverse = UIFactory.CreateIcon("ReverseButton", root,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 130f), new Vector2(190f, 190f),
                UIFactory.CircleSprite, new Color(0.95f, 0.35f, 0.3f, 0.55f));
            UIFactory.CreateText("ReverseLabel", reverse.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "GERİ", 44, Color.white);
            var reverseHold = reverse.gameObject.AddComponent<HoldButton>();
            reverseHold.StateChanged = pressed => DriveInput.TouchReverse = pressed;
        }

        void PushSteer()
        {
            DriveInput.TouchSteer = (_leftHeld ? -1f : 0f) + (_rightHeld ? 1f : 0f);
        }

        void BuildOfferPanel(Transform root)
        {
            var panel = UIFactory.CreatePanel("OfferPanel", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(920f, 570f),
                new Color(0.12f, 0.16f, 0.24f, 0.93f));
            _offerPanel = panel.gameObject;

            _offerTitle = UIFactory.CreateText("OfferTitle", panel.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -95f), new Vector2(840f, 95f),
                "", 64, new Color(1f, 0.85f, 0.2f));

            _offerDescription = UIFactory.CreateText("OfferDescription", panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f), new Vector2(840f, 130f),
                "", 46, Color.white);

            _offerReward = UIFactory.CreateText("OfferReward", panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -55f), new Vector2(840f, 70f),
                "", 40, Core.GameConfig.Gold);

            UIFactory.CreateButton("StartButton", panel.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 115f), new Vector2(430f, 135f),
                "BAŞLA!", 58, new Color(0.24f, 0.72f, 0.34f), OnStartClicked);

            _offerPanel.SetActive(false);
        }

        void BuildCelebration(Transform root)
        {
            _celebrationText = UIFactory.CreateText("Celebration", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 130f), new Vector2(1400f, 150f),
                "", 84, Core.GameConfig.Gold);
            _celebrationText.gameObject.SetActive(false);
        }

        void OnStartClicked()
        {
            _offerPanel.SetActive(false);
            var start = _pendingStart;
            _pendingStart = null;
            start?.Invoke();
        }

        void Update()
        {
            bool hasTarget = _target.HasValue && _vehicle != null;
            if (hasTarget)
            {
                Vector3 direction = _target.Value - _vehicle.position;
                direction.y = 0f;

                // Kamera kuzeyi sabit olduğu için ekran açısı doğrudan dünya açısıdır.
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                _arrow.localEulerAngles = new Vector3(0f, 0f, -angle);
                _distanceText.text = Mathf.RoundToInt(direction.magnitude) + " m";
            }

            if (_arrow.gameObject.activeSelf != hasTarget)
            {
                _arrow.gameObject.SetActive(hasTarget);
                _distanceText.gameObject.SetActive(hasTarget);
            }
        }

        public void SetTarget(Vector3? target) => _target = target;

        public void SetMissionText(string text) => _missionText.text = text;

        public void SetCurrency(int coins, int stars)
        {
            _coinText.text = coins.ToString();
            _starText.text = stars.ToString();
        }

        public void SetDailyProgress(int completed, int target)
        {
            _dailyText.text = completed >= target
                ? "Bonus görevler!"
                : "Görev " + completed + "/" + target;
        }

        public void ShowMissionOffer(Mission mission, System.Action onStart)
        {
            _pendingStart = onStart;
            _offerTitle.text = mission.Title;
            _offerDescription.text = mission.DropoffText;
            _offerReward.text = "Ödül: " + mission.RewardCoins + " altın"
                                + (mission.BonusSeconds > 0f ? "  •  Hızlı olursan +1 yıldız!" : "");
            _offerPanel.SetActive(true);
        }

        public void ShowCelebration(int coins, int stars)
        {
            string message = "+" + coins + " ALTIN   +" + stars + " YILDIZ!";
            StartCoroutine(CelebrationRoutine(message));
        }

        IEnumerator CelebrationRoutine(string message)
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

            yield return new WaitForSeconds(1.8f);
            _celebrationText.gameObject.SetActive(false);
        }
    }
}

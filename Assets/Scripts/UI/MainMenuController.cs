using MeteGame.Core;
using UnityEngine;

namespace MeteGame.UI
{
    /// <summary>Boot sahnesinin ana menüsü: başlık, OYNA butonu ve cüzdan özeti.</summary>
    public class MainMenuController : MonoBehaviour
    {
        void Start()
        {
            CreateMenuCamera();
            BuildMenu();
        }

        void CreateMenuCamera()
        {
            var go = new GameObject("MenuCamera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.42f, 0.75f, 0.94f); // Gökyüzü
            go.AddComponent<AudioListener>();
        }

        void BuildMenu()
        {
            var canvas = UIFactory.CreateCanvas("MenuCanvas");
            var root = canvas.transform;

            // Dekor: güneş ve alt tarafta yol şeridi
            UIFactory.CreateIcon("Sun", root,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-220f, -190f), new Vector2(220f, 220f),
                UIFactory.CircleSprite, new Color(1f, 0.9f, 0.45f));

            UIFactory.CreatePanel("RoadStrip", root,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 140f), new Vector2(0f, 280f),
                new Color(0.24f, 0.25f, 0.28f));
            for (int i = 0; i < 16; i++)
                UIFactory.CreatePanel("Dash" + i, root,
                    new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(90f + i * 175f, 140f), new Vector2(90f, 14f),
                    new Color(0.95f, 0.93f, 0.88f));

            UIFactory.CreateText("Title", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -200f), new Vector2(1700f, 170f),
                "METE'NİN OYUNU", 128, GameConfig.Gold);

            UIFactory.CreateText("Subtitle", root,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -320f), new Vector2(1500f, 76f),
                "Şehirde sür, görevleri tamamla, garajını doldur!", 46, Color.white);

            var data = SaveManager.Data;
            bool resume = data.city != null && data.city.active;
            UIFactory.CreateButton("PlayButton", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(470f, 155f),
                resume ? "DEVAM ET" : "OYNA!", 66, new Color(0.24f, 0.72f, 0.34f),
                SceneFlow.OpenCity);

            if (resume)
            {
                UIFactory.CreateText("ResumeHint", root,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 110f), new Vector2(900f, 48f),
                    "Kaldığın yerden devam", 34, new Color(1f, 0.95f, 0.7f));
            }

            UIFactory.CreateButton("GarageButton", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -200f), new Vector2(470f, 130f),
                "GARAJ", 58, new Color(0.95f, 0.55f, 0.18f),
                () => SceneFlow.OpenGarage("Boot"));

            string wallet = "Altın: " + data.coins + "      Yıldız: " + data.stars;
            if (data.bestStreak >= 2)
                wallet += "      Seri rekoru: ×" + data.bestStreak;
            UIFactory.CreateText("Wallet", root,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 80f), new Vector2(1400f, 64f),
                wallet, 42, Color.white);
        }
    }
}

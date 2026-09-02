using MeteGame.Controls;
using MeteGame.Garage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeteGame.Core
{
    /// <summary>Menü ↔ şehir ↔ garaj geçişi. Garaj sahnesi yoksa yerinde kurulur.</summary>
    public static class SceneFlow
    {
        public static string AfterGarage = "Boot";

        public static void OpenGarage(string returnScene)
        {
            AfterGarage = string.IsNullOrEmpty(returnScene) ? "Boot" : returnScene;
            ResumeTime();
            SaveManager.Save();
            DriveInputReset();
            if (ApplicationCanLoad("Garage"))
            {
                SceneManager.LoadScene("Garage");
                return;
            }

            SpawnGarageInPlace();
        }

        public static void LeaveGarage()
        {
            ResumeTime();
            SaveManager.Save();
            DriveInputReset();
            string target = string.IsNullOrEmpty(AfterGarage) ? "Boot" : AfterGarage;
            if (ApplicationCanLoad(target))
                SceneManager.LoadScene(target);
            else
                SceneManager.LoadScene("Boot");
        }

        public static void OpenCity()
        {
            ResumeTime();
            SaveManager.Save();
            DriveInputReset();
            SceneManager.LoadScene("City");
        }

        public static void OpenMenu()
        {
            ResumeTime();
            SaveManager.Save();
            DriveInputReset();
            SceneManager.LoadScene("Boot");
        }

        public static void ResumeTime()
        {
            Time.timeScale = 1f;
        }

        static bool ApplicationCanLoad(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).IsValid()
                   || ApplicationCanStream(sceneName);
        }

        static bool ApplicationCanStream(string sceneName)
        {
            try
            {
                return Application.CanStreamedLevelBeLoaded(sceneName);
            }
            catch
            {
                return false;
            }
        }

        static void DriveInputReset()
        {
            DriveInput.Locked = false;
            DriveInput.ResetTouch();
        }

        static void SpawnGarageInPlace()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                roots[i].SetActive(false);
                Object.Destroy(roots[i]);
            }

            var go = new GameObject("GarageRoot");
            go.AddComponent<GarageBootstrap>();
        }
    }
}

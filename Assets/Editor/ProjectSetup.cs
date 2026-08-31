using System.IO;
using MeteGame.Core;
using MeteGame.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MeteGame.EditorTools
{
    /// <summary>
    /// Proje ilk kez açıldığında sahneleri, URP ayarlarını ve oyuncu ayarlarını
    /// otomatik kurar. Böylece repo klonlanıp Unity ile açıldığında elle hiçbir
    /// kurulum yapmadan Play'e basılabilir.
    /// </summary>
    public static class ProjectSetup
    {
        const string ScenesDir = "Assets/Scenes";
        const string BootScenePath = ScenesDir + "/Boot.unity";
        const string CityScenePath = ScenesDir + "/City.unity";
        const string SettingsDir = "Assets/Settings";
        const string RendererPath = SettingsDir + "/MeteURPRenderer.asset";
        const string PipelinePath = SettingsDir + "/MeteURP.asset";
        const string MaterialsDir = "Assets/Resources/Materials";
        const string BaseMaterialPath = MaterialsDir + "/MeteLit.mat";

        [InitializeOnLoadMethod]
        static void AutoSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!IsSetupComplete())
                    RunSetup();
            };
        }

        [MenuItem("Mete Oyunu/Projeyi Kur (Setup)")]
        public static void RunSetupFromMenu()
        {
            RunSetup();
        }

        static bool IsSetupComplete()
        {
            return File.Exists(CityScenePath)
                   && File.Exists(BootScenePath)
                   && GraphicsSettings.defaultRenderPipeline != null;
        }

        static void RunSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EnsureFolders();
            SetupRenderPipeline();
            SetupBaseMaterial();
            SetupScenes();
            SetupBuildScenes();
            SetupPlayerSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (File.Exists(CityScenePath))
                EditorSceneManager.OpenScene(CityScenePath);

            Debug.Log("[Mete Oyunu] Kurulum tamam! City sahnesi açıldı — Play'e basıp sürebilirsin. " +
                      "Kontroller: Sol/Sağ ok (veya A/D) direksiyon, Aşağı ok/S/Boşluk geri vites.");
        }

        static void EnsureFolders()
        {
            Directory.CreateDirectory(ScenesDir);
            Directory.CreateDirectory(SettingsDir);
            Directory.CreateDirectory(MaterialsDir);
            AssetDatabase.Refresh();
        }

        static void SetupRenderPipeline()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            // Mobil için makul varsayılanlar.
            pipeline.supportsHDR = false;
            pipeline.shadowDistance = 80f;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        static void SetupBaseMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterialPath);
            if (material != null)
                return;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            material = new Material(shader) { color = Color.white };
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.18f);

            AssetDatabase.CreateAsset(material, BaseMaterialPath);
        }

        static void SetupScenes()
        {
            if (!File.Exists(BootScenePath))
            {
                var bootScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var menuRoot = new GameObject("MainMenu");
                menuRoot.AddComponent<MainMenuController>();
                EditorSceneManager.SaveScene(bootScene, BootScenePath);
            }

            if (!File.Exists(CityScenePath))
            {
                var cityScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var gameRoot = new GameObject("GameRoot");
                gameRoot.AddComponent<GameBootstrap>();
                EditorSceneManager.SaveScene(cityScene, CityScenePath);
            }
        }

        static void SetupBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(CityScenePath, true)
            };
        }

        static void SetupPlayerSettings()
        {
            PlayerSettings.companyName = "Mete Games";
            PlayerSettings.productName = "Mete'nin Oyunu";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;

            // Yatay (landscape) yönelim — sürüş oyunu için doğru format.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            try
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.metegames.metenoyunu");
                PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            }
            catch (System.Exception e)
            {
                // iOS Build Support kurulu değilse burada sorun çıkabilir; oyun editörde yine de çalışır.
                Debug.LogWarning("[Mete Oyunu] iOS ayarları uygulanamadı (iOS Build Support kurulu mu?): " + e.Message);
            }
        }
    }
}

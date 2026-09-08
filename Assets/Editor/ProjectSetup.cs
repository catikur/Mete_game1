using System.IO;
using MeteGame.Core;
using MeteGame.Garage;
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
        const string GarageScenePath = ScenesDir + "/Garage.unity";
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
                   && File.Exists(GarageScenePath)
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
            UpgradeKenneyMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (File.Exists(CityScenePath))
                EditorSceneManager.OpenScene(CityScenePath);

            Debug.Log("[Mete Oyunu] Kurulum tamam! City sahnesi açıldı — Play'e basıp sürebilirsin. " +
                      "Kontroller: WASD/oklar = joystick yönü, H korna, Esc menü.");
        }

        [MenuItem("Mete Oyunu/Kenney Materyallerini URP'ye Çevir")]
        public static void UpgradeKenneyMaterialsFromMenu()
        {
            UpgradeKenneyMaterials();
            AssetDatabase.SaveAssets();
            Debug.Log("[Mete Oyunu] Kenney materyalleri URP Lit olarak güncellendi.");
        }

        /// <summary>
        /// Kenney Standard/pembe materyallerini URP Lit'e çevirir; colormap'i _BaseMap'e taşır.
        /// Import sonrası ve kurulum menüsünden çağrılır.
        /// </summary>
        public static void UpgradeKenneyMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                return;

            string[] folders = { "Assets/Resources/Kenney" };
            if (!AssetDatabase.IsValidFolder(folders[0]))
                return;

            string[] materialGuids = AssetDatabase.FindAssets("t:Material", folders);
            int converted = 0;
            for (int i = 0; i < materialGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                    continue;

                Texture tex = material.mainTexture;
                if (tex == null && material.HasProperty("_MainTex"))
                    tex = material.GetTexture("_MainTex");
                if (tex == null && material.HasProperty("_BaseMap"))
                    tex = material.GetTexture("_BaseMap");

                bool needsShader = material.shader == null
                                    || material.shader.name == "Hidden/InternalErrorShader"
                                    || material.shader.name == "Standard"
                                    || material.shader.name.StartsWith("Legacy")
                                    || !material.shader.name.Contains("Universal Render Pipeline");

                bool dirty = false;
                if (needsShader)
                {
                    material.shader = shader;
                    converted++;
                    dirty = true;
                }

                if (tex != null && material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != tex)
                {
                    material.SetTexture("_BaseMap", tex);
                    converted++;
                    dirty = true;
                }

                if (material.HasProperty("_Smoothness") && !Mathf.Approximately(material.GetFloat("_Smoothness"), 0.22f))
                {
                    material.SetFloat("_Smoothness", 0.22f);
                    dirty = true;
                }

                if (!material.enableInstancing)
                {
                    material.enableInstancing = true;
                    dirty = true;
                }

                if (dirty)
                    EditorUtility.SetDirty(material);
            }

            if (converted > 0)
                Debug.Log("[Mete Oyunu] " + converted + " Kenney materyali URP Lit yapıldı.");
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

            if (!File.Exists(GarageScenePath))
            {
                var garageScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var garageRoot = new GameObject("GarageRoot");
                garageRoot.AddComponent<GarageBootstrap>();
                EditorSceneManager.SaveScene(garageScene, GarageScenePath);
            }
        }

        static void SetupBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(CityScenePath, true),
                new EditorBuildSettingsScene(GarageScenePath, true)
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

using UnityEditor;
using UnityEngine;

namespace MeteGame.EditorTools
{
    /// <summary>
    /// Kenney FBX/PNG import ayarları: collider yok, ölçek 1, atlas nokta örnekleme,
    /// materyaller URP Lit. Unity Mac'te ilk kez içe aktarınca çalışır.
    /// </summary>
    public sealed class KenneyAssetPostprocessor : AssetPostprocessor
    {
        static bool IsKenney(string path)
        {
            return path.Replace('\\', '/').Contains("/Resources/Kenney/");
        }

        void OnPreprocessModel()
        {
            if (!IsKenney(assetPath))
                return;

            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.addCollider = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.External;
            importer.materialName = ModelImporterMaterialName.BasedOnTextureName;
        }

        void OnPreprocessTexture()
        {
            if (!IsKenney(assetPath))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
        }

        Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            if (!IsKenney(assetPath))
                return null;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                return null;

            var mat = new Material(shader);
            Texture tex = material != null ? material.mainTexture : null;
            if (tex == null && material != null && material.HasProperty("_MainTex"))
                tex = material.GetTexture("_MainTex");
            if (tex != null && mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.22f);
            mat.enableInstancing = true;
            return mat;
        }

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool kenney = false;
            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (IsKenneyStatic(importedAssets[i]))
                {
                    kenney = true;
                    break;
                }
            }

            if (kenney)
            {
                EditorApplication.delayCall -= ProjectSetup.UpgradeKenneyMaterials;
                EditorApplication.delayCall += ProjectSetup.UpgradeKenneyMaterials;
            }
        }

        static bool IsKenneyStatic(string path)
        {
            return path.Replace('\\', '/').Contains("/Resources/Kenney/");
        }
    }
}

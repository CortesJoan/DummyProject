#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AnimalMemory.Editor
{
    public static class MementoDuelSheetImporter
    {
        private const string SourceFolder =
            "Assets/AnimalMemory/Art/Opponents/Source/MementoDuelSheet";
        private const string TargetAsset =
            "Assets/AnimalMemory/Art/Opponents/memento-duel-opponents-sheet-4k-v1.png";
        private const string RuntimeAsset =
            "Assets/AnimalMemory/Resources/AnimalMemory/Duel/memento-duel-opponents-sheet-4k-v1.png";

        private static readonly string[] SpriteNames =
        {
            "mika_neutral",
            "mika_thinking",
            "mika_ability",
            "mika_defeated",
            "yoru_neutral",
            "yoru_thinking",
            "yoru_ability",
            "yoru_defeated"
        };

        [MenuItem("Tools/Memento Match/Rebuild Duel Sprite Sheet")]
        public static void Rebuild()
        {
            string sourceAbsolute = ToAbsolutePath(SourceFolder);
            string[] parts = Directory
                .GetFiles(sourceAbsolute, "part_*.txt", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            if (parts.Length != 29)
                throw new InvalidOperationException(
                    $"Expected 29 Memento duel sheet fragments, found {parts.Length}.");

            StringBuilder encoded = new StringBuilder(8_700_000);
            foreach (string part in parts)
                encoded.Append(File.ReadAllText(part).Trim());

            byte[] png = Convert.FromBase64String(encoded.ToString());
            string targetAbsolute = ToAbsolutePath(TargetAsset);
            Directory.CreateDirectory(Path.GetDirectoryName(targetAbsolute) ??
                throw new InvalidOperationException("Target folder is invalid."));
            File.WriteAllBytes(targetAbsolute, png);

            AssetDatabase.ImportAsset(
                TargetAsset,
                ImportAssetOptions.ForceSynchronousImport |
                ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(TargetAsset)
                as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException(
                    $"Unity did not create a TextureImporter for {TargetAsset}.");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
#pragma warning disable CS0618
            importer.spritesheet = BuildSpriteMetadata();
#pragma warning restore CS0618

            ConfigureMobile(importer, "Android", TextureImporterFormat.ETC2_RGBA8);
            ConfigureMobile(importer, "iPhone", TextureImporterFormat.ASTC_6x6);
            importer.SaveAndReimport();

            string runtimeFolder = Path.GetDirectoryName(ToAbsolutePath(RuntimeAsset)) ??
                throw new InvalidOperationException("Runtime duel folder is invalid.");
            Directory.CreateDirectory(runtimeFolder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (AssetDatabase.LoadMainAssetAtPath(RuntimeAsset) != null)
                AssetDatabase.DeleteAsset(RuntimeAsset);
            if (!AssetDatabase.CopyAsset(TargetAsset, RuntimeAsset))
                throw new InvalidOperationException("Could not create the runtime duel sheet copy.");
            AssetDatabase.ImportAsset(RuntimeAsset, ImportAssetOptions.ForceSynchronousImport);

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetAsset);
            if (texture == null || texture.width != 3840 || texture.height != 2160)
                throw new InvalidOperationException(
                    "Imported duel sheet dimensions are not 3840x2160.");

            UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(TargetAsset);
            int spriteCount = sprites.Count(asset => asset is Sprite);
            if (spriteCount != SpriteNames.Length)
                throw new InvalidOperationException(
                    $"Expected {SpriteNames.Length} sprites, imported {spriteCount}.");

            Debug.Log(
                $"Memento Match duel sheet rebuilt: {texture.width}x{texture.height}, " +
                $"{spriteCount} sprites, {png.Length} PNG bytes.");
        }

        private static SpriteMetaData[] BuildSpriteMetadata()
        {
            SpriteMetaData[] metadata = new SpriteMetaData[SpriteNames.Length];
            for (int index = 0; index < metadata.Length; index++)
            {
                int column = index % 4;
                bool mikaRow = index < 4;
                metadata[index] = new SpriteMetaData
                {
                    name = SpriteNames[index],
                    rect = new Rect(column * 960f, mikaRow ? 1080f : 0f, 960f, 1080f),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero
                };
            }

            return metadata;
        }

        private static void ConfigureMobile(
            TextureImporter importer,
            string platform,
            TextureImporterFormat format)
        {
            TextureImporterPlatformSettings settings =
                importer.GetPlatformTextureSettings(platform);
            settings.name = platform;
            settings.overridden = true;
            settings.maxTextureSize = 2048;
            settings.format = format;
            settings.textureCompression = TextureImporterCompression.CompressedHQ;
            settings.compressionQuality = 100;
            importer.SetPlatformTextureSettings(settings);
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath) ??
                throw new InvalidOperationException("Unity project root is unavailable.");
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
#endif

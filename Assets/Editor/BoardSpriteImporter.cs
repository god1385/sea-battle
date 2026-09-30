using UnityEditor;
using UnityEngine;

public static class BoardSpriteImporter
{
    [InitializeOnLoadMethod]
    private static void ImportAsSprites()
    {
        const string folder = "Assets/_Art/Resources/Sprites";
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        for (var i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType == TextureImporterType.Sprite
                && importer.alphaIsTransparency
                && !importer.mipmapEnabled)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}

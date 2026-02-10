using UnityEngine;
using UnityEditor;

public class GetSpriteID
{
    [MenuItem("Tools/FixAndGetID")]
    public static void Run()
    {
        string path = "Assets/SectorCircle.png";
        
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
        
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null)
            Debug.Log("SpriteID: " + s.GetInstanceID());
        else
            Debug.LogError("Failed to load Sprite after reimport.");
    }
}
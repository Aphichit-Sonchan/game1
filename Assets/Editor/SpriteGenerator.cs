using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteGenerator
{
    [MenuItem("Tools/GenerateCircleSprite")]
    public static void GenerateCircle()
    {
        int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size/2f, size/2f);
        float radius = size/2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        byte[] bytes = texture.EncodeToPNG();
        string path = "Assets/SectorCircle.png";
        File.WriteAllBytes(path, bytes);
        
        AssetDatabase.Refresh();
        
        // Import as Sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
        
        Debug.Log("Created Circle Sprite at " + path);
    }
}
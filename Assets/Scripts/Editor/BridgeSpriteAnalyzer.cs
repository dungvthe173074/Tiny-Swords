#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BridgeSpriteAnalyzer
{
    [MenuItem("Tools/Auto Clean Bridge Slices")]
    public static void AutoClean()
    {
        string path = "Assets/Sprites/Tiny Swords (Update 010)/Terrain/Bridge/Bridge_All.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.isReadable = true;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;

        int width = tex.width;
        int height = tex.height;
        Color32[] pixels = tex.GetPixels32();

        Debug.Log($"[BridgeSpriteAnalyzer] Texture size: {width}x{height}");

        // Let's inspect the two main bridge regions:
        // 1. Horizontal Bridge (around the top region):
        // Current slice: x=17, y=164, w=158, h=92.
        // Let's find the exact bounding box of the horizontal bridge in y >= 170:
        int minX_H = width, maxX_H = 0, minY_H = height, maxY_H = 0;
        // In the top part of the texture (e.g. y from 185 to 255, x from 0 to 192):
        for (int y = 180; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 c = pixels[y * width + x];
                if (c.a > 10)
                {
                    if (x < minX_H) minX_H = x;
                    if (x > maxX_H) maxX_H = x;
                    if (y < minY_H) minY_H = y;
                    if (y > maxY_H) maxY_H = y;
                }
            }
        }
        Debug.Log($"[Horizontal Bridge Clean Box]: x={minX_H}, y={minY_H}, w={maxX_H - minX_H + 1}, h={maxY_H - minY_H + 1}");

        // 2. Vertical Bridge (left column, y from 0 to 180, x from 0 to 70):
        int minX_V = width, maxX_V = 0, minY_V = height, maxY_V = 0;
        for (int y = 0; y < 178; y++)
        {
            for (int x = 0; x < 65; x++) // Keep x < 65 to exclude the right artifacts at x > 65
            {
                Color32 c = pixels[y * width + x];
                if (c.a > 10)
                {
                    if (x < minX_V) minX_V = x;
                    if (x > maxX_V) maxX_V = x;
                    if (y < minY_V) minY_V = y;
                    if (y > maxY_V) maxY_V = y;
                }
            }
        }
        Debug.Log($"[Vertical Bridge Clean Box]: x={minX_V}, y={minY_V}, w={maxX_V - minX_V + 1}, h={maxY_V - minY_V + 1}");

        // Update Sprite Sheet slices in TextureImporter
        List<SpriteMetaData> newSheet = new List<SpriteMetaData>();

        // Clean Vertical Bridge
        SpriteMetaData vMeta = new SpriteMetaData();
        vMeta.name = "Bridge_All_0";
        vMeta.rect = new Rect(minX_V, minY_V, maxX_V - minX_V + 1, maxY_V - minY_V + 1);
        vMeta.alignment = (int)SpriteAlignment.Center;
        vMeta.pivot = new Vector2(0.5f, 0.5f);
        newSheet.Add(vMeta);

        // Clean Horizontal Bridge
        SpriteMetaData hMeta = new SpriteMetaData();
        hMeta.name = "Bridge_All_1";
        hMeta.rect = new Rect(minX_H, minY_H, maxX_H - minX_H + 1, maxY_H - minY_H + 1);
        hMeta.alignment = (int)SpriteAlignment.Center;
        hMeta.pivot = new Vector2(0.5f, 0.5f);
        newSheet.Add(hMeta);

        importer.spritesheet = newSheet.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("[BridgeSpriteAnalyzer] Bridge sprites cleaned! No more stray pixels or artifacts!");
    }
}
#endif

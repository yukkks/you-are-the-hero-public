using UnityEditor;
using UnityEngine;

// glTFast's editor importer leaves textures UNCOMPRESSED (raw RGBA32) — the 8
// Avaturn character GLBs alone put ~360 MB of raw textures into the WebGL build.
// This postprocessor DXT-compresses every texture sub-asset of a .glb right
// after import, editor and Cloud Build alike.
public class CompressGlbTextures : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (string path in imported)
        {
            if (!path.EndsWith(".glb")) continue;
            CompressAt(path);
        }
    }

    static void CompressAt(string path)
    {
        foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
        {
            if (obj is Texture2D tex && !GraphicsFormatIsCompressed(tex))
            {
                EditorUtility.CompressTexture(tex, TextureFormat.DXT5, TextureCompressionQuality.Best);
            }
        }
    }

    static bool GraphicsFormatIsCompressed(Texture2D tex)
    {
        switch (tex.format)
        {
            case TextureFormat.DXT1:
            case TextureFormat.DXT5:
            case TextureFormat.BC7:
                return true;
            default:
                return false;
        }
    }

    // Manual pass over the character GLBs + report, for verification.
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();
        foreach (string path in System.IO.Directory.GetFiles("Assets/Characters", "*.glb"))
        {
            long before = 0, after = 0;
            int count = 0;
            foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
            {
                if (obj is Texture2D tex)
                {
                    before += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
                    if (!GraphicsFormatIsCompressed(tex))
                        EditorUtility.CompressTexture(tex, TextureFormat.DXT5, TextureCompressionQuality.Best);
                    after += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
                    count++;
                }
            }
            sb.AppendLine($"{System.IO.Path.GetFileName(path)}: {count} textures, {before / (1024 * 1024)}MB -> {after / (1024 * 1024)}MB");
            foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
            {
                if (obj is Texture2D t && !GraphicsFormatIsCompressed(t))
                    sb.AppendLine($"   STILL RAW: {t.name} {t.width}x{t.height} {t.format}");
            }
        }
        return sb.ToString();
    }
}

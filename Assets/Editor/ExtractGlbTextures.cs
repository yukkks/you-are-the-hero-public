using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// glTFast's editor importer bakes UNCOMPRESSED textures into its import artifact,
// and the WebGL build inherits them raw (~360 MB for the 8 avatar GLBs; 90% of
// the build). There is no import setting for it and the importer is internal.
//
// Fix: extract every texture referenced by the character materials into real
// .png assets (which get the normal TextureImporter pipeline: size cap + DXT),
// clone the GLB sub-asset materials to standalone .mat assets pointing at the
// extracted textures, and swap all scene renderers under /Characters over.
// The GLBs' own texture sub-assets become unreferenced -> stripped from builds.
public static class ExtractGlbTextures
{
    const string OutDir = "Assets/CharactersTex";

    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();
        Directory.CreateDirectory(OutDir);

        // 1. Extract textures + clone materials for every character GLB.
        var matMap = new Dictionary<Material, Material>();   // glb sub-asset -> standalone clone
        var texMap = new Dictionary<Texture2D, Texture2D>(); // glb sub-asset -> extracted asset

        foreach (string glbPath in Directory.GetFiles("Assets/Characters", "*.glb"))
        {
            string modelName = Path.GetFileNameWithoutExtension(glbPath);
            string modelDir = $"{OutDir}/{modelName}";
            Directory.CreateDirectory(modelDir);

            foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(glbPath))
            {
                if (obj is Material srcMat)
                {
                    var clone = new Material(srcMat);
                    ReplaceTextures(clone, modelDir, texMap, sb);
                    string matPath = $"{modelDir}/{Sanitize(srcMat.name)}.mat";
                    AssetDatabase.CreateAsset(clone, matPath);
                    matMap[srcMat] = clone;
                }
            }
        }

        // 2. Standalone project materials (e.g. ShikhaDress.mat) may reference GLB
        // textures directly — retarget them in place.
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Characters" }))
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat != null) ReplaceTextures(mat, OutDir, texMap, sb);
        }

        // 3. Swap every renderer in the scene under /Characters to the clones.
        int swapped = 0;
        var charactersRoot = GameObject.Find("/Characters");
        if (charactersRoot == null) return "ERROR: /Characters root not found in scene";
        foreach (var r in charactersRoot.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            bool dirty = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && matMap.TryGetValue(mats[i], out var clone))
                {
                    mats[i] = clone;
                    dirty = true;
                }
            }
            if (dirty) { r.sharedMaterials = mats; swapped++; }
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        sb.AppendLine($"Materials cloned: {matMap.Count}, textures extracted: {texMap.Count}, renderers swapped: {swapped}");
        return sb.ToString();
    }

    static void ReplaceTextures(Material mat, string dir, Dictionary<Texture2D, Texture2D> texMap, System.Text.StringBuilder sb)
    {
        foreach (string prop in mat.GetTexturePropertyNames())
        {
            if (mat.GetTexture(prop) is Texture2D tex && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex))
                && AssetDatabase.GetAssetPath(tex).EndsWith(".glb"))
            {
                if (!texMap.TryGetValue(tex, out var extracted))
                {
                    bool isColor = prop.Contains("BaseColor") || prop.Contains("baseColor") || prop == "_MainTex" || prop.Contains("BaseMap");
                    extracted = ExtractToPng(tex, dir, isColor, sb);
                    texMap[tex] = extracted;
                }
                mat.SetTexture(prop, extracted);
            }
        }
    }

    static Texture2D ExtractToPng(Texture2D tex, string dir, bool isColor, System.Text.StringBuilder sb)
    {
        // Textures aren't CPU-readable — round-trip through a RenderTexture.
        var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32,
            isColor ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
        Graphics.Blit(tex, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var readable = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false,
            linear: !isColor);
        readable.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        readable.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{Sanitize(tex.name)}.png");
        File.WriteAllBytes(path, readable.EncodeToPNG());
        Object.DestroyImmediate(readable);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.sRGBTexture = isColor;
        importer.mipmapEnabled = true;
        importer.SaveAndReimport();

        sb.AppendLine($"  extracted {path} ({tex.width}x{tex.height}, {(isColor ? "sRGB" : "linear")})");
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return string.IsNullOrEmpty(name) ? "tex" : name;
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class PTLImporter : EditorWindow 
{
    [MenuItem("Rose Tools/PTL Importer (Dym 8x8 Additive)")]
    public static void ShowWindow() => GetWindow<PTLImporter>("PTL Pro Fix");

    private void OnGUI()
    {
        GUILayout.Label("Rose Online FX - Dym 8x8 Additive Shader", EditorStyles.boldLabel);
        if (GUILayout.Button("Buduj Efekt (Dym 8x8 Additive)", GUILayout.Height(50)))
        {
            string path = EditorUtility.OpenFilePanel("Wybierz plik PTL", "", "ptl");
            if (!string.IsNullOrEmpty(path)) ImportEffect(path);
        }
    }

    void ImportEffect(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        List<string> textures = FindTexturePaths(fileData);

        GameObject root = new GameObject("FX_Blacksmith_8x8_Additive");

        foreach (string tPath in textures)
        {
            string fileName = Path.GetFileNameWithoutExtension(tPath).ToLower();
            GameObject part = new GameObject("Layer_" + fileName);
            part.transform.SetParent(root.transform);
            ParticleSystem ps = part.AddComponent<ParticleSystem>();

            if (fileName.Contains("fire")) SetupFire(ps, fileName);
            else if (fileName.Contains("smoke")) SetupSmoke(ps, fileName);
            else SetupFire(ps, fileName);
        }

        // Warstwa Iskry star01
        GameObject sparks = new GameObject("Layer_sparks_star01");
        sparks.transform.SetParent(root.transform);
        SetupSparks(sparks.AddComponent<ParticleSystem>(), "star01");

        Selection.activeGameObject = root;
        Debug.Log("<color=lime>Import zakończony! Dym ustawiony na 8x8 i Additive Shader.</color>");
    }

    void SetupFire(ParticleSystem ps, string tex)
    {
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = 5.0f;
        main.startSize = 2.5f;
        main.gravityModifier = -0.3f;
        main.startColor = new Color(1f, 0.5f, 0.1f, 1f);

        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.numTilesX = 4;
        tsa.numTilesY = 4;
        tsa.animation = ParticleSystemAnimationType.WholeSheet;

        ApplyMaterial(ps, tex, "Legacy Shaders/Particles/Additive");
    }

    void SetupSmoke(ParticleSystem ps, string tex)
    {
        var main = ps.main;
        main.startLifetime = 2.5f;
        main.startSpeed = 0.8f;
        main.startSize = 6.0f;
        main.gravityModifier = -0.1f; // Dym Additive powinien szybciej unosić się w górę
        
        // Ciemniejszy kolor, żeby Additive nie "przepalił" całego ekranu na biało
        main.startColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);

        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.numTilesX = 8; // FIX: Siatka dymu 8x8
        tsa.numTilesY = 8;
        tsa.animation = ParticleSystemAnimationType.WholeSheet;

        // ZMIANA: Teraz dym też używa Additive
        ApplyMaterial(ps, tex, "Legacy Shaders/Particles/Additive");
    }

    void SetupSparks(ParticleSystem ps, string tex)
    {
        var main = ps.main;
        main.startLifetime = 0.4f;
        main.startSpeed = 6.0f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.9f, 0.5f, 1f);
        ApplyMaterial(ps, tex, "Legacy Shaders/Particles/Additive");
    }

    void ApplyMaterial(ParticleSystem ps, string texName, string shader)
    {
        string[] guids = AssetDatabase.FindAssets(texName + " t:texture2D");
        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]); // Naprawa błędu CS1503
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Shader s = Shader.Find(shader) ?? Shader.Find("Particles/Standard Unlit");

            Material mat = new Material(s);
            mat.mainTexture = tex;
            ps.GetComponent<ParticleSystemRenderer>().material = mat;
        }
    }

    List<string> FindTexturePaths(byte[] data)
    {
        List<string> paths = new List<string>();
        string content = Encoding.ASCII.GetString(data);
        int index = content.IndexOf("3DData");
        while (index != -1)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = index; i < data.Length && data[i] >= 32; i++) sb.Append((char)data[i]);
            string found = sb.ToString();
            if (found.ToLower().Contains(".dds")) paths.Add(found);
            index = content.IndexOf("3DData", index + 1);
        }
        return paths;
    }
}

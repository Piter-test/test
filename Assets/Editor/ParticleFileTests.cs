using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class RoseParticleEditor : EditorWindow
{
    [MenuItem("Rose Tools/Nowy Importer PTL")]
    public static void ShowWindow() => GetWindow<RoseParticleEditor>("PTL Editor 2024");

    private void OnGUI()
    {
        GUILayout.Label("Rose Online - Importer", EditorStyles.boldLabel);
        if (GUILayout.Button("Importuj .PTL", GUILayout.Height(40)))
        {
            string path = EditorUtility.OpenFilePanel("Wybierz PTL", "", "ptl");
            if (!string.IsNullOrEmpty(path)) ProcessFile(path);
        }
    }

    public void ProcessFile(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        List<string> textureNames = ExtractTextures(data);
        GameObject root = new GameObject("PTL_" + Path.GetFileNameWithoutExtension(path));
        foreach (string texName in textureNames) CreateLayer(root.transform, texName);
        Selection.activeGameObject = root;
    }

    // Zmienione na public static, aby testy widziały tę metodę bez problemów
    public static List<string> ExtractTextures(byte[] data)
    {
        List<string> found = new List<string>();
        string content = Encoding.ASCII.GetString(data);
        int index = content.IndexOf("3DData");
        while (index != -1)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = index; i < data.Length && data[i] >= 32; i++) sb.Append((char)data[i]);
            string path = sb.ToString();
            if (path.ToLower().EndsWith(".dds")) found.Add(Path.GetFileNameWithoutExtension(path));
            index = content.IndexOf("3DData", index + 1);
        }
        return found;
    }

    private void CreateLayer(Transform parent, string texName)
    {
        GameObject layer = new GameObject("Layer_" + texName);
        layer.transform.SetParent(parent);
        var ps = layer.AddComponent<ParticleSystem>();
        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.numTilesX = texName.Contains("smoke") ? 8 : 4;
        tsa.numTilesY = tsa.numTilesX;
    }
}

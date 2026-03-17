using UnityEngine;
using UnityEditor;
//using UnityRose.Formats;
using System.Collections;

public class ROSEImportMap : EditorWindow {

    private const string DataPathKey = "ROSE_DataPath";

    private bool wasEditing = false;
    private string rootPath = "rootPath";

    [MenuItem("ROSE/Rose Map Editor")]
    static void Init()
    {
        var window = GetWindow<ROSEImportMap>();
        window.titleContent = new GUIContent("Rose Map Editor");
        window.Show();
    }

	private Vector2 mapListScrollPosition;
    private bool mapListShowUnnamed = false;

    void OnGUI()
    {
        
        GUILayout.Label("\nSettings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80;

        string tempPath = EditorGUILayout.TextField("3DData Path:", rootPath, GUILayout.ExpandWidth(true));

        if (tempPath != rootPath)
        {
            rootPath = tempPath.Replace("\\", "/");

            if (!string.IsNullOrEmpty(rootPath) && !rootPath.EndsWith("/"))
            {
                rootPath += "/";
            }

            EditorPrefs.SetString(DataPathKey, rootPath);
        }

        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
                string selected = EditorUtility.OpenFolderPanel("Select 3DData Path folder", rootPath, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    rootPath = selected.Replace("\\", "/");
                    if (!rootPath.EndsWith("/")) rootPath += "/";

                    EditorPrefs.SetString(DataPathKey, rootPath);
                    Main.MaybeUpdate();
                    GUIUtility.ExitGUI();
                }

                string checkSTB = System.IO.Path.Combine(selected, "STB");
                string checkDataInside = System.IO.Path.Combine(selected, "3DDATA");

                GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();

        if (string.IsNullOrEmpty(rootPath))
        {
            rootPath = "3DData Path";
        }

        if (!System.IO.Directory.Exists(rootPath))
        {
            EditorGUILayout.HelpBox("ERROR: Folder not found!\nPlease enter a valid path to 3DData Path above.", MessageType.Error);

            return;
        }

        GUILayout.Label("\nImporting", EditorStyles.boldLabel);
        GUILayout.Label("Current Path: " + Main.GetCurrentPath() + "\n");

        if (GUILayout.Button("Clear Unity 3Ddata"))
            Main.ClearUData();
        if (GUILayout.Button("Clear All 3Ddata"))
            Main.ClearData();

        GUILayout.Label("\nMaps\n", EditorStyles.boldLabel);
        mapListShowUnnamed = GUILayout.Toggle(mapListShowUnnamed, "Show Unnamed Maps");
        GUILayout.Label("", EditorStyles.boldLabel);

        var mapData = Main.GetMapListData();

        if (mapData != null)
        {
            mapListScrollPosition = GUILayout.BeginScrollView(mapListScrollPosition);// GUILayout.Height(100));

			for (var i = 1; i < mapData.stb.Cells.Count; ++i)
            {
                string Deco = mapData.stb.Cells[i][12];
                string Cnst = mapData.stb.Cells[i][13];
                string Zon = mapData.stb.Cells[i][2];
				string mapName = mapData.stl.GetString(mapData.stb.Cells[i][27], Revise.Files.STL.STL.Language.English);//NPC 41

				if (mapName != null || mapListShowUnnamed)
				{
				    GUILayout.BeginHorizontal();
					GUILayout.Label("[" + i.ToString() + "] " + mapName);

					if (GUILayout.Button("Import", GUILayout.Width(100)))
                    {
                        Main.ClearData();
                        Main.ImportMapName(i, mapName, Deco, Cnst, Zon);// i, mapName, 
					}
                    GUILayout.EndHorizontal();
                }
            }
			GUILayout.EndScrollView();

        }

        if (EditorGUIUtility.editingTextField)
        {
            wasEditing = true;
        }
        else
        {
            if (wasEditing)
            {
                wasEditing = false;
                EditorPrefs.SetString(DataPathKey, rootPath);
                Main.MaybeUpdate();
            }
        }
    }

    void OnFocus()
    {
        if (EditorPrefs.HasKey(DataPathKey))
        {
            rootPath = EditorPrefs.GetString(DataPathKey);
        }
        Main.MaybeUpdate();
    }

    void OnLostFocus()
    {
        EditorPrefs.SetString(DataPathKey, rootPath);
        Main.MaybeUpdate();
    }

    void OnDestroy()
    {
        EditorPrefs.SetString(DataPathKey, rootPath);
        Main.MaybeUpdate();
    }
    public static string GetDataPath()
    {
        return EditorPrefs.GetString(DataPathKey);
    }

}

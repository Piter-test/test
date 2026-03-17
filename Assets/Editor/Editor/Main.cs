using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Main;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;

public class Main {
	/*
	//[MenuItem("ROSE/A Test")]
	static void Test() {
		try {
			AssetDatabase.StartAssetEditing();
			ImportCharModel1();
		} finally {
			AssetDatabase.StopAssetEditing();
		}
		try {
			AssetDatabase.StartAssetEditing();
			ImportCharModel2();
		} finally {
			AssetDatabase.StopAssetEditing();
		}
	}*/
	private static string rootPath = "";

    public class MapListData
	{
		//public Revise.Files.STB.DataFile stb;
		//public Revise.Files.STL.StringTableFile stl;
		public Revise.Files.STB.STB stb;
		public Revise.Files.STL.STL stl;
	}
	private static MapListData mapListData = null;

	public static string NormalizePath(string path)
	{
		string newPath = path;

		newPath = newPath.Replace("\\", "/");

		while (true)
		{
			var repPath = newPath.Replace("//", "/");
			if (repPath == newPath)
			{
				break;
			}
			newPath = repPath;
		}
		while (true)
		{
			var repPath = newPath.Replace("[^a-z]", "[^A-Z]");
			if (repPath == newPath)
			{
				break;
			}
			newPath = repPath;
		}

		return newPath;
	}

	public static string CombinePath(string path1, string path2)
	{
		return NormalizePath(path1 + "/" + path2);
	}

	public static string CombinePath(string path1, string path2, string path3)
	{
		return CombinePath(CombinePath(path1, path2), path3);
	}

	public static string CombinePath(string path1, string path2, string path3, string path4)
	{
		return CombinePath(CombinePath(CombinePath(path1, path2), path3), path4);
	}

	public static string GetCurrentPath()
	{
		return rootPath;
	}

    public static void ClearData()
	{

        // 1. USUWANIE OBIEKTÓW ZE SCENY
        // Pobieramy wszystkie obiekty, używając pełnej ścieżki UnityEngine.Object dla bezpieczeństwa
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        for (int i = allObjects.Length - 1; i >= 0; i--)
        {
            GameObject obj = allObjects[i];

            // Pominięcie, jeśli obiekt już nie istnieje (np. usunięty wraz z rodzicem)
            if (obj == null) continue;

            // Filtr nazw obiektów do usunięcia (Teren, NPC, Efekty, Budynki)
            if (obj.name.Contains("_") || obj.GetComponent<Terrain>() != null ||
                obj.name.StartsWith("NPC_") || obj.name.StartsWith("EFF_") ||
                obj.name.StartsWith("SND_") || obj.name.Contains("CNST") || obj.name.Contains("DECO") || obj.name.Contains("ANI_") || obj.name.Contains("New Game Object"))
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
        
        // 2. USUWANIE PLIKÓW I FOLDERÓW (Obsługa .meta)
        string folderPath = "Assets/ROSE Map";

        // Sprawdzamy, czy Unity widzi folder jako Asset (najbezpieczniejsza metoda)
        if (UnityEditor.AssetDatabase.IsValidFolder(folderPath))
        {
            // DeleteAsset usuwa folder ORAZ powiązany plik .meta automatycznie
            UnityEditor.AssetDatabase.DeleteAsset(folderPath);
        }
        else if (System.IO.Directory.Exists(folderPath))
        {
            // Jeśli folder istnieje tylko na dysku (np. po błędzie importu), używamy FileUtil
            UnityEditor.FileUtil.DeleteFileOrDirectory(folderPath);
        }

        UnityEditor.AssetDatabase.Refresh();

        //Debug.LogWarning("Wszystkie dane mapy i obiekty zostały wyczyszczone.");

        ClearUData();

	}

	public static void ClearUData()
	{
        //Directory.Delete("Assets/ROSEChars", true);
        //Directory.Delete("Assets/ROSEPmaps", true);
        //Directory.Delete("Assets/ROSEMdls", true);
        AssetDatabase.Refresh();
	}

    public static void MaybeUpdate()
	{
        string path = ROSEImportMap.GetDataPath();

        // Jeśli ścieżka nie istnieje, przerwij działanie i nie szukaj plików
        if (string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path))
        {
            return;
        }
        string curDataPath = ROSEImportMap.GetDataPath();
		if (curDataPath != rootPath)
		{
			rootPath = curDataPath;
			Update();
		}
	}

	private static void Update()
	{
		var md = new MapListData();
		md.stb = new Revise.Files.STB.STB(CombinePath(rootPath, "3DDATA/STB/LIST_ZONE.STB"));
		md.stl = new Revise.Files.STL.STL(CombinePath(rootPath, "3DDATA/STB/LIST_ZONE_S.STL"));
		//md.stb = new Revise.Files.STB.STB(CombinePath(rootPath, "3DDATA/STB/LIST_NPC.STB"));
		//md.stl = new Revise.Files.STL.STL(CombinePath(rootPath, "3DDATA/STB/LIST_NPC_S.STL"));
		mapListData = md;
	}
	public static MapListData GetMapListData()
	{
		return mapListData;
	}
    //public static void ImportMapCoord(int ix, int iy)
	//{
        //return (int ix, int iy);
        //int result = ix, iy;
        //return result;
        //ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
        //return ix;
        //return iy;
        //Debug.LogWarning("Test: Coord " + ix + ", " + iy);
        //var tx = ImportMapName
        //mapcoord = test1;
        //return importmapcoord;
        //Debug.LogWarning("Test: " + test1);// + ", " + test2 + ", " + test3 + ", " + test4);
    //}

	public static void ImportMapName(int mapIdx, string mapName, string Deco, string Cnst, string Zon)// int mapIdx, string mapName
	{
		//var coord = new TestCoord();
			//Debug.LogWarning("Test: " + ix + ", " + iy);
            Cnst = (NormalizePath(Cnst));
			Deco = (NormalizePath(Deco));
			Zon = (NormalizePath(Zon));

			Regex mapZsc = new Regex("3DDATA/([A-Z]*)/LIST_([A-Z_]*).ZSC", RegexOptions.IgnoreCase);
			Regex mapZsc1 = new Regex("3DDATA/([A-Z]*)/LIST_([A-Z_]*).ZSC", RegexOptions.IgnoreCase);
			Regex mapZon = new Regex("3DDATA/Maps/([A-Z]*)/([A-Z_,0-9]*)/([A-Z_,0-9]*).zon", RegexOptions.IgnoreCase);

			var mapMatches = mapZsc.Match(Deco);
			var mapMatches1 = mapZsc1.Match(Cnst);
			var mapMatches2 = mapZon.Match(Zon);

			var WorldName = mapMatches.Groups[1].Value;
			var DecoName = mapMatches.Groups[2].Value;
			//Debug.LogWarning("Test: " + WorldName + ", " + DecoName);

			var WorldName1 = mapMatches1.Groups[1].Value;
			var CnstName = mapMatches1.Groups[2].Value;
			//Debug.LogWarning("Test: " + CnstName);

			var planet1 = mapMatches2.Groups[1].Value;
			var map1 = mapMatches2.Groups[2].Value;
			var map2 = mapMatches2.Groups[3].Value;
			Debug.LogWarning("Wgrała się mapa: "  + mapName + ", " + map1 + " na Planecie " + planet1);// + planet1

        //Debug.LogWarning(Deco);
        //Debug.LogWarning(Cnst);
        //Debug.LogWarning(Zon);
        //Debug.LogWarning("Test: " + mapName);

        var files = new List<ModelDatabaseImporter>();

	    files.Add(new ModelDatabaseImporter(WorldName, map1, WorldName + "_" + DecoName, Deco));
		files.Add(new ModelDatabaseImporter(WorldName, map1, WorldName1 + "_" + CnstName, Cnst));
		//files.Add(new ModelDatabaseImporter(WorldName + "_" + DecoName, Deco));
		//files.Add(new ModelDatabaseImporter(WorldName1 + "_" + CnstName, Cnst));

		    try
		    {
				AssetDatabase.StartAssetEditing(); 
				foreach (var file in files)
					file.CopyTextures();
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		    /*
			try
			{
				AssetDatabase.StartAssetEditing();
				foreach (var file in files)
					file.CopyEffect();
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
			
			try 
			{
				AssetDatabase.StartAssetEditing();
				foreach (var file in files)
					file.ImportTiles();
			} finally {
				AssetDatabase.StopAssetEditing();
			}
			*/
			try
			{
				AssetDatabase.StartAssetEditing();
				foreach (var file in files)
                file.ImportModels();
            }
			finally
			{
				AssetDatabase.StopAssetEditing();
			}

			map.ImportMapCoord1(WorldName, map1);
		//ROSEImportMap.OnGUI(WorldName, map1);

		//Directory.CreateDirectory("Assets/ROSEMdls/" + WorldName + "/" + map1);
		//Directory.CreateDirectory("Assets/ROSEMdls/" + WorldName + "/" + map1 + "/Maps/");
		//Directory.CreateDirectory("Assets/ROSE Map/" + WorldName + "/" + map1 + "/Tile/");

		//Terrain[,] ter = new Terrain[65, 65];

		var zon = new Revise.Files.ZON.ZoneFile();

			zon.Load(rootPath + Zon);

			ImportMap(WorldName, map1, zon);

        if (WorldName + "/" + map1 == "JUNON/JDT01") // Canyon City of Zant
		{
			for (int ix = 31; ix <= 34; ++ix)
			{
				for (int iy = 30; iy <= 33; ++iy)
				{
					Terrain[,] ter = new Terrain[65, 65];
					ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
				}
			}
		}
		if (WorldName + "/" + map1 == "JUNON/JPT01") // City of Junon Polis
		{
			for (int ix = 30; ix <= 37; ++ix)
			{
				for (int iy = 30; iy <= 35; ++iy)
				{
					Terrain[,] ter = new Terrain[65, 65];
					ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
				}
			}
		}
		if (WorldName + "/" + map1 == "JUNON/JG06") // Dolphin Island
		{
			for (int ix = 31; ix <= 36; ++ix)
			{
				for (int iy = 30; iy <= 34; ++iy)
				{
					Terrain[,] ter = new Terrain[65, 65];
					ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
				}
			}
		}
		if (WorldName + "/" + map1 == "ELDEON/Title") // Title Eldeon
		{
			for (int ix = 31; ix <= 32; ++ix)
			{
				for (int iy = 31; iy <= 33; ++iy)
				{
					Terrain[,] ter = new Terrain[65, 65];
					ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
				}
			}
		}
		if (WorldName + "/" + map1 == "JUNON/TITLE_JG01") // Title Dolphin Island
		{
			for (int ix = 35; ix <= 36; ++ix)
			{
				for (int iy = 32; iy <= 33; ++iy)
				{
					Terrain[,] ter = new Terrain[65, 65];
					ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
				}
			}
		}
		//int result = ix, iy;
		//return result;
		//return Add(ix, iy);
		//int iy;
		//int ix;
		//ix = ImportMapCoord(ix, iy);
		//iy = ImportMapCoord(ix, iy);
		//numberWasUnderFive = ImportMapCoord(ix, iy);
		//var iy = ImportMapCoord(ix, iy);
		//var tik = ImportMapCoord(test1);
		//test.Loadtest1(mapcoord );
		//ImportMapCoord(ix, iy);
		//return ImportMapCoord(ix, iy);
		//Import(test1) = test1
		//int test;
		//Debug.LogWarning("Test1: " + test);
		//return coordix;
		//ImportMapCoord(test1, test2, test3, test4);
		//test1 = (ImportMapCoord(test1));
		//Debug.LogWarning("Test Coord: " + coordix);
		//int test;
		//test1 = test1;
		//test1 = (ImportMapCoord(test1));
		//static void ImportMapCoord(int test1, int test2, int test3, int test4)
		//{s
		//for (int ix = 31; ix <= 34; ++ix) // 31-32 / 31-34
		//{
		//for (int iy = 30; iy <= 33; ++iy) // 31-33 / 30-33
		//{
		//Debug.LogWarning("Test: Coord " + ix + ", " + iy);
		//ter[ix, iy] = ImportTerrain(WorldName, map1, zon, ix, iy, CnstName, DecoName);
		//}

		//}

	}

    //[MenuItem("ROSE/Import Model Databases")]
    //static void ImportModelDatabases()
    //{       
    //var files = new List<ModelDatabaseImporter>();

    //files.Add(new ModelDatabaseImporter("JUNON_JPT_DECO", Deco));
    //files.Add(new ModelDatabaseImporter("JUNON_JPT_DECO", "3DDATA/JUNON/LIST_DECO_JPT.ZSC"));
    //files.Add(new ModelDatabaseImporter("JUNON_JPT_CNST", "3DDATA/JUNON/LIST_CNST_JPT.ZSC"));
    /*
    try
    {
        AssetDatabase.StartAssetEditing();
        foreach (var file in files)
            file.CopyTextures();
    } finally {
        AssetDatabase.StopAssetEditing();
    }
    /*
    try 
    {
        AssetDatabase.StartAssetEditing();
        foreach (var file in files)
            file.ImportTiles();
    } finally {
        AssetDatabase.StopAssetEditing();
    }*/
    /*
    try 
    {
        AssetDatabase.StartAssetEditing();
        foreach (var file in files)
            file.ImportModels();
    } finally {
        AssetDatabase.StopAssetEditing();
    }*/
    //}
    //[MenuItem("ROSE/Import Map")]
    //static void DoSomething()
    //{
    /*
    Directory.CreateDirectory("Assets/ROSEPmaps/JUNON/JPT01");

    Terrain[,] ter = new Terrain[65, 65];

    var zon = new Revise.Files.ZON.ZoneFile();

    zon.Load(rootPath + "3DDATA/MAPS/JUNON/JPT01/JPT01.zon");

    ImportMap("JUNON", "JPT01", zon);


    for (int ix = 30; ix <= 37; ++ix)
    {
        for (int iy = 30; iy <= 35; ++iy)
        {
            ter[ix, iy] = ImportTerrain("JUNON", "JPT01", zon, ix, iy);
        }
    }*/
    /*
    for (int ix = 0; ix < 65; ++ix)
    {
        for (int iy = 0; iy < 65; ++iy)
        {
            if (!ter[ix,iy]) {
                continue;
            }
            Terrain left = null; 
            Terrain top = null;
            Terrain right = null;
            Terrain bottom = null;
            if (ix > 0) left = ter[ix - 1, iy];
            if (iy > 0) top = ter[ix, iy - 1];
            if (ix < 65) right = ter[ix + 1, iy];
            if (iy < 65) bottom = ter[ix, iy + 1];
            ter[ix, iy].SetNeighbors(left, top, right, bottom);
        }
    }
    //*/
    //}

    const string charBasePath = "Assets/ROSEChars/";
	static public void ImportCharModel1() {
		Directory.CreateDirectory(charBasePath);

		string ddsPath = rootPath + "3DDATA/NPC/PLANT/JELLYBEAN1/BODY02.DDS";
		string texPath = charBasePath + "JELLYBEAN.DDS";
		if (!File.Exists(texPath)) {
			File.Copy(ddsPath, texPath);
			AssetDatabase.ImportAsset(texPath);
		}
	}

	static public void ImportCharMesh(string meshPath, string zmsPath) {
		if (!File.Exists(zmsPath)) {
			Debug.LogWarning("Failed to find referenced ZMS.");
			return;
		}

		var mesh = new Mesh();

		var zms = new Revise.Files.ZMS.ModelFile();
		zms.Load(zmsPath);

		var verts = new Vector3[zms.Vertices.Count];
		var uvs = new Vector2[zms.Vertices.Count];
		var bones = new BoneWeight[zms.Vertices.Count];

		for (int k = 0; k < zms.Vertices.Count; ++k) {
			var v = zms.Vertices[k];
			v.TextureCoordinates[0].y = 1 - v.TextureCoordinates[0].y;

			verts[k] = rtuPosition(v.Position);
			uvs[k] = v.TextureCoordinates[0];

			bones[k] = new BoneWeight();
			bones[k].boneIndex0 = zms.BoneTable[v.BoneIndices.X];
			bones[k].boneIndex1 = zms.BoneTable[v.BoneIndices.Y];
			bones[k].boneIndex2 = zms.BoneTable[v.BoneIndices.Z];
			bones[k].boneIndex3 = zms.BoneTable[v.BoneIndices.W];
			bones[k].weight0 = v.BoneWeights.x;
			bones[k].weight1 = v.BoneWeights.y;
			bones[k].weight2 = v.BoneWeights.z;
			bones[k].weight3 = v.BoneWeights.w;
		}
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.boneWeights = bones;

		int[] indices = new int[zms.Indices.Count * 3];
		for (int k = 0; k < zms.Indices.Count; ++k) {
			indices[k * 3 + 0] = zms.Indices[k].X;
			indices[k * 3 + 2] = zms.Indices[k].Y;
			indices[k * 3 + 1] = zms.Indices[k].Z;
		}
		mesh.triangles = indices;

		mesh.RecalculateNormals();

		AssetDatabase.CreateAsset(mesh, meshPath);
	}

	static public void ImportCharModel2() {
		ImportCharMesh(charBasePath + "JELLYBEAN_1.asset", rootPath + "3DDATA/NPC/PLANT/JELLYBEAN1/BODY01.ZMS");
		ImportCharMesh(charBasePath + "JELLYBEAN_2.asset", rootPath + "3DDATA/NPC/PLANT/JELLYBEAN1/BODY02.ZMS");
	}

	static public string hashFile(string filePath) {
		using (var c = new SHA256Managed()) {
			using (var f = new FileStream(filePath, FileMode.Open))
			{
				byte[] hash = c.ComputeHash(f);
				StringBuilder sb = new StringBuilder();
				foreach (byte b in hash) {
					sb.Append(b.ToString("x2"));
				}
				return sb.ToString();
			}
		}
	}

	//const string rootPath = "E:/Rose/ROSE Online VFS/";

	private class ModelDatabaseImporter
	{
		private string _name;
		private string _basePath;
		private Revise.Files.ZSC.ModelListFile _f; // = null;
        //private Revise.Files.PTL.ParticleFile _e;
        //private Revise.Files.ZON.ZoneFile til;


        Dictionary<string, int> hashLookup = new Dictionary<string, int>();
		int[] texLookup = null;
        int[] effLookup = null;
        //int[] tilLookup = null;
        Mesh[] _meshLookup = null;
		Material[] _matLookup = null;
        //Material[] _matptlLookup = null;
        int texIndex = 0;
        int effIndex = 0;
        //int tilIndex = 0;

        public ModelDatabaseImporter(string planet, string map, string name, string zscPath)
		{
			_name = name;
			_basePath = "Assets/ROSE Map/" + planet + "/" + map + "/" + _name + "/";

			Directory.CreateDirectory(_basePath);

            _f = new Revise.Files.ZSC.ModelListFile();
			//til = new Revise.Files.ZON.ZoneFile();
			_f.Load(rootPath + zscPath);
			//til.Load(rootPath + zscPath);

			//eff = new Revise.Files.EFT.EffectFile();
			//eff.Load(rootPath + eftPath);

			texLookup = new int[_f.TextureFiles.Count];
            effLookup = new int[_f.EffectFiles.Count];


        }

		public void CopyTextures()
		{
			for (int i = 0; i < _f.TextureFiles.Count; ++i)
			{
				var ddsPath = rootPath + _f.TextureFiles[i].FilePath;
                ddsPath = (NormalizePath(ddsPath));

                if (!File.Exists(ddsPath))
				{
					Debug.LogWarning("Could not find referenced texture - " + ddsPath);
					continue;
				}

				string texHash = hashFile(ddsPath);
				if (hashLookup.ContainsKey(texHash))
				{
					texLookup[i] = hashLookup[texHash];
					continue;
				}

				var texIdx = texIndex++;
				hashLookup[texHash] = texIdx;
				texLookup[i] = texIdx;

				var texPath = _basePath + "Tex_" + texIdx.ToString() + ".png"; //DDS
				if (!File.Exists(texPath))
				{
					File.Copy(ddsPath, texPath);
					AssetDatabase.ImportAsset(texPath);
				}
			}
            for (int i = 0; i < _f.EffectFiles.Count; ++i)
            {
                var ddsPath = rootPath + _f.EffectFiles[i];
				ddsPath = (NormalizePath(ddsPath));

                var eff = new Revise.Files.EFT.EffectFile();
				eff.Load(ddsPath);

                for (int g = 0; g < eff.Animations.Count; ++g)
				{
					var animPath = rootPath + eff.Animations[g].TextureFilePath;
					animPath = (NormalizePath(animPath));
				}
				for (int j = 0; j < eff.Particles.Count; ++j)
				{
					var effPath = rootPath + eff.Particles[j].FilePath;
					effPath = NormalizePath(effPath);

					var ptl = new Revise.Files.PTL.ParticleFile();
					ptl.Load(effPath);

                    for (int h = 0; h < ptl.Sequences.Count; ++h)
					{
						var ptlPath = rootPath + ptl.Sequences[h].TextureFileName;
						ptlPath = (NormalizePath(ptlPath));

						if (!File.Exists(ptlPath))
						{
							Debug.LogWarning("Could not find referenced texture - " + ptlPath);
							continue;
						}
						string texHash = hashFile(ptlPath);
						if (hashLookup.ContainsKey(texHash))
						{
							effLookup[h] = hashLookup[texHash];
							continue;
						}

                        var ptlIdx = effIndex++;
                        hashLookup[texHash] = ptlIdx;
                        effLookup[h] = ptlIdx;

                        var texPath = _basePath + "Tex_Ptl_" + ptlIdx.ToString() + ".png"; //DDS

                        if (!File.Exists(texPath))
						{
							File.Copy(ptlPath, texPath);
							AssetDatabase.ImportAsset(texPath);
						}
                    }
				}
			}
		}

        public void ImportModels()
        {
            List<GameObject> temporaryNpcList = new List<GameObject>();
            for (int i = 0; i < _f.EffectFiles.Count; ++i)
			{
                //var mdl = ScriptableObject.CreateInstance<RoseMapObjectData>();
                var ddsPath = rootPath + _f.EffectFiles[i];
				ddsPath = (NormalizePath(ddsPath));
				var eff = new Revise.Files.EFT.EffectFile();
				eff.Load(ddsPath);

                for (int g = 0; g < eff.Animations.Count; ++g)
				{
					var animPath = rootPath + eff.Animations[g].TextureFilePath;
					animPath = (NormalizePath(animPath));
				}

                for (int j = 0; j < eff.Particles.Count; ++j)
				{
                    var partData = eff.Particles[j];
                    //var effPath = rootPath + partData.FilePath;
                    var effPath = NormalizePath(rootPath + partData.FilePath);
                    var nameOnly = System.IO.Path.GetFileNameWithoutExtension(partData.FilePath);
                    //var test = partData.FilePath;
                    //effPath = (NormalizePath(effPath));
                    //Debug.LogWarning("Failed to find referenced ZMS." + nameOnly);
                    if (!File.Exists(effPath)) continue;

                    var ptl = new Revise.Files.PTL.ParticleFile();
					ptl.Load(effPath);

					//_matptlLookup = new Material[ptl.Sequences.Count];

					for (int h = 0; h < ptl.Sequences.Count; ++h)
					{
                        
                        var sequence = ptl.Sequences[h];
                        var ptlPath = rootPath + ptl.Sequences[h].TextureFileName;
						ptlPath = (NormalizePath(ptlPath));
                        //Debug.LogWarning("LifeTime: " + ptlPath);
                        string texHash = hashFile(ptlPath);
						int ptlIdx;

						if (hashLookup.ContainsKey(texHash))
						{
							ptlIdx = hashLookup[texHash];
						}
						else
						{
							ptlIdx = effIndex++;
							hashLookup[texHash] = ptlIdx;

							string newTexPath = _basePath + "Tex_Ptl_" + ptlIdx + ".png";
                            if (File.Exists(ptlPath)) File.Copy(ptlPath, newTexPath, true);
                            //File.Copy(ptlPath, newTexPath, true);
                        }
                        AssetDatabase.ImportAsset(_basePath + "Tex_Ptl_" + ptlIdx + ".png");
                        var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(_basePath + "Tex_Ptl_" + ptlIdx + ".png");

                        var ptlmat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                        ptlmat.mainTexture = tex2d;

                        //string matPath = _basePath + "Eff_Mat_" + ptlIdx + "_" + h + ".mat";
                        string matPath = _basePath + "Mat_Ptl_" + ptlIdx + ".mat";

                        AssetDatabase.CreateAsset(ptlmat, matPath);

						//string npcName = "Ptl_Part_" + h + "_" + sequence.Name;
						//GameObject ptlObj = new GameObject(npcName);

						string ptlName = "Ptl_" + ptlIdx;// sequence.Name.Trim() + "_" + h;
                        GameObject ptlObj = new GameObject(ptlName);


                        //var mapMatches = mapZsc.Match(Deco);
                        //var map1 = test.Groups[1].Value;
						//var test = partData.name;
							//Debug.LogWarning("Failed to find referenced ZMS." + test);
                        // 2. DODAJEMY KOMPONENTY PARTICLE SYSTEM
                        var ps = ptlObj.AddComponent<ParticleSystem>();
                        var renderer = ptlObj.GetComponent<ParticleSystemRenderer>();
                        var main = ps.main;
                        var emission = ps.emission;
                        var force = ps.forceOverLifetime;
                        var tsa = ps.textureSheetAnimation;
                        var shape = ps.shape;

                        // 3. KONFIGURACJA ZGODNIE Z KLASĄ ParticleFile (DANE Z BINARY READER)
                        // Czas życia (Lifetime)
                        main.startLifetime = new ParticleSystem.MinMaxCurve(sequence.Lifetime.Minimum, sequence.Lifetime.Maximum);
                        //main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
                        // Ilość cząsteczek i częstotliwość (EmitRate & ParticleCount)
                        main.maxParticles = sequence.ParticleCount;
						main.startSpeed = 0;
						//main.simulationSpeed = sequence.SpawnDirection;// sequence.Gravity;
                        //main.simulationSpeed = 0.0f; // <--- TUTAJ
                        //main.simulationSpace = ParticleSystemSimulationSpace.Local;
                        emission.enabled = true;
                        
                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(sequence.EmitRate.Minimum, sequence.EmitRate.Maximum);
                        Debug.LogWarning("Failed to find referenced ZMS." + sequence.ParticleCount);
                        // Grawitacja (Gravity)
                        force.enabled = true;
                        force.x = new ParticleSystem.MinMaxCurve(sequence.Gravity.Minimum.x, sequence.Gravity.Maximum.x);
                        force.y = new ParticleSystem.MinMaxCurve(sequence.Gravity.Minimum.y, sequence.Gravity.Maximum.y);
                        force.z = new ParticleSystem.MinMaxCurve(sequence.Gravity.Minimum.z, sequence.Gravity.Maximum.z);

                        // Obszar emisji (EmitRadius)
                        shape.enabled = true;
                        shape.shapeType = ParticleSystemShapeType.Box;
                        shape.scale = sequence.EmitRadius.Maximum; // Mapowanie promienia na skalę boxa

                        // Texture Sheet Animation (TextureWidth / TextureHeight)
                        if (sequence.TextureWidth > 1 || sequence.TextureHeight > 1)
                        {
                            tsa.enabled = true;
                            tsa.numTilesX = sequence.TextureWidth;
                            tsa.numTilesY = sequence.TextureHeight;
                            tsa.mode = ParticleSystemAnimationMode.Grid;
                            tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                            tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f, 1f);
                        }

                        // 4. PRZYPISANIE MATERIAŁU (Używamy ptlmat stworzonego wcześniej w pętli h)
                        renderer.material = ptlmat;

                        // 5. TRANSFORMACJA I RODZIC
                        ptlObj.transform.position = rtuPosition(partData.Position) / 100f;
                        ptlObj.transform.rotation = rtuRotation(partData.Rotation);
                        //Debug.LogWarning("Failed to find referenced ZMS.");
                        // Jeśli masz listę temporaryNpcList, dodaj do niej obiekt
                        if (temporaryNpcList != null)
                        {
                            temporaryNpcList.Add(ptlObj);
                        }
                        //Debug.LogWarning("LifeTime: " + sequence.Lifetime.Minimum + ", " + sequence.Lifetime.Maximum + "maxParticles: " + sequence.ParticleCount);
                        // 6. START
                        ps.Play();



                        //effData.parent = partData.Parent; // To przypnie go do konkretnego Mesha

                        // DODANIE DO MODELU (Każde h to teraz osobny GameObject w UpdateModels)
                        //mdl.effects.Add(effData);

                        /*
                        // --- TWORZENIE OSOBNEGO EFEKTU DLA KAŻDEJ SEKWENCJI ---
                        var effData = new RoseMapObjectData.EffectData();

                        // Dane transformacji z partData (z pliku ZSC/IFO)
                        effData.lifetimeMin = sequence.Lifetime.Minimum;
                        effData.lifetimeMax = sequence.Lifetime.Maximum;
                        effData.emitRateMin = sequence.EmitRate.Minimum;
                        effData.emitRateMax = sequence.EmitRate.Maximum;
                        effData.gravityMin = sequence.Gravity.Minimum;
                        effData.gravityMax = sequence.Gravity.Maximum;
                        effData.spawnDirMin = sequence.SpawnDirection.Minimum;
                        effData.spawnDirMax = sequence.SpawnDirection.Maximum;
                        effData.emitRadiusMin = sequence.EmitRadius.Minimum;
                        effData.emitRadiusMax = sequence.EmitRadius.Maximum;
                        effData.TextureWidth = sequence.TextureWidth;
                        effData.TextureHeight = sequence.TextureHeight;

                        effData.particleCount = sequence.ParticleCount;
                        effData.loopCount = sequence.LoopCount;
                        effData.alignment = (int)sequence.Alignment;

                        effData.position = rtuPosition(partData.Position) / 100f;
                        effData.rotation = rtuRotation(partData.Rotation);
                        effData.scale = Vector3.one; // PTL zazwyczaj mają własną skalę w sekwencji
                        Debug.LogWarning("Could not find texture file - " + effData.lifetimeMin + " " + effData.TextureHeight + " " + effData.TextureWidth);
                        // Rodzic z ZSC
                        //effData.parent = partData.Parent;

                        // Przypisanie unikalnego materiału tej sekwencji
                        effData.material = ptlmat;
                        mdl.effects.Add(effData);
                        EditorUtility.SetDirty(mdl);*/
                        // DODAJEMY DO MODELU - teraz każda sekwencja PTL to osobny wpis na liście
                        //mdl.effects.Add(effData);
                        /*effLookup[h] = ptlIdx;
						var ptlPath1 = _basePath + "Tex_Eff_" + ptlIdx.ToString() + ".png";

						if (!File.Exists(ptlPath1))
						{
							Debug.LogWarning("Could not find texture file - " + ptlPath1);
							continue;
						}

						var tex2d = AssetDatabase.LoadAssetAtPath(ptlPath1, typeof(Texture2D)) as Texture2D;

						var ptlmat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));//("Universal Render Pipeline/Particles/Unlit"));
						ptlmat.mainTexture = tex2d;

						var ptlPath2 = _basePath + "Eff_Mat_" + ptlIdx.ToString() + ".mat";
						AssetDatabase.CreateAsset(ptlmat, ptlPath2);

                        var effData = new RoseMapObjectData.EffectData();
                        effData.position = rtuPosition(partData.Position) / 100f;
                        effData.rotation = rtuRotation(partData.Rotation);

                        _matptlLookup[h] = ptlmat;*/
                    }
				}
			}

            _meshLookup = new Mesh[_f.ModelFiles.Count];
			for (int i = 0; i < _f.ModelFiles.Count; ++i)
			{
				var zmsPath = rootPath + _f.ModelFiles[i];
				if (!File.Exists(zmsPath))
				{
					Debug.LogWarning("Failed to find referenced ZMS.");
					continue;
				}

				var mesh = new Mesh();

				var zms = new Revise.Files.ZMS.ModelFile();
				zms.Load(zmsPath);

				var verts = new Vector3[zms.Vertices.Count];
				var uvs = new Vector2[zms.Vertices.Count];
				for (int k = 0; k < zms.Vertices.Count; ++k)
				{
					var v = zms.Vertices[k];
					v.TextureCoordinates[0].y = 1 - v.TextureCoordinates[0].y;

					verts[k] = rtuPosition(v.Position);
					uvs[k] = v.TextureCoordinates[0];
				}
				mesh.vertices = verts;
				mesh.uv = uvs;

				int[] indices = new int[zms.Indices.Count * 3];
				for (int k = 0; k < zms.Indices.Count; ++k)
				{
					indices[k * 3 + 0] = zms.Indices[k].X;
					indices[k * 3 + 2] = zms.Indices[k].Y;
					indices[k * 3 + 1] = zms.Indices[k].Z;
				}
				mesh.triangles = indices;

				mesh.RecalculateNormals();
				Unwrapping.GenerateSecondaryUVSet(mesh);

				var meshPath = _basePath + "Mesh_" + i.ToString() + ".asset";
				AssetDatabase.CreateAsset(mesh, meshPath);
				_meshLookup[i] = mesh;
			}

                _matLookup = new Material[_f.TextureFiles.Count];
			for (int i = 0; i < _f.TextureFiles.Count; ++i)
			{
				var tex = _f.TextureFiles[i];

				var texPath = _basePath + "Tex_" + texLookup[i].ToString() + ".png";//DDS

                if (!File.Exists(texPath))
				{
					continue;
				}

				var tex2d = AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D)) as Texture2D;

				Shader shader = null;
				if (tex.TwoSided) {
					if (tex.AlphaTestEnabled)
						shader = Shader.Find("Transparent/Cutout/DoubleSided");
					else if (tex.AlphaEnabled)
						shader = Shader.Find("Transparent/DoubleSided");
					else {
						Debug.LogWarning("Two-sided non-alpha material encountered.");
					}
				} else if (tex.AlphaTestEnabled)
					shader = Shader.Find("Transparent/Cutout/Diffuse");
				else if (tex.AlphaEnabled)
					shader = Shader.Find("Transparent/Diffuse");
				else
					shader = Shader.Find("Diffuse");

				if (!shader)
				{
					Debug.LogWarning("Failed to find appropriate shader for material.");
					continue;
				}

				var mat = new Material(shader);
				if (tex.AlphaTestEnabled)
					mat.SetFloat("_Cutoff", (float)tex.AlphaReference / 256);
				mat.mainTexture = tex2d;

				var matPath = _basePath + "Mat_" + i.ToString() + ".mat";
				AssetDatabase.CreateAsset(mat, matPath);

				_matLookup[i] = mat;
            }

			for (int i = 0; i < _f.Objects.Count; ++i)
			{
				var obj = _f.Objects[i];
				var mdl = ScriptableObject.CreateInstance<RoseMapObjectData>();
                mdl.subObjects.Clear();
                mdl.effects.Clear();
                for (int j = 0; j < obj.Parts.Count; ++j)
				{
					var part = obj.Parts[j];
					var subObj = new RoseMapObjectData.SubObject();

					subObj.mesh = _meshLookup[part.Model];
					subObj.material = _matLookup[part.Texture];
					subObj.animation = null;
					subObj.parent = part.Parent;
					subObj.position = rtuPosition(part.Position) / 100;
					subObj.rotation = rtuRotation(part.Rotation);
					subObj.scale = rtuScale(part.Scale);
					if (part.Collision == Revise.Files.ZSC.CollisionType.None)
					{
						subObj.colMode = 0;
					}
					else
					{
						subObj.colMode = 1;
					}

					if (part.AnimationFilePath != "")
					{
						var animPath = _basePath + "Anim_" + i.ToString() + "_" + j.ToString() + ".asset";
						var clip = ImportNodeAnimation(animPath, part.AnimationFilePath);
						subObj.animation = clip;

					}
					mdl.subObjects.Add(subObj);
				}

                    for (int h = 0; h < obj.Effects.Count; ++h)
					{

                    var eff = obj.Effects[h];
                    var effData = new RoseMapObjectData.EffectData();

                    effData.position = rtuPosition(eff.Position) / 100f;
                    effData.rotation = rtuRotation(eff.Rotation);
                    effData.scale = rtuScale(eff.Scale);
                    effData.parent = eff.Parent;

                    mdl.effects.Add(effData);
                    }
				var mdlPath = _basePath + "Model_" + i.ToString() + ".asset";
				AssetDatabase.CreateAsset(mdl, mdlPath);
			}
            /*
            for (int i = 0; i < _f.EffectFiles.Count; i++)
            {
				var eff = _f.EffectFiles[i];
                var zmsPath = rootPath + _f.EffectFiles[i];
                var mdl = ScriptableObject.CreateInstance<RoseMapObjectData>();
				Debug.LogWarning("test Effect " + i + " - " + zmsPath);
				var effPath = _basePath + "TexEff_" + i.ToString() + ".EFT";
				AssetDatabase.CreateAsset(mdl, effPath);
				EditorUtility.SetDirty(mdl);
				if (!File.Exists(effPath))
				{
				continue;
				}
				var a = new GameObject();
				//for (int j = 0; j < eff.effects.Count; ++j)
				//{
					//var part = eff.effects[j];
					//var subObj = new RoseMapObjectData.SubObject();
					//subObj.position = rtuPosition(eff.Position);
				//}
				for (int j = 0; j < eff.Parts.Count; ++j)
				{
					var part = eff.Parts[j];
					var subObj = new RoseMapObjectData.SubObject();
					Debug.LogWarning("test 1 " + j);
					//eff.Name.ToString());
					subObj.Name = _meshLookup[part.Model];
					subObj.material = _matLookup[part.Texture];
					subObj.animation = null;
					subObj.parent = part.Parent;
					subObj.position = rtuPosition(part.Position);// / 100;
					subObj.rotation = rtuRotation(part.Rotation);
					subObj.scale = rtuScale(part.Scale);
					if (part.Collision == Revise.Files.ZSC.CollisionType.None)
					{
					subObj.colMode = 0;
					}
					else
					{
					subObj.colMode = 1;
					}
					if (eff.AnimationFilePath != "")
					{
						var effPath = _basePath + "Effect_" + i.ToString() + "_" + j.ToString() + ".asset";
						var clip = ImportNodeAnimation(effPath, eff.AnimationFilePath);
						subObj.animation = clip;
					}
					mdl.subObjects.Add(subObj);
						{
							var mdlPath = _basePath + "Effect_" + i.ToString() + ".asset";
							AssetDatabase.CreateAsset(mdl, mdlPath);
							EditorUtility.SetDirty(mdl);
						}
					
				}
           */
		}
	}

	static AnimationClip ImportNodeAnimation(string clipPath, string zmoPath) {
		var f = new Revise.Files.ZMO.MotionFile();
		f.Load(rootPath + zmoPath);

		var clip = new AnimationClip();
		clip.legacy = true; // change to legacy
		clip.wrapMode = WrapMode.Loop;
		clip.frameRate = f.FramesPerSecond;

		for (int i = 0; i < f.ChannelCount; ++i) {
			if (f[i].Index != 0) {
				Debug.LogWarning("Invalid channel index encountered");
				continue;
			}

			if (f[i].Type == Revise.Files.ZMO.ChannelType.Rotation) {
				var c = f[i] as Revise.Files.ZMO.Channels.RotationChannel;
				var curvex = new AnimationCurve();
				var curvey = new AnimationCurve();
				var curvez = new AnimationCurve();
				var curvew = new AnimationCurve();
				for (int j = 0; j < f.FrameCount; ++j) {
					var frame = rtuRotation(c.Frames[j]);
					curvex.AddKey((float)j / (float)f.FramesPerSecond, frame.x);
					curvey.AddKey((float)j / (float)f.FramesPerSecond, frame.y);
					curvez.AddKey((float)j / (float)f.FramesPerSecond, frame.z);
					curvew.AddKey((float)j / (float)f.FramesPerSecond, frame.w);
				}
				clip.SetCurve("", typeof(Transform), "localRotation.x", curvex);
				clip.SetCurve("", typeof(Transform), "localRotation.y", curvey);
				clip.SetCurve("", typeof(Transform), "localRotation.z", curvez);
				clip.SetCurve("", typeof(Transform), "localRotation.w", curvew);
				//clip.legacy = true; // change to legacy
			} else if (f[i].Type == Revise.Files.ZMO.ChannelType.Position) {
				var c = f[i] as Revise.Files.ZMO.Channels.PositionChannel;
				var curvex = new AnimationCurve();
				var curvey = new AnimationCurve();
				var curvez = new AnimationCurve();
				for (int j = 0; j < f.FrameCount; ++j) {
					var frame = rtuPosition(c.Frames[j]) / 100;
					curvex.AddKey((float)j / (float)f.FramesPerSecond, frame.x);
					curvey.AddKey((float)j / (float)f.FramesPerSecond, frame.y);
					curvez.AddKey((float)j / (float)f.FramesPerSecond, frame.z);
				}
				clip.SetCurve("", typeof(Transform), "localPosition.x", curvex);
				clip.SetCurve("", typeof(Transform), "localPosition.y", curvey);
				clip.SetCurve("", typeof(Transform), "localPosition.z", curvez);
				//clip.legacy = true; // change to legacy
			} else {
				Debug.LogWarning("Encountered unknown channel type.");
			}
		}

		AssetDatabase.CreateAsset(clip, clipPath);
		return clip;
	}


	static Texture2D ImportPlanMap(string planet, string map, int x, int y) {

        string mapPath1 = planet + "/" + map + "/Map/" + x.ToString() + "_" + y.ToString() + "PLANMAP.PNG";
		string mapPath = planet + "/" + map + "/" + x.ToString() + "_" + y.ToString() + "PLANMAP.DDS";
		string assetPath = "Assets/ROSE Map/" + mapPath1;
		string mapRoot = (rootPath + "3DDATA/MAPS/" + mapPath);

		if (System.IO.File.Exists(mapRoot))
		{
			Directory.CreateDirectory("Assets/ROSE Map/" + planet + "/" + map + "/Map/");
			File.Copy(mapRoot, assetPath, true);
			AssetDatabase.ImportAsset(assetPath);
		}
		else
		{
			Debug.LogWarning("PLANMAP.DDS Not Found"); 
		}

		return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
    }

    static Vector3 ifotruPosition(Vector3 v) {
		return (rtuPosition(v) / 100) + new Vector3(80, 0, 80);
	}

    static GameObject ImportObject(string set, int x, int y, string prefix, int i, Revise.Files.IFO.Blocks.MapBlock obj, string planet, string map)
    //static void ImportObject(string set, int x, int y, string prefix, int i, Revise.Files.IFO.Blocks.MapBlock obj, string planet, string map)
    {
        //GameObject go = new GameObject();
        //got.name = prefix + "_" + obj.ObjectID + "_";
        
        //GameObject got = null;
        var blockName = x.ToString() + "_" + y.ToString();
		string mdlBasePath = "Assets/ROSE Map/" +planet + "/" +map + '/'+ set + "/";
        var mdlName = prefix + "_" + obj.ObjectID.ToString() + " " + "( " + i.ToString() + " )";
        //got.name = prefix + "_" + i.ToString();
        //Debug.LogWarning("Could not find referenced tile texture - " + mdlName);


        //var mdlName = prefix + "_" + i.ToString();
        //got.name = mdlName;
        /*GameObject terrainObj = null;
        Terrain foundTerrain = UnityEngine.Object.FindFirstObjectByType<Terrain>();

        if (foundTerrain != null)
        {
            terrainObj = foundTerrain.gameObject;
        }

        // 2. Teraz linia 1016 będzie działać, bo 'terrainObj' już istnieje
        if (terrainObj != null)
        {
            UnityEngine.Object.DestroyImmediate(terrainObj);
        }*/

        //UnityEngine.Object.DestroyImmediate(terrainObj);
        //Object.DestroyImmediate(GameObject.Find(mdlName));

        // Temporarily disable objects while working on Terrain!
        //return;
        //RoseMapObjectData modata = AssetDatabase.LoadAssetAtPath<RoseMapObjectData>(mdlPath);

        var mdlBaseName = mdlBasePath + "Model_" + obj.ObjectID.ToString();
		var mdlPath = mdlBaseName + ".asset";

		RoseMapObjectData modata = AssetDatabase.LoadAssetAtPath(mdlPath, typeof(RoseMapObjectData)) as RoseMapObjectData;
        
		if (modata == null)
        {
            return null; // Nie ma pliku -> nie twórz obiektu
        }
		if (!modata)
		{
		    Debug.LogWarning("Failed to find map model - " + mdlPath);
            return null;
        }

        GameObject go = new GameObject();
		var mo = go.AddComponent<RoseMapObject>();
		mo.data = modata;
		mo.UpdateModels();

        

        go.transform.localPosition = ifotruPosition(obj.Position);
		go.transform.localRotation = rtuRotation(obj.Rotation);
		go.transform.localScale = rtuScale(obj.Scale);
		go.isStatic = true;
		go.name = mdlName;
        //Debug.LogWarning("Could not find referenced tile texture - " + go);
        return go;
    }
    //private string _name;
    //private string _basePath;
    static void ImportMap(string planet, string map, Revise.Files.ZON.ZoneFile zon) {

		/*for (var i = 0; i < zon.SpawnPoints.Count; ++i) {
        var spawn = zon.SpawnPoints[i];
        Debug.LogWarning(
        i.ToString() + ": " +
        spawn.Position.ToString() + "," +
        spawn.Name.ToString());
        }
		
		for (int i = 0; i < zon.Textures.Count; ++i)
		{
			string tilsPath = rootPath + zon.Textures[i];
			Regex maptil = new Regex("3DDATA/Terrain/Tiles/([A-Z]*)/([A-Z_,0-9]*)/([A-Z_,0-9]*).DDS", RegexOptions.IgnoreCase);
			tilsPath = (NormalizePath(tilsPath));
			var tilMatches = maptil.Match(tilsPath);
			string tilesname = tilMatches.Groups[3].Value;
			string tilPath = "Assets/ROSEMdls/TILES/" + planet + "/" + map + "/" + "Til_" + tilesname.ToString() + ".PNG"; //.ToString()
			if (!File.Exists(tilPath))
			{
				//Debug.LogWarning("Could not find referenced tile texture - " + rootPath + tilMatches);
				//File.Copy(rootPath + tilMatches, tilPath);
				//AssetDatabase.ImportAsset(tilPath);
			}
		}
        /*
        for (var i = 0; i < zon.Textures.Count; ++i) {
             var tex = zon.Textures[i];
        }

		for (var i = 0; i < zon.Tiles.Count; ++i) {
			var tile = zon.Tiles[i];
            Debug.Log(
				i.ToString() + ": " +
				tile.TileType.ToString() + "," +
				tile.Layer1.ToString() + "," + 
				tile.Layer2.ToString() + "," + 
				tile.Offset1.ToString() + "," + 
				tile.Offset2.ToString() + "," +
				tile.Rotation.ToString());
            //tile.Load(zon.Textures[tile.Offset1 + tile.Layer1]);
            //tile.Load(zon.Textures[tile.Offset2 + tile.Layer2]);
            Debug.Log(zon.Textures[tile.Offset1 + tile.Layer1]);
            Debug.Log(zon.Textures[tile.Offset2 + tile.Layer2]);
        }*/
	}

	static Terrain ImportTerrain(string planet, string map, Revise.Files.ZON.ZoneFile zon, int x, int y, string CnstName, string DecoName)
	{
		var blockName = x.ToString() + "_" + y.ToString();
		var basePath = rootPath + "3DDATA/MAPS/" + planet + "/" + map + "/" + blockName;
		float blockX = (x - 32) * 160;
		float blockY = (32 - y) * 160;

		var ifo = new Revise.Files.IFO.MapDataFile();
		ifo.Load(basePath + ".IFO");

		string blockRootName = "Zone_" + x + "_" + y;

		GameObject root = GameObject.Find(blockRootName);
		if (root == null)
		{
			root = new GameObject(blockRootName);
            //root.transform.position = new Vector3(x, 0, y);
        }

		List<GameObject> temporaryAniList = new List<GameObject>();

		for (int i = 0; i < ifo.Animations.Count; ++i)
		{
			var ani = ifo.Animations[i];

			string aniName = "ANIME_" + ani.ObjectID.ToString() + " " + "( " + i.ToString() + " )";
			GameObject importedani = new GameObject(aniName);

			importedani.transform.localPosition = ifotruPosition(ani.Position);
			importedani.transform.localRotation = rtuRotation(ani.Rotation);
			importedani.transform.localScale = rtuScale(ani.Scale);
			importedani.name = aniName;
			importedani.isStatic = true;

			if (importedani != null)
			{
				temporaryAniList.Add(importedani);
			}
		}

		if (temporaryAniList.Count > 0)
		{
			Transform aniFolder = root.transform.Find("Animations");
			if (aniFolder == null)
			{
				aniFolder = new GameObject("Animations").transform;
				aniFolder.SetParent(root.transform);
			}
			foreach (var n in temporaryAniList) n.transform.SetParent(aniFolder);
		}

		List<GameObject> temporaryBldList = new List<GameObject>();

        for (int i = 0; i < ifo.Buildings.Count; ++i)
		{
            var obj = ifo.Buildings[i];
            GameObject importedbld = ImportObject(planet + "_" + CnstName, x, y, "CNST", i, obj, planet, map);

			if (importedbld != null)
			{
			temporaryBldList.Add(importedbld);
			}
		}

		if (temporaryBldList.Count > 0)
		{
			Transform bldFolder = root.transform.Find("Buildings");
			if (bldFolder == null)
			{
				GameObject goBld = new GameObject("Buildings");
				goBld.transform.SetParent(root.transform);
				goBld.transform.localPosition = Vector3.zero;
                goBld.transform.localRotation = Quaternion.identity;
                bldFolder = goBld.transform;
			}

			foreach (GameObject bld in temporaryBldList)
			{
                bld.transform.SetParent(bldFolder, false);
                //bld.transform.SetParent(bldFolder);
			}
		}

        List<GameObject> temporaryEffList = new List<GameObject>();

		for (int i = 0; i < ifo.Effects.Count; ++i)
		{
			var eff = ifo.Effects[i];

			string effName = "EFF_" + eff.ObjectID.ToString() + " " + "( " + i.ToString() + " )";
			GameObject importedeff = new GameObject(effName);

			importedeff.transform.localPosition = ifotruPosition(eff.Position);
			importedeff.transform.localRotation = rtuRotation(eff.Rotation);
			importedeff.transform.localScale = rtuScale(eff.Scale);
			importedeff.isStatic = true;

			if (importedeff != null)
			{
				temporaryEffList.Add(importedeff);
			}
        }

        if (temporaryEffList.Count > 0)
		{
			Transform effFolder = root.transform.Find("Effects");
			if (effFolder == null)
			{
				GameObject goEff = new GameObject("Effects");
				goEff.transform.SetParent(root.transform);
				goEff.transform.localPosition = Vector3.zero;
				effFolder = goEff.transform;
			}

			foreach (GameObject eff in temporaryEffList)
			{
				eff.transform.SetParent(effFolder);
			}
		}

        List<GameObject> temporaryNpcList = new List<GameObject>();

		for (int i = 0; i < ifo.NPCs.Count; ++i)
		{
			var npc = ifo.NPCs[i];

			string npcName = "NPC_" + npc.ObjectID.ToString() + " " + "( " + i.ToString() + " )";
			GameObject importednpc = new GameObject(npcName);

			importednpc.transform.position = ifotruPosition(npc.Position);
			importednpc.transform.localRotation = rtuRotation(npc.Rotation);
			importednpc.transform.localScale = rtuScale(npc.Scale);
			//importednpc.name = npcName;
			importednpc.isStatic = true;

			if (importednpc != null)
			{
				temporaryNpcList.Add(importednpc);
			}
		}

		if (temporaryNpcList.Count > 0)
		{
			Transform npcFolder = root.transform.Find("NPC-s");
			if (npcFolder == null)
			{
				npcFolder = new GameObject("NPC-s").transform;
				npcFolder.SetParent(root.transform);
			}
			foreach (var n in temporaryNpcList) n.transform.SetParent(npcFolder);
		}

		List<GameObject> temporaryObjList = new List<GameObject>();

		for (int i = 0; i < ifo.Objects.Count; ++i)
		{
			var obj = ifo.Objects[i];
			GameObject importedObj = ImportObject(planet + "_" + DecoName, x, y, "DECO", i, obj, planet, map);

			if (importedObj != null)
			{
				temporaryObjList.Add(importedObj);
			}
		}

		if (temporaryObjList.Count > 0)
		{
			Transform objFolder = root.transform.Find("Objects");
			if (objFolder == null)
			{
				GameObject goObj = new GameObject("Objects");
				goObj.transform.SetParent(root.transform);
				goObj.transform.localPosition = Vector3.zero;
                goObj.transform.localRotation = Quaternion.identity;
                objFolder = goObj.transform;
			}

			foreach (GameObject deco in temporaryObjList)
			{
				deco.transform.SetParent(objFolder);
			}
		}

        //List<GameObject> temporaryPtlList = new List<GameObject>();

        // 2. Ładowanie pliku ZSC (musisz podać ścieżkę do pliku .zsc z efektami)
        //var f = new Revise.Files.ZSC.ModelListFile();
        //string zscPath = rootPath + zscPath; // Przykład ścieżki w ROSE
        //_f = new Revise.Files.ZSC.ModelListFile();
        //_f.Load(rootPath + zscPath);
        //if (System.IO.File.Exists(zscPath))
        //{
        //f.Load(zscPath);
        // 1. Popraw ścieżkę do ZSC
        //var f = new Revise.Files.ZSC.ModelListFile();
        // Upewnij się, że zscPath zawiera tylko relatywną ścieżkę, np. "3DDATA/PARTICLES/LIST_EFFECT.ZSC"
        //string f = rootPath + zscPath;

        /*if (System.IO.File.Exists(fullZscPath))
        {
            f.Load(fullZscPath);

        // 3. Pętla po załadowanych plikach efektów
        for (int i = 0; i < f.EffectFiles.Count; ++i)
        {

            var ddsPath = rootPath + _f.EffectFiles[i];
            ddsPath = (NormalizePath(ddsPath));
            var eff = new Revise.Files.EFT.EffectFile();
            eff.Load(ddsPath);

        for (int g = 0; g < eff.Animations.Count; ++g)
        {
            var animPath = rootPath + eff.Animations[g].TextureFilePath;
            animPath = (NormalizePath(animPath));
        }
        for (int j = 0; j < eff.Particles.Count; ++j)
        {
            var effPath = rootPath + eff.Particles[j].FilePath;
            effPath = (NormalizePath(effPath));

            var ptl = new Revise.Files.PTL.ParticleFile();
            ptl.Load(effPath);

                for (int h = 0; h < ptl.Sequences.Count; ++h)
                {
                    var ptl = ptl.Sequences[h];

                    string ptlName = "PTL_" + ptl.ObjectID.ToString() + " " + "( " + i.ToString() + " )";
                    GameObject importedptl = new GameObject(ptlName);

                    importedptl.transform.localPosition = ifotruPosition(ptl.Position);
                    importedptl.transform.localRotation = rtuRotation(ptl.Rotation);
                    importedptl.transform.localScale = rtuScale(ptl.Scale);
                    importedptl.name = ptlName;
                    importedptl.isStatic = true;

                    if (importedptl != null)
                    {
                        temporaryPtlList.Add(importedptl);
                    }

                    if (temporaryPtlList.Count > 0)
                    {
                        Transform ptlFolder = root.transform.Find("Particles");


                        if (ptlFolder == null)
                        {
                            ptlFolder = new GameObject("Particles").transform;
                            ptlFolder.SetParent(root.transform);
                        }
                        foreach (var n in temporaryPtlList) n.transform.SetParent(ptlFolder);
                    }
                }	
        }
        }
        }*/
        List<GameObject> temporarySndList = new List<GameObject>();

        for (int i = 0; i < ifo.Sounds.Count; ++i)
        {
            var snd = ifo.Sounds[i];

            string sndName = "SND_" + snd.ObjectID.ToString() + " " + "( " + i.ToString() + " )";
            GameObject importedsnd = new GameObject(sndName);

            importedsnd.transform.localPosition = ifotruPosition(snd.Position);
            importedsnd.transform.localRotation = rtuRotation(snd.Rotation);
            importedsnd.transform.localScale = rtuScale(snd.Scale);
            importedsnd.name = sndName;
            importedsnd.isStatic = true;

            if (importedsnd != null)
            {
                temporarySndList.Add(importedsnd);
            }
        }

        if (temporarySndList.Count > 0)
        {
            Transform sndFolder = root.transform.Find("Sounds");
            if (sndFolder == null)
            {
                sndFolder = new GameObject("Sounds").transform;
                sndFolder.SetParent(root.transform);
            }
            foreach (var n in temporarySndList) n.transform.SetParent(sndFolder);
        }

        var tex = ImportPlanMap(planet, map, x, y);
        //var tex = ImportTilMap(planet, map, Revise.Files.ZON.ZoneFile zon);
        //var tex = ImportTiles()
        
        var him = new Revise.Files.HIM.HeightmapFile();
		him.Load(basePath + ".HIM");

        float[,] heights = new float[65,65];
		float heightMin = him.Heights[0, 0];
		float heightMax = him.Heights[0, 0];
		for (int ix = 0; ix < 65; ++ix)
		{
			for (int iy = 0; iy < 65; ++iy)
			{
				if (him.Heights[ix, iy] < heightMin)
				{
					heightMin = him.Heights[ix, iy];
				}
				if (him.Heights[ix, iy] > heightMax)
				{
					heightMax = him.Heights[ix, iy];
				}
			}
		}
		float heightBase = heightMin;
		float heightDelta = heightMax - heightMin;
		for (int ix = 0; ix < 65; ++ix)
		{
			for (int iy = 0; iy < 65; ++iy)
			{
				heights[ix, iy] = (him.Heights[64 - ix, iy] - heightBase) / heightDelta;
			}
		}

        var til = new Revise.Files.TIL.TileFile();
		til.Load(basePath + ".TIL");

		int[] texLookup = null;
		int texIndex = 0;
		Dictionary<string, int> hashLookup = new Dictionary<string, int>();
		texLookup = new int[zon.Textures.Count];

		for (var i = 1; i < zon.Textures.Count; ++i)
        {
            var tilsPath = rootPath + zon.Textures[i];
			tilsPath = (NormalizePath(tilsPath));
			if (!File.Exists(tilsPath))
			{
				//Debug.LogWarning("Could not find referenced texture - " + tilsPath);
				continue;
			}

			string texHash = hashFile(tilsPath);
			if (hashLookup.ContainsKey(texHash))
			{
				texLookup[i] = hashLookup[texHash];
				continue;
			}

			var texIdx = texIndex++;
			hashLookup[texHash] = texIdx;
			texLookup[i] = texIdx;
			
			var texPath = "Assets/ROSE Map/" + planet + "/" + map + "/Tile/" + "Til_" + texIdx.ToString() + ".PNG";
			//var texPath = _basePath + "Til_" + texIdx.ToString() + ".PNG";
			if (!File.Exists(texPath))
			{
				Directory.CreateDirectory("Assets/ROSE Map/" + planet + "/" + map + "/Tile/");
				File.Copy(tilsPath, texPath);
				AssetDatabase.ImportAsset(texPath);
			}
			//Regex maptil = new Regex("3DDATA/Terrain/Tiles/([A-Z]*)/([A-Z_,0-9]*)/([A-Z_,0-9]*).DDS", RegexOptions.IgnoreCase);
			//tilsPath = (NormalizePath(tilsPath));
			//var tilMatches = maptil.Match(tilsPath);
			//string tilesname = tilMatches.Groups[3].Value;
			//string tilPath = "Assets/ROSEMdls/TILES/" + planet + "/" + map + "/" + "Til_" + tilesname.ToString() + ".PNG"; //.ToString()
			//if (!File.Exists(tilPath))
			//{
			//Debug.LogWarning("Could not find referenced tile texture - " + rootPath + tilMatches);
			//File.Copy(rootPath + tilMatches, tilPath);
			//AssetDatabase.ImportAsset(tilPath);
			//}
		}
		//Regex maptil = new Regex("3DDATA/Terrain/Tiles/([A-Z]*)/([A-Z_,0-9]*)/([A-Z_,0-9]*).DDS", RegexOptions.IgnoreCase);
		//tilsPath = (NormalizePath(tilsPath));
		//var mapMatches = maptil.Match(tilsPath);
		//var tilesname = mapMatches.Groups[3].Value;
		//var tilPath = "Assets/ROSEMdls/TILES/" + planet + "/" + map + "/" + "Til_" + tilesname.ToString() + ".PNG"; //.ToString()
		//if (!File.Exists(tilPath))
		//{
		//Debug.LogWarning("Could not find referenced tile texture - " + tilsPath);
		//continue;

		//Debug.LogWarning("Could not find referenced tile texture - " + tilsPath);
		//}
	//}

		//return AssetDatabase.LoadAssetAtPath(tilPath, typeof(Texture2D)) as Texture2D;
		//if (!File.Exists(assetPath))
		//{
		//File.Copy(rootPath + "3DDATA/MAPS/" + mapPath, assetPath);
		//AssetDatabase.ImportAsset(assetPath);
		//}
		//if (!File.Exists(tilPath))
		//{
		//File.Copy(tilsPath, tilPath);
		//AssetDatabase.ImportAsset(tilPath);
		//}
		//else
		//{
		//Debug.LogWarning("Could not find referenced tile texture - " + tilPath);
		//}
		

		for (int ix = 0; ix < til.Width; ++ix) 
		{
			for (int iy = 0; iy < til.Height; ++iy)
			{
				var t = til[ix, iy].Tile;
				Debug.Log
				(
					til.ToString() + ": " +
					til[ix, iy].Brush.ToString() + "," +
					til[ix, iy].TileSet.ToString() + "," +
					til[ix, iy].TileIndex.ToString() + "," +
					til[ix, iy].Tile.ToString()
				);
				Debug.Log
				(
					zon.ToString() + ": " +
					zon.Tiles[t].Layer1.ToString() + "," +
					zon.Tiles[t].Offset1.ToString() + "," +
					zon.Tiles[t].Layer2.ToString() + "," +
					zon.Tiles[t].Offset2.ToString() + "," +
					zon.Tiles[t].BlendingEnabled.ToString() + "," +
					//zon.Tiles[t].TileType.ToString() + "," +
					zon.Tiles[t].TileType.ToString() + "," +
					zon.Tiles[t].Rotation.ToString()
				);

				Debug.Log(zon.Textures[zon.Tiles[t].Layer1 + zon.Tiles[t].Offset1]);
				Debug.Log(zon.Textures[zon.Tiles[t].Layer2 + zon.Tiles[t].Offset2]);
				/*
                //int height, width;  // these must be powers of 2 to be compatible with iPhone
                //if (atlasRectHash.Count <= 16) width = height = 4 * 256;
                //Texture2D atlas = new Texture2D(width, height);
                //Texture2D myAtlas = new Texture2D(width, height);
                Texture2D atlas = new Texture2D(ix, iy);
                var tile_index1 = (zon.Textures[zon.Tiles[t].Layer1 + zon.Tiles[t].Offset1]);
                //var pixel1 = tile_index1.GetPixels(ix, iy);
                //myAtlas.SetPixels(tile_image1.GetPixels(x, y);
                //var pixel1 = tile_image1.GetPixels(x, y);
                var td1 = new TerrainData();
                td1.size = new Vector3(80, heightDelta / 100, 80);
                td1.heightmapResolution = 65;
                td1.SetHeights(0, 0, heights);
                //var ts = new SplatPrototype[1];
                //ts[0] = new SplatPrototype();
                var ts1 = new TerrainLayer[1];
                ts1[0] = new TerrainLayer();
                //ts1[0].texture = tile_index1;
                ts1[0].diffuseTexture = tile_index1;

                ts1[0].tileSize = new Vector2(160, 160);
                //td.splatPrototypes = ts;
                td1.terrainLayers = ts1;
				*/
            }
        }

        /*// 1. Tworzymy TerrainData i ustawiamy rozdzielczość (musi być 65 dla HIM 65x65)
        TerrainData tData = new TerrainData();
        tData.heightmapResolution = 65;
        tData.size = new Vector3(160, heightDelta, 160); // heightDelta to skala wysokości
        tData.SetHeights(0, 0, heights);

        // 2. Tworzymy fizyczny obiekt w hierarchii Unity
        GameObject terrainGo = Terrain.CreateTerrainGameObject(tData);
        terrainGo.name = blockName;

        // 3. Ustawiamy pozycję w świecie (blockX i blockY już obliczyłeś)
        terrainGo.transform.position = new Vector3(blockX, heightBase, blockY);

        // 4. Przypisujemy teksturę z ImportPlanMap
        if (tex != null)
        {
            TerrainLayer layer = new TerrainLayer();
            layer.diffuseTexture = tex;
            tData.terrainLayers = new TerrainLayer[] { layer };
        }

        // 5. Zapisujemy dane jako asset, żeby nie zniknęły
        if (!System.IO.Directory.Exists("Assets/ROSE Map"))
            System.IO.Directory.CreateDirectory("Assets/ROSE Map");

        UnityEditor.AssetDatabase.CreateAsset(tData, "Assets/ROSE Map/Data_" + blockName + ".asset");
        UnityEditor.AssetDatabase.SaveAssets();

        return terrainGo.GetComponent<Terrain>();

        var td = new TerrainData();
		td.size = new Vector3(80, heightDelta/100, 80);
		td.heightmapResolution = 65;
		td.SetHeights(0, 0, heights);
        //var ts = new SplatPrototype[1];
        //ts[0] = new SplatPrototype();
        var ts = new TerrainLayer[1];
        ts[0] = new TerrainLayer();
		//ts[0].texture = tex;
		ts[0].diffuseTexture = tex;

        ts[0].tileSize = new Vector2(160, 160);
        //td.splatPrototypes = ts;
        td.terrainLayers = ts;

        var ter = Terrain.CreateTerrainGameObject(td).GetComponent<Terrain>();
		ter.name = blockName;

		ter.transform.localPosition = new Vector3(blockX, heightBase/100, blockY);
        //Debug.LogWarning(ter);
        // 1. Upewnij się, że folder istnieje
        if (!System.IO.Directory.Exists("Assets/ROSE Map"))
            System.IO.Directory.CreateDirectory("Assets/ROSE Map");

        // 2. ZAPISZ DANE NA DYSKU (Bez tego teren nie "przetrwa")
        UnityEditor.AssetDatabase.CreateAsset(td, "Assets/ROSE Map/TData_" + blockName + ".asset");
        UnityEditor.AssetDatabase.SaveAssets();

        return ter;*/

        var td = new TerrainData();

        //td.size = new Vector3(160, heightDelta/100, 160);
        td.size = new Vector3(80, heightDelta/100, 80);
        td.heightmapResolution = 65;
        td.SetHeights(0, 0, heights);

        if (tex != null)
        {
            TerrainLayer layer = new TerrainLayer();
            layer.diffuseTexture = tex;

            // Rozmiar 160x160 sprawi, że PLANMAP pokryje dokładnie cały blok
            layer.tileSize = new Vector2(160, 160);

            // Unity WYMAGA, aby każda warstwa (Layer) była zapisana jako osobny plik .terrainlayer
            //string layerPath = "Assets/ROSE Map/Layer_" + blockName + ".terrainlayer";
            //UnityEditor.AssetDatabase.CreateAsset(layer, layerPath);

            // Przypisujemy warstwę do tablicy warstw terenu
            td.terrainLayers = new TerrainLayer[] { layer };
        }

        // 3. Tworzenie obiektu w scenie
        var terGo = Terrain.CreateTerrainGameObject(td);
        var ter = terGo.GetComponent<Terrain>();
        ter.name = blockName;
        ter.transform.localPosition = new Vector3(blockX, heightBase/100, blockY);
        //terGo.transform.position = new Vector3(blockX, heightBase / 100f, blockY);
        //Debug.LogWarning("Could not find referenced tile texture - " + blockName);
        // 4. Zapisanie danych terenu na dysku
        //UnityEditor.AssetDatabase.CreateAsset(td, "Assets/ROSE Map/TData_" + blockName + ".asset");

        List<GameObject> temporaryZonList = new List<GameObject>();
        if (terGo != null)
        {
            temporaryZonList.Add(terGo);
        }

        if (temporaryZonList.Count > 0)
        {
            Transform zonFolder = root.transform.Find("Zone");
            if (zonFolder == null)
            {
                GameObject goZon = new GameObject("Zone");
                goZon.transform.SetParent(root.transform);
                // Folder Zone zazwyczaj zostawia się w (0,0,0), a kafelki ustawia w świecie
                goZon.transform.localPosition = Vector3.zero;
                zonFolder = goZon.transform;
            }

            foreach (GameObject zone in temporaryZonList)
            {
                // Zachowujemy pozycję world (true), aby kafelek nie uciekł po przypisaniu do folderu
                zone.transform.SetParent(zonFolder, true);
            }
        }

        UnityEditor.AssetDatabase.SaveAssets();

        return ter;

        /*List<GameObject> temporaryZonList = new List<GameObject>();

        var zoneObject = Terrain.CreateTerrainGameObject(td);
        var ter1 = zoneObject.GetComponent<Terrain>();
        ter1.name = blockName;
        //zoneObject.transform.position = new Vector2(blockX, blockY);

        if (zoneObject != null)
        {
            temporaryZonList.Add(zoneObject);
        }

        if (temporaryZonList.Count > 0)
        {
            Transform zonFolder = root.transform.Find("Zone");
            if (zonFolder == null)
            {
                GameObject goZon = new GameObject("Zone");
                goZon.transform.SetParent(root.transform);
                goZon.transform.localPosition = new Vector2(blockX, blockY);//.zero; // Folder nadrzędny w (0,0,0)
                zonFolder = goZon.transform;
            }

            foreach (GameObject zone in temporaryZonList)
            {
                // Używamy 'true', aby zachować pozycję blockX/blockY w świecie po zmianie rodzica
                zone.transform.SetParent(zonFolder, true);
            }
        }*/

        /*var ts = new TerrainLayer[1];
        ts[0] = new TerrainLayer();
        //ts[0].texture = tex;
        ts[0].diffuseTexture = tex;
        ts[0].tileSize = new Vector2(160, 160);
        td.terrainLayers = ts;

        // 2. ZAPISZ ASSET (Bez tego teren będzie niewidoczny/pusty)
        string folderPath = "Assets/ROSE Map";
        if (!System.IO.Directory.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);
        UnityEditor.AssetDatabase.CreateAsset(td, folderPath + "/TData_" + blockName + ".asset");

        var terGo = Terrain.CreateTerrainGameObject(td);
        var ter = terGo.GetComponent<Terrain>();
        ter.name = blockName;

        // 3. Pozycja - usuń /100. Teren musi być na wysokości heightBase
        ter.transform.localPosition = new Vector3(blockX, heightBase/100, blockY);

        UnityEditor.AssetDatabase.SaveAssets();
        return ter;*/
    }


    static Vector3 rtuPosition(Vector3 v) {
		return new Vector3(v.x, v.z, v.y);
	}
	static Quaternion rtuRotation(Quaternion q) {
		Vector3 v;
		float a;
		q.ToAngleAxis(out a, out v);
		return Quaternion.AngleAxis(-a, new Vector3(v.x, v.z, v.y));
	}
	static Vector3 rtuScale(Vector3 v) {
		return new Vector3(v.x, v.z, v.y);
	}
}


using UnityEngine;
using UnityEditor;
using System.Collections;

public class map
{
    public static void ImportMapCoord1(string WorldName, string map1)
    {
            if (WorldName + "/" + map1 == "JUNON/JDT01") // Canyon City of Zant
            {
                //Debug.LogWarning("Test: Coord " + WorldName + "/" + map1);
                //Debug.LogWarning("Test: Coord " + ix + "/" + iy);
                for (int ix = 31; ix <= 34; ++ix)
                {
                    for (int iy = 30; iy <= 33; ++iy)
                    {
                    //Debug.LogWarning("Test: Coord " + ix + ", " + iy + " - " + WorldName + "/" + map1);
                    //var Coord = ImportTerrain(ix, iy);
                    //Main.ImportMapCoord(ix ,iy);
                    //ROSEImportMap.ImportMapCoord(ix ,iy);
                    //ROSEImportMap.OnGUI(ix, iy);
                    }
                }
                //int test2 = 34;
                //int test3 = 30;
                //int test4 = 33;
                //ROSEImportWindow.ImportMapName(test1);
                //Main.ImportMapCoord(test1);//, test2, test3, test4);
                //Main.ImportMapName(test1, test2, test3, test4);
            }
            //if (WorldName + "/" + map1 == "JUNON/JPT01") // City of Junon Polis
            //{
                //Debug.LogWarning("Test: Coord " + WorldName + "/" + map1);
                //for (int ix = 30; ix <= 37; ++ix)
                //{
                    //for (int iy = 30; iy <= 35; ++iy)
                    //{
                        //Main.ImportMapCoord(ix, iy);
                    //}
                //}
                //int test1 = 30;
                //int test2 = 37;
                //int test3 = 30;
                //int test4 = 35;
                //Main.ImportMapCoord(test1);//, test2, test3, test4);
            //}
        
    }
    //public static void ImportMapCoord1(int test1, int test2, int test3, int test4)
    //public static void ImportMapCoord1( int test2)//, int test2, int test3, int test4)
    //{
        //Debug.LogWarning("Test1: " + test1);// + ", " + test2 + ", " + test3 + ", " + test4);
        //public static string GetDataPath(int test1)
    //}
}
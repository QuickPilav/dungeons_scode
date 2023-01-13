using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TopMenuEditor : EditorWindow
{

    [MenuItem("WAVE_WORLD/Show Saves")]
    public static void ShowSaves ()
    {
        ShowExplorer(Application.persistentDataPath);
    }
    
    public static void ShowExplorer(string itemPath)
    {
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe",itemPath);
    }
}

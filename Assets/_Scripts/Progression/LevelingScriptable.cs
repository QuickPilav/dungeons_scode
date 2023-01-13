using RoboRyanTron.QuickButtons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression System/New Leveling")]
public class LevelingScriptable : ScriptableObject
{
    [System.Serializable]
    public class Level
    {
        public int xpRequired;
        public int coinGives;
    }

    public Level[] levels;

#if UNITY_EDITOR
    public QuickButton ReadFromAFile = new QuickButton(nameof(ReadFromFile));

    public void ReadFromFile ()
    {
        string txtPath = UnityEditor.EditorUtility.OpenFilePanel("Dosya seç", "", "txt");

        if (string.IsNullOrEmpty(txtPath))
            return;

        string text = System.IO.File.ReadAllText(txtPath);

        string[] lines = text.Split('\n');

        Debug.Log(lines.Length);

        try
        {
            List<Level> levels = new List<Level>();

            foreach (string line in lines)
            {
                var lineSeperators = line.Split('_');

                for (int i = 0; i < lineSeperators.Length; i++)
                {
                    Debug.Log(lineSeperators[i]);
                }

                levels.Add(new Level()
                {
                    xpRequired = int.Parse(lineSeperators[1]),
                    coinGives = int.Parse(lineSeperators[2])
                });
            }

            this.levels = levels.ToArray();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Bir hata çýktý! {ex.Message}");
            throw;
        }
    }
#endif
}

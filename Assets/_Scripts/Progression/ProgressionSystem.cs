using System;
using System.Linq;
using UnityEngine;

public class ProgressionSystem : MonoBehaviour
{
    public static InstantEvent<VisualLevel> OnExperienceUpdated = new InstantEvent<VisualLevel>(() => saveLoaded, () => GetVisualLevel(), true);

    private static int loadedExperience;

    [SerializeField] private LevelingScriptable levelScriptable;
    private static LevelingScriptable LevelScriptable;

    private static int lastLVString;
    private static bool saveLoaded;

    public void Initialize()
    {
        LevelScriptable = levelScriptable;
        SaveSocket.OnSaveDataLoadedForTheFirstTime.SubscribeToEvent(OnSaveLoaded);
    }

    //DEBUG
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AddExperience(1);
        }
    }

    private void OnSaveLoaded(SaveData save)
    {
        loadedExperience = save.xp;

        lastLVString = OnExperienceUpdated.Invoke().currentLevelString;
        saveLoaded = true;
    }

    public static void AddExperience(int xp)
    {
        if (xp <= 0)
            return;

        loadedExperience += xp;

        var visual = OnExperienceUpdated.Invoke();

        if (visual.currentLevelString != lastLVString)
        {
            //SEVITE ATLADIN
            SaveSocket.CurrentSave.points += visual.currentLevel.coinGives;
            ClientUI.PopupInstance.ShowOnlyTextPopup("TEBRÝKLER!", $"{visual.currentLevelString} level oldun ve {visual.currentLevel.coinGives}<sprite=0> kazandýn!", true, 5);
        }

        SaveSocket.CurrentSave.xp = loadedExperience;
        SaveSocket.Save();

        lastLVString = visual.currentLevelString;
    }

    private static VisualLevel GetVisualLevel()
    {
        var lv = LevelScriptable.levels.Where(x => loadedExperience >= x.xpRequired).Last();
        int nextLvIndex = Mathf.Min(Array.IndexOf(LevelScriptable.levels, lv) + 1, LevelScriptable.levels.Length - 1);


        return new VisualLevel(lv, nextLvIndex, LevelScriptable.levels[nextLvIndex], loadedExperience - lv.xpRequired,loadedExperience);
    }

    public struct VisualLevel
    {

        public LevelingScriptable.Level currentLevel;
        public int currentLevelString;
        public LevelingScriptable.Level nextLevel;
        public int remainingXp;
        public int loadedXp;

        public VisualLevel(LevelingScriptable.Level currentLevel, int currentLevelString, LevelingScriptable.Level nextLevel, int remainingXp, int loadedXp)
        {
            this.currentLevel = currentLevel;
            this.currentLevelString = currentLevelString;
            this.nextLevel = nextLevel;
            this.remainingXp = remainingXp;
            this.loadedXp = loadedXp;
        }
    }
}

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class InstantEvent
{
    protected Action action;
    protected readonly Func<bool> executeDirectlyIf;
    private readonly bool subscribeAnyway;

    public InstantEvent(Func<bool> executeDirectlyIf, bool subscribeAnyway)
    {
        this.executeDirectlyIf = executeDirectlyIf;
        this.subscribeAnyway = subscribeAnyway;
    }

    public virtual void SubscribeToEvent(Action act)
    {
        if (executeDirectlyIf())
        {
            act();

            if (subscribeAnyway)
            {
                action += act;
            }
        }
        else
        {
            action += act;
        }
    }

    public virtual void UnsubscribeToEvent(Action act)
    {
        action -= act;
    }

    public virtual void Invoke()
    {
        action?.Invoke();
    }

}

public class InstantEvent<T>
{
    private readonly Func<bool> executeDirectlyIf;
    private readonly Func<T> value;
    private readonly bool subscribeAnyway;
    protected Action<T> action;

    public InstantEvent(Func<bool> executeDirectlyIf, Func<T> value, bool subscribeAnyway)
    {
        this.executeDirectlyIf = executeDirectlyIf;
        this.subscribeAnyway = subscribeAnyway;
        this.value = value;
    }

    public void SubscribeToEvent(Action<T> act)
    {
        if (executeDirectlyIf())
        {
            act(value());

            if (subscribeAnyway)
            {
                action += act;
            }
        }
        else
        {
            action += act;
        }
    }

    public void UnsubscribeToEvent(Action<T> act)
    {
        action -= act;
    }

    public T Invoke()
    {
        var v = value();
        action?.Invoke(v);
        return v;
    }
}

public enum Language
{
    Turkish,
    English
}

[System.Serializable]
public struct SettingsSave
{
    public int resIndex;
    public bool fullscreen;
    public float sfx;
    public float sensitivity;
    public bool autoAim;
    public bool toggleAds;
    public Language language;
}

[System.Serializable]
public struct Stats
{
    public int WavesSurvived;
}

[System.Serializable]
public class SaveData
{
    public SettingsSave settings;
    public int points;
    public int xp;
    public MissionHandler.MissionSaveData[] missionsSaved;
    public CharactersUI.UnlockablePlayerClasses unlockedCharacters;
    public Stats stats;

    public static SaveData GetDefaultSave()
    {
        return new SaveData()
        {
            settings = new SettingsSave()
            {
                sensitivity = 1f,
                sfx = .6f,
                toggleAds = false,
                language = Language.Turkish,
                autoAim = true,
                fullscreen = true,
                resIndex = 0,
            },
            unlockedCharacters = CharactersUI.UnlockablePlayerClasses.classMami,
            missionsSaved = new MissionHandler.MissionSaveData[0],
            points = 0,
            xp = 0,
            stats = new Stats()
            {
                WavesSurvived = 0,
            },
        };
    }
}

public static class SaveSocket
{
#if UNITY_EDITOR
    private static bool SAVE_AS_JSON = true;
#else
    private static bool SAVE_AS_JSON = false;
#endif

    public static InstantEvent<SaveData> OnSaveDataLoadedForTheFirstTime = new InstantEvent<SaveData>(() => saveDataLoadedOnce, () => currentSave, false);
    public static InstantEvent<SettingsSave> OnSettingsChanged = new InstantEvent<SettingsSave>(() => saveDataLoadedOnce, () => currentSave.settings, true);

    public static SaveData CurrentSave
    {
        get
        {
            return currentSave;
        }
    }

    public static InstantEvent OnGameSaved = new InstantEvent(() => saveDataLoadedOnce, true);

    private static SaveData currentSave;
    private static bool saveDataLoadedOnce;

    public static event Action BeforeSavingEvent;

    public static string GetBinarySaveDirectory() => $"{Application.persistentDataPath}/MySaveData.bin";
    public static string GetJSONSaveDirectory() => $"{Application.persistentDataPath}/MySaveData.json";

    public static void Save()
    {
        BeforeSavingEvent?.Invoke();

        if (SAVE_AS_JSON)
        {
            string json = JsonUtility.ToJson(currentSave, true);

            File.WriteAllText(GetJSONSaveDirectory(), json);
        }
        else
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Create(GetBinarySaveDirectory());

            formatter.Serialize(saveFile, currentSave);

            saveFile.Close();
        }

        OnSettingsChanged.Invoke();

        OnGameSaved.Invoke();

        Debug.Log("Game data saved!");
    }

    public static void Load()
    {
        if (SAVE_AS_JSON)
        {
            if (File.Exists(GetJSONSaveDirectory()))
            {
                currentSave = JsonUtility.FromJson<SaveData>(File.ReadAllText($"{Application.persistentDataPath}/MySaveData.json"));
            }
            else
            {
                Debug.LogWarning("No game data found, creating new one!");
                currentSave = SaveData.GetDefaultSave();
                Save();
            }
        }
        else
        {
            if (File.Exists(GetBinarySaveDirectory()))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                FileStream saveFile = File.Open(GetBinarySaveDirectory(), FileMode.Open);

                currentSave = (SaveData)formatter.Deserialize(saveFile);

                saveFile.Close();
            }
            else
            {
                Debug.LogWarning("No game data found, creating new one!");
                currentSave = SaveData.GetDefaultSave();
                Save();
            }
        }

        Debug.Log("Save data loaded!");

        saveDataLoadedOnce = true;

        OnSaveDataLoadedForTheFirstTime.Invoke();
        OnSettingsChanged.Invoke();
    }

}

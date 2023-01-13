using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionHandler : StaticInstance<MissionHandler>
{
    public const int MISSION_NOT_STARTED_STATE = 0;
    public const int MISSION_STARTED_STATE = 1;
    public const int MISSION_FINISHED_STATE = 2;

    [Serializable]
    public struct MissionSaveData
    {
        public MissionSaveData(string missionId, int missionState)
        {
            this.missionId = missionId;
            this.missionState = missionState;
        }

        public string missionId;
        public int missionState;
    }

    public static Dictionary<string, MissionBase> MissionsLoaded;
    public static Dictionary<string, MissionBase> MissionsActive;
    public static Dictionary<string, MissionBase> MissionsCompleted;

    private static bool initialized;

    public static InstantEvent<Dictionary<string, MissionBase>> OnMissionsLoaded = new InstantEvent<Dictionary<string, MissionBase>>(() => initialized, () => MissionsLoaded, false);
    public void Initialize()
    {
        MissionsLoaded = new Dictionary<string, MissionBase>();
        MissionsActive = new Dictionary<string, MissionBase>();
        MissionsCompleted = new Dictionary<string, MissionBase>();

        SaveSocket.OnSaveDataLoadedForTheFirstTime.SubscribeToEvent(LoadMissions);
        SaveSocket.BeforeSavingEvent += () => SaveSocket.CurrentSave.missionsSaved = SaveMissions();
    }
    private static MissionSaveData[] SaveMissions()
    {
        List<MissionSaveData> saveDatas = new List<MissionSaveData>();

        foreach (var item in MissionsLoaded)
        {
            if (item.Value.MissionState == MISSION_NOT_STARTED_STATE)
                continue;
            
            if (item.Value.ActivatedByDefault && item.Value.MissionState == MISSION_STARTED_STATE)
                continue;

            Debug.Log("this here gets called");

            saveDatas.Add(new MissionSaveData(item.Key, item.Value.MissionState));
        }

        return saveDatas.ToArray();
    }
    private static void LoadMissions(SaveData save)
    {
        foreach (var item in ResourceManager.GetMissionBases())
        {
            Debug.Log($"adding \"{item.MissionId}\" to missions");
            MissionsLoaded.Add(item.MissionId, item);
        }

        foreach (var item in save.missionsSaved)
        {
            if (!MissionsLoaded.TryGetValue(item.missionId, out MissionBase mission))
                continue;

            if (item.missionState == MISSION_FINISHED_STATE)
            {
                mission.SetStateAsCompleted();
                MissionsCompleted.Add(item.missionId, mission);
            }
            else if (item.missionState == MISSION_NOT_STARTED_STATE)
            {
                Debug.LogWarning("DAHA BAÞLAMAMIÞ BÝR GÖREVÝ NASIL EKLÝYORUZ???");
                continue;
            }
            else
            {
                MissionsActive.Add(item.missionId, mission);
            }
        }
        initialized = true;

        OnMissionsLoaded.Invoke();

        foreach (var item in MissionsLoaded)
        {
            item.Value.Initialize();
        }
    }
    public static void StartMission(string missionId)
    {
        if (MissionsCompleted.ContainsKey(missionId))
        {
            Debug.Log("this mission is already completed");
            return;
        }

        if (!MissionsLoaded.TryGetValue(missionId, out var value))
        {
            Debug.Log("this mission has not been loaded yet!");
            return;
        }

        MissionsActive.Add(missionId, value);
        value.StartMission();
    }
    public static void CompleteMission(string missionId)
    {
        if (!MissionsActive.ContainsKey(missionId))
            return;

        MissionsActive.Remove(missionId, out var mission);
        mission.SetAsComplete(true);
        MissionsCompleted.Add(missionId, mission);
    }
}

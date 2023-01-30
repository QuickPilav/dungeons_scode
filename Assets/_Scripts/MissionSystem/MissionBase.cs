using System;
using UnityEngine;

public class MissionBase
{
    public enum MissionType
    {
        ValueReach
    }

    public string MissionId { get; private set; }
    public int MissionState { get; private set; }
    public bool ActivatedByDefault { get => activatedByDefault; }

    public MissionBase(string missionId, bool checkConditionAtActivation, Action<MissionBase> OnMissionStarted, Action<MissionBase, bool> OnMissionEnd, Func<ReachValueDetails> CheckDidReachValue, MissionVisual mVisual, bool activatedByDefault)
    {
        this.MissionId = missionId;
        this.CheckDidReachValue = CheckDidReachValue;
        this.OnMissionEnd = OnMissionEnd;
        this.OnMissionStarted = OnMissionStarted;
        this.checkConditionAtActivation = checkConditionAtActivation;
        this.mVisual = mVisual;
        this.activatedByDefault = activatedByDefault;

        missionType = MissionType.ValueReach;

        MissionState = MissionHandler.MISSION_NOT_STARTED_STATE;
    }

    public void Initialize ()
    {
        if (activatedByDefault)
        {
            MissionHandler.StartMission(MissionId);
        }
    }

    private readonly MissionType missionType;
    private readonly bool checkConditionAtActivation;
    private readonly bool activatedByDefault;
    private readonly Action<MissionBase> OnMissionStarted;
    /// <summary>if objective was complete, return true, otherwise false</summary>
    private readonly Action<MissionBase, bool> OnMissionEnd;
    /// <summary>did it reach?, current?, target?</summary>
    private readonly Func<ReachValueDetails> CheckDidReachValue;

    private MissionVisual mVisual;

    public void StartMission()
    {
        Debug.Log("starting mission");
        OnMissionStarted?.Invoke(this);
        MissionState = MissionHandler.MISSION_STARTED_STATE;

        if (checkConditionAtActivation)
        {
            CheckState();
        }
    }

    public void CheckState()
    {
        var a = CheckDidReachValue.Invoke();

        mVisual.currentValue = a.currentValue;
        mVisual.targetValue = a.targetValue;

        Debug.Log($"checking mission \"{MissionId}\"");

        //if CheckDidReachValue returns true...
        if (a.isCompleted)
        {
            MissionHandler.CompleteMission(MissionId);
        }
    }

    public void SetAsComplete(bool wasCompleted)
    {
        if (MissionState == MissionHandler.MISSION_FINISHED_STATE)
            return;

        Debug.Log("setting mission as complete");

        SetStateAsCompleted();
        OnMissionEnd?.Invoke(this, wasCompleted);

        SaveSocket.Save();
    }

    public void SetStateAsCompleted ()
    {
        MissionState = MissionHandler.MISSION_FINISHED_STATE;
        mVisual.isCompleted = true;
    }

    public MissionVisual GetVisual()
    {
        return mVisual;
    }

    public struct ReachValueDetails
    {
        public bool isCompleted;
        public int currentValue;
        public int targetValue;
    }

    public struct MissionVisual
    {
        public bool isCompleted;
        public int currentValue;
        public int targetValue;

        public TranslationScriptable missionName;
        public TranslationScriptable missionDescription;

        public MissionVisual(bool isCompleted, TranslationScriptable missionName, TranslationScriptable missionDescription)
        {
            this.isCompleted = isCompleted;
            this.missionName = missionName;
            this.missionDescription = missionDescription;

            this.currentValue = 0;
            this.targetValue = 0;
        }
    }

}

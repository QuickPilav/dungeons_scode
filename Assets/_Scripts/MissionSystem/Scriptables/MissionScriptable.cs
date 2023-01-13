using UnityEngine;

public abstract class MissionScriptable : ScriptableObject
{
    public string missionId;

    [SerializeField] protected bool activatedByDefault;
    [SerializeField] protected string missionName;

    [SerializeField, TextArea] protected string missionDescription;
    public abstract MissionBase GetMission();
}

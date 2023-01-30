using UnityEngine;

public abstract class MissionScriptable : ScriptableObject
{
    public string missionId;

    [SerializeField] protected bool activatedByDefault;
    [SerializeField] protected TranslationScriptable missionName;

    [SerializeField] protected TranslationScriptable missionDescription;
    public abstract MissionBase GetMission();
}

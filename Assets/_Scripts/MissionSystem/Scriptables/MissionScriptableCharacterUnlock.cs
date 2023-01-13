using UnityEngine;

[CreateAssetMenu(menuName = "Missions/New Character Unlock Mission")]
public class MissionScriptableCharacterUnlock : MissionScriptable
{
    [SerializeField, Range(0, 50)] private int waveReachAmount;
    [SerializeField] private PlayerClassScriptable classToUnlock;

    public override MissionBase GetMission()
    {
        return new MissionBase(
            missionId: missionId,
            checkConditionAtActivation: false,
            OnMissionStarted: (mBase) =>
            {
                SaveSocket.OnGameSaved.SubscribeToEvent(mBase.CheckState);
            },
            OnMissionEnd: (mBase, wasCompleted) =>
            {
                SaveSocket.OnGameSaved.UnsubscribeToEvent(mBase.CheckState);

                if (wasCompleted)
                {
                    CharactersUI.UnlockCharacter(classToUnlock,false);
                }
            },
            CheckDidReachValue: () =>
            {
                int currentValue = SaveSocket.CurrentSave.stats.WavesSurvived;

                MissionBase.ReachValueDetails reachDetails = new MissionBase.ReachValueDetails
                {
                    currentValue = currentValue,
                    targetValue = waveReachAmount,
                    isCompleted = currentValue >= waveReachAmount
                };

                return reachDetails;
            },
            mVisual: new MissionBase.MissionVisual(false,missionName, missionDescription),
            activatedByDefault: activatedByDefault
            );

    }
}

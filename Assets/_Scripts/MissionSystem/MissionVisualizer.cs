using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionVisualizer : MonoBehaviour
{
    [SerializeField] private MissionScriptable scriptable;

    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private TextMeshProUGUI missionDescriptionText;
    [SerializeField] private TextMeshProUGUI missionSliderText;
    [SerializeField] private Slider missionSlider;

    [SerializeField] private GameObject notCompleted;
    [SerializeField] private GameObject completed;


    private void Start()
    {
        MissionHandler.OnMissionsLoaded.SubscribeToEvent(OnMissionsLoaded);
    }

    private void OnDestroy()
    {
        MissionHandler.OnMissionsLoaded.UnsubscribeToEvent(OnMissionsLoaded);
    }

    private void OnMissionsLoaded(Dictionary<string, MissionBase> missions)
    {
        Debug.Log($"mission \"{scriptable.missionId}\" is loaded");

        if (!missions.TryGetValue(scriptable.missionId, out var m))
        {
            Debug.Log($"{scriptable.missionId} was not found on missionsList!");
            return;
        }

        var visual = m.GetVisual();

        missionDescriptionText.text = visual.missionDescription;
        missionNameText.text = visual.missionName;
        missionSliderText.text = $"{visual.currentValue}/{visual.targetValue}";

        missionSlider.minValue = 0;
        if (visual.isCompleted)
        {
            missionSlider.maxValue = 1;
            missionSlider.value = 1;
        }
        else
        {
            missionSlider.maxValue = visual.targetValue;
            missionSlider.value = visual.currentValue;
        }

        notCompleted.SetActive(!visual.isCompleted);
        completed.SetActive(visual.isCompleted);
    }
}

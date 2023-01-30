using System;
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

    private MissionBase mBase;


    private void Start()
    {
        MissionHandler.OnMissionsLoaded.SubscribeToEvent(OnMissionsLoaded);
        SaveSocket.OnSettingsChanged.SubscribeToEvent(OnSettingsChanged);
    }
    private void OnDestroy()
    {
        MissionHandler.OnMissionsLoaded.UnsubscribeToEvent(OnMissionsLoaded);
        SaveSocket.OnSettingsChanged.UnsubscribeToEvent(OnSettingsChanged);
    }

    private void OnSettingsChanged(SettingsSave settings)
    {
        if (mBase == null)
            return;

        var visual = mBase.GetVisual();

        missionDescriptionText.text = visual.missionDescription.GetTranslationOf(settings.language);
        missionNameText.text = visual.missionName.GetTranslationOf(settings.language);
    }

    private void OnMissionsLoaded(Dictionary<string, MissionBase> missions)
    {
        Debug.Log($"mission \"{scriptable.missionId}\" is loaded");

        if (!missions.TryGetValue(scriptable.missionId, out var m))
        {
            Debug.Log($"{scriptable.missionId} was not found on missionsList!");
            return;
        }

        mBase = m;

        var visual = m.GetVisual();

        var lng = SaveSocket.CurrentSave.settings.language;

        missionDescriptionText.text = visual.missionDescription.GetTranslationOf(lng);
        missionNameText.text = visual.missionName.GetTranslationOf(lng);
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

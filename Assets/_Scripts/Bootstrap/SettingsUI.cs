using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SettingsUI
{
    public enum SliderSettings
    {
        Sensitivity,
        Volume,
    }
    public enum ToggleSettings
    {
        ADS,
        AutoAim,
        Language,
        Fullscreen
    }
    public enum DropdownSettings
    {
        Resolution
    }

    private ClientUI clientUi;

    public bool IsSettingsMenuOpen
    {
        get => isSettingsMenuOpen;
        set
        {
            if (!value && changesMade)
            {
                ClientUI.PopupInstance.ShowAreYouSurePopup("Deðiþiklikleri kaydetmek istediðine emin misin?"
                , OnClickedYes: () =>
                {
                    changesMade = false;
                    IsSettingsMenuOpen = false;
                    SaveSocket.CurrentSave.settings = settingsOnMemory;
                    SaveSocket.Save();

                    UpdateSettings(settingsOnMemory);
                }, OnClickedNo: () =>
                {
                    changesMade = false;
                    IsSettingsMenuOpen = false;

                    RevertSettings(SaveSocket.CurrentSave.settings);
                }, isNotification: false);
                return;
            }

            isSettingsMenuOpen = value;

            clientUi.normalPauseMenu.SetActive(!value);
            settingsMenu.SetActive(value);
        }
    }

    private bool changesMade;

    private bool isSettingsMenuOpen;

    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider sfxSlider;
    [Space]
    [SerializeField] private Toggle adsSetting;
    [SerializeField] private Toggle autoAimSetting;
    [SerializeField] private Toggle languageSetting;
    [SerializeField] private Toggle fullscreenSetting;
    [Space]
    [SerializeField] private TMP_Dropdown resolutionSetting;

    private SettingsSave settingsOnMemory;

    private List<Resolution> resolutionsLoaded;

    public void Initialize(ClientUI clientUi)
    {
        this.clientUi = clientUi;

        resolutionsLoaded = GetResolutions();

        resolutionSetting.ClearOptions();
        resolutionSetting.AddOptions(Array.ConvertAll(resolutionsLoaded.ToArray(), delegate (Resolution r)
        { return new TMP_Dropdown.OptionData($"{r.width} x {r.height} : {r.refreshRate}"); }).ToList());

        SaveSocket.OnSaveDataLoadedForTheFirstTime.SubscribeToEvent((set) =>
        {
            settingsOnMemory = set.settings;
            RevertSettings(settingsOnMemory);
        });

    }


    private void RevertSettings(SettingsSave toRevert)
    {
        sensitivitySlider.SetValueWithoutNotify(toRevert.sensitivity);
        sfxSlider.SetValueWithoutNotify(toRevert.sfx);
        adsSetting.SetIsOnWithoutNotify(toRevert.toggleAds);
        autoAimSetting.SetIsOnWithoutNotify(toRevert.autoAim);
        languageSetting.SetIsOnWithoutNotify(toRevert.language == Language.Turkish);
        fullscreenSetting.SetIsOnWithoutNotify(toRevert.fullscreen);

        resolutionSetting.SetValueWithoutNotify(toRevert.resIndex);

        UpdateSettings(toRevert);

        LanguageHandler.VisualizeLanguageChange(toRevert.language);
    }

    private void UpdateSettings(SettingsSave toRevert)
    {
        SetResolution(resolutionsLoaded[toRevert.resIndex]);
    }


    public void OnChanged_Slider(float newSetting, SliderSettings set)
    {
        switch (set)
        {
            case SliderSettings.Sensitivity:
                settingsOnMemory.sensitivity = newSetting;
                break;
            case SliderSettings.Volume:
                settingsOnMemory.sfx = newSetting;
                break;
        }
        changesMade = true;
    }
    public void OnChanged_Toggle(bool newSetting, ToggleSettings set)
    {
        switch (set)
        {
            case ToggleSettings.ADS:
                settingsOnMemory.toggleAds = newSetting;
                break;
            case ToggleSettings.AutoAim:
                settingsOnMemory.autoAim = newSetting;
                break;
            case ToggleSettings.Language:
                settingsOnMemory.language = newSetting ? Language.Turkish : Language.English;

                LanguageHandler.VisualizeLanguageChange(settingsOnMemory.language);

                break;
            case ToggleSettings.Fullscreen:
                settingsOnMemory.fullscreen = newSetting;
                break;
        }
        changesMade = true;
    }
    public void OnChanged_Dropdown(int newSetting, DropdownSettings set)
    {
        switch (set)
        {
            case DropdownSettings.Resolution:

                int oldResIndex = SaveSocket.CurrentSave.settings.resIndex;

                var oldRes = resolutionsLoaded[oldResIndex];
                var res = resolutionsLoaded[newSetting];
                SetResolution(res);

                ClientUI.PopupInstance.ShowPopup(
                    PopupManager.T_SURE_TEXT,
                    "Emin Misin?",
                    "Uygula",
                    "Geri al",
                    () =>
                    {
                        //yes

                        settingsOnMemory.resIndex = newSetting;

                    }, () =>
                    {
                        //no
                        SetResolution(oldRes);
                        resolutionSetting.SetValueWithoutNotify(oldResIndex);

                    },
                    true,
                    true,
                    true,
                    false,
                    10
                    );


                break;
        }

        changesMade = true;
    }

    public void ResetSettings()
    {
        settingsOnMemory = SaveData.GetDefaultSave().settings;
        RevertSettings(settingsOnMemory);
        changesMade = true;
    }

    private void SetResolution(Resolution res)
    {
        Screen.SetResolution(res.width, res.height, GetFullscreenStatus(), res.refreshRate);

        Application.targetFrameRate = res.refreshRate;
    }

    private FullScreenMode GetFullscreenStatus()
    {
        return settingsOnMemory.fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    }

    public static List<Resolution> GetResolutions()
    {
        //Filters out all resolutions with low refresh rate:
        Resolution[] resolutions = Screen.resolutions;
        HashSet<Tuple<int, int>> uniqResolutions = new HashSet<Tuple<int, int>>();
        Dictionary<Tuple<int, int>, int> maxRefreshRates = new Dictionary<Tuple<int, int>, int>();
        for (int i = 0; i < resolutions.GetLength(0); i++)
        {
            //Add resolutions (if they are not already contained)
            Tuple<int, int> resolution = new Tuple<int, int>(resolutions[i].width, resolutions[i].height);
            uniqResolutions.Add(resolution);
            //Get highest framerate:
            if (!maxRefreshRates.ContainsKey(resolution))
            {
                maxRefreshRates.Add(resolution, resolutions[i].refreshRate);
            }
            else
            {
                maxRefreshRates[resolution] = resolutions[i].refreshRate;
            }
        }
        //Build resolution list:
        List<Resolution> uniqResolutionsList = new List<Resolution>(uniqResolutions.Count);
        foreach (Tuple<int, int> resolution in uniqResolutions)
        {
            Resolution newResolution = new Resolution();
            newResolution.width = resolution.Item1;
            newResolution.height = resolution.Item2;
            if (maxRefreshRates.TryGetValue(resolution, out int refreshRate))
            {
                newResolution.refreshRate = refreshRate;
            }
            uniqResolutionsList.Add(newResolution);
        }
        uniqResolutionsList.Reverse();
        return uniqResolutionsList;
    }
}

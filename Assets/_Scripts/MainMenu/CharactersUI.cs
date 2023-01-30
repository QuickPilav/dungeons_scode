using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharactersUI
{
    [Flags]
    public enum UnlockablePlayerClasses
    {
        None = 0x00,              //0
        classMami = 0x01,         //1
        classSissy = 1 << 1,      //2
        classIbo = 1 << 2,        //4
        classVlonderz = 1 << 3,   //8
        classDibaba = 1 << 4,     //16
        classAmy = 1 << 5         //32
    }

    [SerializeField] private PlayerController ply;

    [Space]

    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private TextMeshProUGUI classDescriptionText;
    [SerializeField] private GameObject notEnoughPointsText;
    [SerializeField] private Button buyCharacterButton;

    private LanguageTextExtra buyButtonText;

    private PlayerClassScriptable selectedClass;

    public void Initialize()
    {
        buyButtonText = buyCharacterButton.GetComponentInChildren<LanguageTextExtra>();
        SaveSocket.OnSettingsChanged.SubscribeToEvent(OnSettingsChanged);
    }
    public void Dispose()
    {
        SaveSocket.OnSettingsChanged.UnsubscribeToEvent(OnSettingsChanged);
    }


    private void OnSettingsChanged(SettingsSave settings)
    {
        if (selectedClass != null)
        {
            SelectCharacter(selectedClass.playerClass);
        }
        else
        {
            SelectCharacter(PlayerClass.classMami);
        }
    }


    public void SelectCharacter(PlayerClass plyClass)
    {
        foreach (var item in ply.ClassHandler.AllMeshRenderers)
        {
            item.enabled = false;
        }

        var plyStats = ResourceManager.GetPlayerClassScriptable(plyClass);
        selectedClass = plyStats;
        var cls = ply.ClassHandler.GetClass(plyClass);


        classNameText.text = plyStats.visualName;
        classDescriptionText.text = plyStats.descriptionScriptable.GetTranslationOf(SaveSocket.CurrentSave.settings.language);

        bool isCharacterUnlocked = SaveSocket.CurrentSave.unlockedCharacters.CustomHasFlag(plyClass.ConvertToUnlockable());

        bool haveEnoughPoints = SaveSocket.CurrentSave.points >= plyStats.pointsCost;

        bool buttonInteractable = !isCharacterUnlocked && haveEnoughPoints;

        bool isButtonVisible = plyStats.pointsCost != -1 && !isCharacterUnlocked;
        buyCharacterButton.gameObject.SetActive(isButtonVisible);
        if (isButtonVisible)
        {
            //karakter açýk deðil ise alma tuþunu aktif et...
            buyCharacterButton.interactable = buttonInteractable;

            notEnoughPointsText.SetActive(!isCharacterUnlocked && !haveEnoughPoints);
        }

        buyButtonText.SetExtraText($"{plyStats.pointsCost}<sprite=0>");

        foreach (var item in cls.CharacterRenderers)
        {
            item.enabled = true;
        }
    }

    public void BuyCurrentCharacter()
    {
        BuyCharacter(selectedClass);
    }

    public void BuyCharacter(PlayerClassScriptable plyClass)
    {
        SaveSocket.CurrentSave.points -= plyClass.pointsCost;

        UnlockCharacter(plyClass, true);

        MainMenuUI.Instance.DeductPointsSlowly(plyClass.pointsCost);

        SelectCharacter(plyClass.playerClass);
    }

    public static void UnlockCharacter(PlayerClassScriptable plyClass, bool save)
    {
        SaveSocket.CurrentSave.unlockedCharacters = SaveSocket.CurrentSave.unlockedCharacters.Add(plyClass.playerClass.ConvertToUnlockable());
        if (save)
        {
            SaveSocket.Save();
        }

        //ClientUI.PopupInstance.ShowPopup(Popups.NewCharacterUnlocked, $"{plyClass.visualName} karakteri baþarýyla eklendi!", true, 5f);
        ClientUI.PopupInstance.ShowPopup(Popups.NewCharacterUnlocked, null, null, 5f, plyClass.visualName);

    }

}

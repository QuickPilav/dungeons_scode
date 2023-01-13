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
        SaveSocket.OnSaveDataLoadedForTheFirstTime.SubscribeToEvent(OnSaveLoaded);
    }

    public void Dispose()
    {
        SaveSocket.OnSaveDataLoadedForTheFirstTime.UnsubscribeToEvent(OnSaveLoaded);
    }

    private void OnSaveLoaded(SaveData saveData)
    {
        SelectCharacter(PlayerClass.classMami);


        foreach (var item in saveData.unlockedCharacters.GetFlags())
        {
            if (item is UnlockablePlayerClasses.None)
                continue;


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
        classDescriptionText.text = plyStats.description;

        bool isCharacterUnlocked = SaveSocket.CurrentSave.unlockedCharacters.CustomHasFlag(plyClass.ConvertToUnlockable());

        bool haveEnoughPoints = SaveSocket.CurrentSave.points >= plyStats.pointsCost;

        bool buttonInteractable = !isCharacterUnlocked && haveEnoughPoints;

        bool isButtonVisible = plyStats.pointsCost != -1 && !isCharacterUnlocked;
        buyCharacterButton.gameObject.SetActive(isButtonVisible);
        if(isButtonVisible)
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

        UnlockCharacter(plyClass,true);

        MainMenuUI.Instance.DeductPointsSlowly(plyClass.pointsCost);

        SelectCharacter(plyClass.playerClass);
    }

    public static void UnlockCharacter(PlayerClassScriptable plyClass, bool save)
    {
        SaveSocket.CurrentSave.unlockedCharacters = SaveSocket.CurrentSave.unlockedCharacters.Add(plyClass.playerClass.ConvertToUnlockable());
        if(save)
        {
            SaveSocket.Save();
        }

        ClientUI.PopupInstance.ShowOnlyTextPopup("Yeni Karakter eklendi.", $"{plyClass.visualName} karakteri baþarýyla eklendi!", true, 5f);

    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

public enum Popups
{
    ChangesPrompt,
    QuitPrompt,
    BuyPrompt,
    ResolutionPrompt,
    LeavePrompt,
    SteamNotInitialized,
    NewCharacterUnlocked,
    LevelUp
}


[System.Serializable]
public class PopupManager
{
    [SerializeField, LabeledArray(typeof(Popups))] private PopupTranslationScriptable[] popupTranslations;

    [SerializeField] private Popup popupPrefab;
    [SerializeField] private Popup popupNotificationPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private Transform popupNotificationParent;
    [SerializeField] private GameObject popupBackground;

    private List<Popup> activePopups;
    private List<Popup> activeNotifications;

    public void Initialize()
    {
        activePopups = new List<Popup>();
        activeNotifications = new List<Popup>();
    }

    /// <param name="timer">timer bitince No çaðýrýlýr, -1 timerý kaldýrýr</param>
    public Popup ShowPopup(Popups popupTranslation, Action OnClickedYes, Action OnClickedNo, float timer = -1, params string[] args)
    {
        var trns = popupTranslations[(int)popupTranslation];

        var spawnedPopup = UnityEngine.Object.Instantiate(trns.isNotification ? popupNotificationPrefab : popupPrefab, trns.isNotification ? popupNotificationParent : popupParent);
        spawnedPopup.Initialize(trns, OnClickedYes, OnClickedNo, timer, args);

        AddPopup(spawnedPopup);

        return spawnedPopup;
    }

    public void AddPopup(Popup popup)
    {
        if (popup.IsNotification)
        {
            activeNotifications.Add(popup);
        }
        else
        {
            activePopups.Add(popup);
        }

        popupBackground.SetActive(activePopups.Count > 0);
    }

    public void RemovePopup(Popup popup)
    {
        if (popup.IsNotification)
        {
            activeNotifications.Remove(popup);
        }
        else
        {
            activePopups.Remove(popup);
        }

        popupBackground.SetActive(activePopups.Count > 0);
    }

    public int GetPopupCount { get { return activePopups.Count; } }

    public Popup GetPopup(int index)
    {
        return activePopups[index];
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PopupManager
{
    public const string T_SURE_TEXT = "Emin misin?";

    public const string M_SURE_WANT_TO_LEAVE = "Lobiye dönmek istediðinden emin misin?";
    public const string M_SURE_WANT_TO_QUIT = "Çýkmak istediðinden emin misin?";
    public const string M_SURE_WANT_TO_BUY = "Almak istediðinden emin misin?";

    public const string B_YES_TEXT = "Evet";
    public const string B_NO_TEXT = "Hayýr";
    public const string B_OK_TEXT = "Tamam";

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

    public Popup ShowAreYouSurePopup(string middleText, Action OnClickedYes, Action OnClickedNo, bool isNotification)
    {
        return ShowPopup(T_SURE_TEXT, middleText, B_YES_TEXT, B_NO_TEXT, OnClickedYes, OnClickedNo,true, true, true, isNotification);
    }

    public Popup ShowOKPopup(string topText, string middleText, Action OnClickedYes, bool isNotification, float timer = -1)
    {
        return ShowPopup(topText, middleText, B_OK_TEXT, string.Empty, OnClickedYes, null, true,false, false, isNotification, timer);
    }

    public Popup ShowOnlyTextPopup(string topText, string middleText, bool isNotification, float timer = -1)
    {
        return ShowPopup(topText, middleText, string.Empty, string.Empty, null, null, false, false, false, isNotification, timer);
    }

    /// <param name="timer">timer bitince No çaðýrýlýr, -1 timerý kaldýrýr</param>
    public Popup ShowPopup(string topText, string middleText, string yesText, string noText, Action OnClickedYes, Action OnClickedNo, bool haveYesButton, bool haveNoButton, bool haveCloseButton, bool isNotification, float timer = -1)
    {
        var spawnedPopup = UnityEngine.Object.Instantiate(isNotification ? popupNotificationPrefab : popupPrefab, isNotification ? popupNotificationParent : popupParent);
        spawnedPopup.Initialize(topText, middleText, yesText, noText, OnClickedYes, OnClickedNo, haveYesButton, haveNoButton, haveCloseButton, timer, isNotification);

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
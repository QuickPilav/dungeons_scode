using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectorUI : StaticInstance<HeroSelectorUI>
{
#if UNITY_EDITOR
    private const bool ALL_HEROS_UNLOCKED = false;
#else
    private const bool ALL_HEROS_UNLOCKED = false;
#endif

    public event Action OnHeroSelected;
    public bool IsSelectingHero
    {
        get => isSelectingHero;
        private set
        {
            isSelectingHero = value;
            heroSelectionObj.SetActive(value);
            ClientUI.SetCursor(value);
        }
    }

    private bool isSelectingHero;

    [SerializeField] private GameObject heroSelectionObj;
    [SerializeField] private GameObject heroHolder;

    public void Initialize ()
    {
        IsSelectingHero = true;
        heroHolder.SetActive(false);

        PhotonManager.OnJoinedToRoom.SubscribeToEvent(OnJoinedToRoom);
    }

    private void OnDestroy ()
    {
        try
        {
            PhotonManager.OnJoinedToRoom.UnsubscribeToEvent(OnJoinedToRoom);
        }
        catch (Exception)
        {

        }
    }

    public void SelectCharacter_Btn (int plyClass)
    {
        GameEvents.SelectedClass = (PlayerClass)plyClass;
        OnHeroSelected?.Invoke();

        IsSelectingHero = false;
    }

    public void OnJoinedToRoom (string roomName)
    {
        heroHolder.SetActive(true);
        ClientUI.SetCursor(true);


        HandleSelection();


        //OTO KARAKTER SEÇME DEBUG

#if UNITY_EDITOR
        //SelectCharacter_Btn((int)PlayerClass.Ibo);
#endif
    }

    private void HandleSelection ()
    {
        int siblingIndex = 0;
        foreach (Transform child in heroHolder.transform)
        {
            PlayerClass plyClass = (PlayerClass)siblingIndex;

            bool shouldBeInteractable = ALL_HEROS_UNLOCKED || SaveSocket.CurrentSave.unlockedCharacters.CustomHasFlag(plyClass.ConvertToUnlockable());
            
            child.GetComponent<Button>().interactable = shouldBeInteractable;   

            siblingIndex++;
        }
    }

}

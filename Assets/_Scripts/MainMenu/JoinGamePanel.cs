using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class JoinGamePanel
{
    public JoinPanelItemUI SelectedJoinItem
    {
        get
        {
            if (selectedJoinIndex < 0)
                return null;

            return joinPanelItems[selectedJoinIndex];
        }
        set
        {
            if (value != null && value.PanelItemIndex == selectedJoinIndex)
            {
                value = null;
            }

            joinGameButton.interactable = value != null &&
                value.RoomInfo.PlayerCount < value.RoomInfo.MaxPlayers;

            if (value == null)
            {
                selectedJoinIndex = -1;
                try
                {
                    selectedItemShower.position = Vector3.zero;
                }
                catch (Exception)
                {
                }
                return;
            }
            selectedJoinIndex = value.PanelItemIndex;

            //StartCoroutine(itemsScroll.FocusOnItemCoroutine(value.Rect, 5f));
            selectedItemShower.position = value.Rect.position;
        }
    }

    [SerializeField] private RectTransform selectedItemShower;
    [SerializeField] private JoinPanelItemUI joinItemUIPrefab;
    [SerializeField] private Transform joinItemParent;

    [SerializeField] private Button joinGameButton;

    private List<JoinPanelItemUI> joinPanelItems;
    private int selectedJoinIndex = -1;

    public void JoinCurrentRoom()
    {
        PhotonManager.Instance.JoinRoom(SelectedJoinItem.RoomInfo.Name);
    }

    public void OnStartedJoiningRoom()
    {
        SelectedJoinItem = null;
    }

    public void OnPanelActivated()
    {
        OnRoomListUpdated(PhotonManager.CurrentRoomsInfo);
        SelectedJoinItem = null;
    }

    public void Initialize()
    {
        PhotonManager.OnCurrentRoomsChanged += OnRoomListUpdated;
        joinPanelItems = new List<JoinPanelItemUI>();
    }

    public void OnRoomListUpdated(List<RoomInfo> roomList)
    {
        foreach (var item in joinPanelItems)
        {
            UnityEngine.Object.Destroy(item.gameObject);
        }

        joinPanelItems.Clear();

        if (roomList == null)
        {
            Debug.Log("Daha hiç oda updatei yememiþiz!!!");
            return;
        }

        int panelIndex = 0;
        foreach (var item in roomList)
        {
            if (item.RemovedFromList)
                continue;

            var spawned = UnityEngine.Object.Instantiate(joinItemUIPrefab, joinItemParent);
            spawned.SetRoomInfo(panelIndex, item);
            joinPanelItems.Add(spawned);

            panelIndex++;
        }
        selectedItemShower.SetAsLastSibling();
        SelectedJoinItem = null;
    }
}
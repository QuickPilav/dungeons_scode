using Photon.Realtime;
using TMPro;
using UnityEngine;

public class JoinPanelItemUI : MonoBehaviour
{
    public int PanelItemIndex { get; private set; }
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI roomSizeText;

    public RectTransform Rect { get => rect; }
    public RoomInfo RoomInfo { get; private set; }

    private RectTransform rect;

    public void SetRoomInfo (int panelIndex, RoomInfo rInfo)
    {
        this.RoomInfo = rInfo;
        PanelItemIndex = panelIndex;
        rect = GetComponent<RectTransform>();

        string roomOwner = (string)rInfo.CustomProperties[PhotonManager.ROOM_OWNER_NAME_KEY];

        string additive = string.Empty;

#if UNITY_EDITOR
        additive = $"{ rInfo.Name[..4]}|";
#endif

        roomNameText.text = $"{additive}{roomOwner}'s Room";
        roomSizeText.text = $"{rInfo.PlayerCount} / {rInfo.MaxPlayers}";
    }

    public void OnSelected_Btn ()
    {
        MainMenuUI.Instance.JoinPanel.SelectedJoinItem = this;
    }
}

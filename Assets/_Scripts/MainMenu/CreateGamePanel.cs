using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CreateGamePanel
{
    public bool IsVisibleToOthers { get; set; }
    [SerializeField] private Button createGameButton;


    public void CreateRoom()
    {
        PhotonManager.Instance.CreateRoom(IsVisibleToOthers);
    }

    public void OnStartedJoiningRoom()
    {
        createGameButton.interactable = false;
    }
}

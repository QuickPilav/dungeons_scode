using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ClientUI;

public class ButtonFunctions : MonoBehaviour
{
    [SerializeField] private ClientUI.ButtonFunctionsEnum buttonSetting;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }
    public void OnClicked()
    {
        switch (buttonSetting)
        {
            default:
                ExecuteButtonPress(buttonSetting);
                break;
            case ClientUI.ButtonFunctionsEnum.ReturnToMainMenuPrompt:

                PopupInstance.ShowAreYouSurePopup(
                    PopupManager.M_SURE_WANT_TO_LEAVE, () =>
                    {
                        ReturnToMainMenu();
                    }, null, false);

                break;
            case ClientUI.ButtonFunctionsEnum.ReturnToMainMenu:

                ReturnToMainMenu();

                break;
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonManager.Instance.LeaveRoom();
    }

    public static void ExecuteButtonPress(ClientUI.ButtonFunctionsEnum buttonSetting)
    {
        switch (buttonSetting)
        {
            case ClientUI.ButtonFunctionsEnum.JoinRoom:
                MainMenuUI.Instance.JoinPanel.JoinCurrentRoom();
                break;
            case ClientUI.ButtonFunctionsEnum.CreateRoom:
                MainMenuUI.Instance.CreatePanel.CreateRoom();
                break;
            case ClientUI.ButtonFunctionsEnum.ReactivateMainMenu:
                MainMenuUI.Instance.ResetToMainMenu_Btn();
                break;
            default:
                ClientUI.Instance.OnPressed_Button(buttonSetting);
                break;
        }
    }
}

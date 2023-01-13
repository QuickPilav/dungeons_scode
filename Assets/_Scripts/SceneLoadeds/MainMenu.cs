using Photon.Pun;
using Steamworks;
using UnityEngine;

public class MainMenu : SceneLoadedHandler
{
    protected override bool subscribeToMainMenuLoaded => true;
    protected override void OnMainMenuLoaded ()
    {
        base.OnMainMenuLoaded();

        PlayerController.DeletedFakeInstance = false;

        SteamManager.OnSteamInitialized.SubscribeToEvent(OnSteamInitialized);

        MainMenuUI.Instance.Initialize();

        //TEST
        //GameManager.LoadSceneAndSetActive((int)Game_State.GameScene);
    }

    private void OnDestroy()
    {
        SteamManager.OnSteamInitialized.UnsubscribeToEvent(OnSteamInitialized);
    }


    private void OnSteamInitialized ()
    {
        Debug.Log($"name set to {SteamManager.LocalSteamNickName}");

        SteamManager.GetMediumSteamAvatar(SteamUser.GetSteamID(), out Texture2D tex);
        MainMenuUI.Instance.SetClient(tex, SteamManager.LocalSteamNickName);
    }
}


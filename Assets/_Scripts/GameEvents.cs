using Photon.Pun;
using System;
using UnityEngine;

public enum Game_State
{
    Init,
    MainMenu,
    GameScene
}

public static class GameEvents
{
    private static Game_State gameState;
    public static Game_State GameState
    {
        get => gameState;
        set
        {
            gameState = value;

            switch (gameState)
            {
                case Game_State.Init:
                    break;
                case Game_State.MainMenu:
                    OnMainMenuLoaded?.Invoke();
                    break;
                case Game_State.GameScene:
                    OnGameSceneLoaded?.Invoke();
                    break;
            }
        }
    }

    public static PlayerClass SelectedClass { get; set; }

    public static bool IsInitSceneLoaded { get; set; }

    public static InstantEvent OnGameSceneLoaded = new InstantEvent(() => GameState == Game_State.GameScene, false);
    public static InstantEvent OnMainMenuLoaded = new InstantEvent(() => GameState == Game_State.MainMenu, false);
    public static Action OnQuitGame { get; private set; }

    public static void Quit()
    {
        Debug.Log("Oyun kapandý!");
        OnQuitGame?.Invoke();
        Application.Quit();
    }
}

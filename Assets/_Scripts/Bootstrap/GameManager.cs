using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private bool forceFromStart;

    public void Initialize ()
    {
        PhotonManager.OnRoomLeft += PhotonManager_OnRoomLeft;

#if UNITY_EDITOR

        if(forceFromStart)
        {
            HandleFromStart();
            return;
        }

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0: //init
                HandleFromStart();
                break;
            case 1: //main menu
                SceneManager.LoadScene((int)Game_State.Init, LoadSceneMode.Additive);
                break;
            default: //any scene
                SceneManager.LoadScene((int)Game_State.Init, LoadSceneMode.Additive);
                break;
        }

#else
        HandleFromStart();
#endif
    }

    private void OnDestroy()
    {
        PhotonManager.OnRoomLeft -= PhotonManager_OnRoomLeft;
    }

    private void PhotonManager_OnRoomLeft()
    {
        LoadSceneAndSetActive((int)Game_State.MainMenu);
    }

    private void HandleFromStart ()
    {
        LoadSceneAndSetActive((int)Game_State.MainMenu);
    }

    public static void LoadSceneAndSetActive (int buildIndex)
    {
        if (buildIndex == (int)GameEvents.GameState)
            return;

        Instance.StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            bool initSceneExists = false;
            List<Scene> scenesToUnload = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.buildIndex == (int)Game_State.Init)
                {
                    initSceneExists = true;
                    continue;
                }

                scenesToUnload.Add(scene);
            }

            if (!initSceneExists)
            {
                SceneManager.LoadScene((int)Game_State.Init, LoadSceneMode.Additive);
            }

            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                Debug.Log($"Unloading {scenesToUnload[i].name}");
                yield return SceneManager.UnloadSceneAsync(scenesToUnload[i]);
            }

            yield return SceneManager.LoadSceneAsync(buildIndex, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });


            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));

            GameEvents.GameState = (Game_State)buildIndex;
        }
    }
}

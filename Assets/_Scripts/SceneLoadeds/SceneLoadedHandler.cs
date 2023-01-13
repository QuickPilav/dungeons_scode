using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadedHandler : MonoBehaviour
{
    protected static SceneLoadedHandler currentScene;
    public static SceneLoadedHandler CurrentScene
    {
        get => currentScene;
        protected set => currentScene = value;
    }

    public static T GetSceneAs<T> () where T : SceneLoadedHandler
    {
        return (T)currentScene;
    }

    protected virtual bool subscribeToMainMenuLoaded { get; }
    protected virtual bool subscribeToGameSceneLoaded { get; }

    protected virtual void Awake ()
    {
        if (!subscribeToMainMenuLoaded && !subscribeToGameSceneLoaded)
        {
            Debug.Log("hiç yok, dönülüyor!");
            return;
        }

        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return new WaitUntil(() => GameEvents.IsInitSceneLoaded);

            if(subscribeToGameSceneLoaded)
            {
                GameEvents.OnGameSceneLoaded.SubscribeToEvent(OnGameSceneLoaded);
            }
            if(subscribeToMainMenuLoaded)
            {
                GameEvents.OnMainMenuLoaded.SubscribeToEvent(OnMainMenuLoaded);
            }
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnGameSceneLoaded.UnsubscribeToEvent(OnGameSceneLoaded);

        GameEvents.OnMainMenuLoaded.UnsubscribeToEvent(OnMainMenuLoaded);
    }

    protected virtual void OnMainMenuLoaded ()
    {
        CurrentScene = this;
    }

    protected virtual void OnGameSceneLoaded ()
    {
        CurrentScene = this;
    }
}
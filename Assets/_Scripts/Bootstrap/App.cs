using UnityEngine;
using UnityEngine.SceneManagement;

public static class App
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

        var app = Object.Instantiate(Resources.Load("App")) as GameObject;

        if (app == null)
        {
            Debug.LogError($"No \"App\" was found!");
            return;
        }

        Object.DontDestroyOnLoad(app);

        GameEvents.GameState = (Game_State)SceneManager.GetActiveScene().buildIndex;

        InitializeOtherSystems(app);
    }

    public static void InitializeOtherSystems (GameObject app)
    {
        var gm = app.GetComponent<GameManager>();
        gm.Initialize();
        app.GetComponent<MissionHandler>().Initialize();
        app.GetComponent<ProgressionSystem>().Initialize();
        ConsoleHandler.Initialize(gm);
    }
}

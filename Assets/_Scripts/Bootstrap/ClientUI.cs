using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientUI : Singleton<ClientUI>
{
    public enum ButtonFunctionsEnum
    {
        //general
        JoinRoom,
        CreateRoom,
        ReturnToMainMenu,

        //client ui thingies
        Continue,
        OpenSettings,
        QuitPrompt,
        ReturnToMainMenuPrompt,
        CloseSettings,
        ResetSettings,

        //others
        ReactivateMainMenu,
        BuyCharacterPrompt
    }

    public static PopupManager PopupInstance { get => Instance.popupInstance; }
    public static SettingsUI SettingsInstance { get => Instance.settingsInstance; }

    public static event Action OnGamePaused;
    public static event Action OnGameContinued;
    public static bool IsGamePaused
    {
        get => lastWasPaused;
    }

    private static bool lastWasPaused;

    public static bool IsMousePaused
    {
        get => lastMouseWasPaused;
    }

    private static bool lastMouseWasPaused;

    public bool IsPauseMenuOpen
    {
        get => isPauseMenuOpen;
        set
        {
            if (value && inMainMenu)
                return;

            isPauseMenuOpen = value;

            pauseMenu.SetActive(value);

            RecalculateIsPaused();
            SetCursor(value);
        }
    }

    private bool isPauseMenuOpen;
    private bool inMainMenu;

    [SerializeField] private PopupManager popupInstance;
    [SerializeField] private SettingsUI settingsInstance;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject returnToMainMenuButton;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private GameObject loadingMenu;
    public GameObject normalPauseMenu;

    protected override void Awake()
    {
        base.Awake();
        Initialize();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        SceneManager_activeSceneChanged(new Scene(), SceneManager.GetActiveScene());

        settingsInstance.IsSettingsMenuOpen = false;
        IsPauseMenuOpen = false;

        GameEvents.IsInitSceneLoaded = true;
    }
    private void SceneManager_activeSceneChanged(Scene current, Scene next)
    {
        inMainMenu = next.buildIndex == (int)Game_State.MainMenu;

        returnToMainMenuButton.SetActive(next.buildIndex == (int)Game_State.GameScene);

        SetCursor(inMainMenu);
        if (inMainMenu)
        {
            settingsInstance.IsSettingsMenuOpen = false;
            IsPauseMenuOpen = false;
        }


        loadingMenu.SetActive(next.buildIndex == (int)Game_State.Init);
    }
    public void Initialize()
    {
        popupInstance.Initialize();
        settingsInstance.Initialize(this);

        SaveSocket.Load();

        SaveSocket.OnSettingsChanged.SubscribeToEvent((newSettings) =>
        {
            AudioListener.volume = newSettings.sfx;
        });


        //FadeIn(1f,0f,1f);
    }
    public void RecalculateIsPaused()
    {
        bool isPausedNow = IsPauseMenuOpen;
        bool isMousePausedNow = false;

        if (ShopUI.Instance != null && ShopUI.Instance.IsShopOpen)
        {
            isMousePausedNow = true;
        }

        bool lastLastWasPaused = lastWasPaused;

        lastWasPaused = isPausedNow;
        lastMouseWasPaused = isMousePausedNow;

        if (lastLastWasPaused != lastWasPaused)
        {
            if (lastWasPaused)
            {
                OnGamePaused?.Invoke();
            }
            else
            {
                OnGameContinued?.Invoke();
            }
        }
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            CalculateEscape();
        }

        if (Input.GetButtonDown("Submit"))
        {
            CalculateEnter();
        }
    }
    private void CalculateEscape()
    {
        if (popupInstance.GetPopupCount > 0)
        {
            popupInstance.GetPopup(0).ClickNo_Btn();
            return;
        }

        if (settingsInstance.IsSettingsMenuOpen)
        {
            settingsInstance.IsSettingsMenuOpen = false;
            return;
        }

        if (ShopUI.Instance != null && ShopUI.Instance.IsShopOpen)
        {
            ShopUI.Instance.IsShopOpen = false;
            return;
        }


        IsPauseMenuOpen = !IsPauseMenuOpen;
    }
    private void CalculateEnter()
    {
        if (popupInstance.GetPopupCount > 0)
        {
            popupInstance.GetPopup(0).ClickYes_Btn();
            return;
        }
    }
    public static void SetCursor(bool visible)
    {
        bool isShopOpen = false;
        if (ShopUI.Instance != null && ShopUI.Instance.IsShopOpen)
        {
            isShopOpen = true;
        }

        bool isSelectingHero = false;

        if (HeroSelectorUI.Instance != null && HeroSelectorUI.Instance.IsSelectingHero)
        {
            isSelectingHero = true;
        }

        bool gameEndedUIActive = false;

        if (InGameUI.Instance != null && InGameUI.Instance.GameEndedUIActive)
        {
            gameEndedUIActive = true;
        }

        if (!visible && (Instance.inMainMenu || IsGamePaused || isShopOpen || isSelectingHero || gameEndedUIActive))
            return;

        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    public void FadeIn(float from, float target, float inSeconds, Action OnComplete = null)
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            fade.GetComponent<Graphic>().raycastTarget = true;
            float timePassed = 0;
            while (timePassed < 1f)
            {
                timePassed += Time.deltaTime / inSeconds;

                fade.alpha = Mathf.Lerp(from, target, timePassed);

                yield return null;
            }

            if (target == 0)
            {
                fade.GetComponent<Graphic>().raycastTarget = false;
            }

            OnComplete?.Invoke();
        }
    }
    //buttons

    public void OnPressed_Button(ButtonFunctionsEnum set)
    {
        switch (set)
        {
            case ButtonFunctionsEnum.Continue:
                IsPauseMenuOpen = false;
                break;
            case ButtonFunctionsEnum.OpenSettings:
                settingsInstance.IsSettingsMenuOpen = true;
                break;
            case ButtonFunctionsEnum.QuitPrompt:
                PopupInstance.ShowPopup(
                    Popups.QuitPrompt,
                    () =>
                    {

                        GameEvents.Quit();
                        
                    }, null);
                break;
            case ButtonFunctionsEnum.CloseSettings:
                settingsInstance.IsSettingsMenuOpen = false;
                break;
            case ButtonFunctionsEnum.ResetSettings:
                settingsInstance.ResetSettings();
                break;

            case ButtonFunctionsEnum.BuyCharacterPrompt:
                PopupInstance.ShowPopup(
                    Popups.BuyPrompt,
                    () =>
                    {
                        MainMenuUI.Instance.BuyCharacter_Btn();
                    }, null);
                break;
        }
    }
}

using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class MainMenuUI : StaticInstance<MainMenuUI>
{
    private readonly WaitForSeconds pointsWaiter = new WaitForSeconds(.05f);
    public JoinGamePanel JoinPanel => joinPanel;
    public CreateGamePanel CreatePanel => createPanel;
    public CharactersUI CharactersPanel => charactersPanel;
    public ProgressionUIPanel ProgressionPanel => progressionPanel;

    [SerializeField] private GameObject[] menus;
    [SerializeField] private Button[] allButtons;
    [SerializeField] private Button joinPanelButton;
    [SerializeField] private Button createPanelButton;
    [SerializeField] private GameObject buttonBlocker;

    [SerializeField] private RawImage clientImage;
    [SerializeField] private TextMeshProUGUI clientNameText;
    [Space]
    [SerializeField] private JoinGamePanel joinPanel;
    [SerializeField] private CreateGamePanel createPanel;
    [SerializeField] private CharactersUI charactersPanel;
    [Space]
    [SerializeField] private ProgressionUIPanel progressionPanel;

    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private AudioClip pointsClip;

    private AudioSource aSource;

    private bool isFirstTime = true;
    private int lastPoints;
    public void Initialize()
    {
        aSource = GetComponent<AudioSource>();

        JoinPanel.Initialize();

        charactersPanel.Initialize();

        progressionPanel.Initialize();

        ResetToMainMenu_Btn();

        buttonBlocker.SetActive(false);
        createPanelButton.interactable = false;
        joinPanelButton.interactable = false;

        PhotonManager.OnStartedJoiningRoom += OnStartedJoiningTheRoom;
        PhotonManager.OnConnectedToPhoton.SubscribeToEvent(OnConnectedtoPhoton);
        SaveSocket.OnSaveDataLoadedForTheFirstTime.SubscribeToEvent(OnSaveLoaded);

        if(!SteamManager.Initialized)
        {
            ClientUI.PopupInstance.ShowOKPopup("Steam is not initialized", "Press OK to quit", () => Application.Quit(), false);
        }
    }

    private void OnSaveLoaded(SaveData save)
    {
        SetPointsSlowly(save.points);
    }

    private void OnStartedJoiningTheRoom()
    {
        GameManager.LoadSceneAndSetActive((int)Game_State.GameScene);

        joinPanel.OnStartedJoiningRoom();
        CreatePanel.OnStartedJoiningRoom();
        buttonBlocker.SetActive(true);
    }
    public void OnConnectedtoPhoton()
    {
        createPanelButton.interactable = true;
        joinPanelButton.interactable = true;
    }
    public void SetClient(Texture2D tex, string clientName)
    {
        clientImage.texture = tex;
        clientNameText.text = clientName;
    }
    public void MainMenu_Btn(int clickIndex)
    {
        switch (clickIndex)
        {
            case 0:
                StartSingleplayer();
                break;
            case 1:
                menus[1].SetActive(true);
                JoinPanel.OnPanelActivated();
                break;
            case 2:
                menus[2].SetActive(true);

                break;
            case 3:
                menus[0].SetActive(false);
                menus[3].SetActive(true);

                break;
            case 4:
                ButtonFunctions.ExecuteButtonPress(ClientUI.ButtonFunctionsEnum.OpenSettings);
                break;
            case 5:
                ButtonFunctions.ExecuteButtonPress(ClientUI.ButtonFunctionsEnum.QuitPrompt);
                break;

        }
    }
    public void ChangeIsOpenLobby_Toggle(bool isOn)
    {
        Debug.Log($"is room gonna be public? {isOn}");
        createPanel.IsVisibleToOthers = isOn;
    }
    public void StartSingleplayer()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return PhotonManager.NotConnecting;
            
            /*
            if (PhotonManager.IsConnected)
            {
                Debug.Log("we need to be offline so, being offline...");
                yield return StartCoroutine(PhotonManager.Instance.Disconnect());
            }
            PhotonNetwork.OfflineMode = true;
            yield return PhotonManager.NotConnecting;
            */
            PhotonManager.Instance.CreateRoom(false);
        }
    }
    private void OnDestroy()
    {
        PhotonManager.OnCurrentRoomsChanged -= joinPanel.OnRoomListUpdated;

        PhotonManager.OnStartedJoiningRoom -= OnStartedJoiningTheRoom;
        PhotonManager.OnConnectedToPhoton.UnsubscribeToEvent(OnConnectedtoPhoton);

        SaveSocket.OnSaveDataLoadedForTheFirstTime.UnsubscribeToEvent(OnSaveLoaded);

        charactersPanel.Dispose();
    }
    public void SelectCharacter_Btn(int classToSelect)
    {
        charactersPanel.SelectCharacter((PlayerClass)classToSelect);
    }
    public void BuyCharacter_Btn()
    {
        charactersPanel.BuyCurrentCharacter();
    }
    public void ResetToMainMenu_Btn()
    {
        foreach (var item in menus)
        {
            item.SetActive(false);
        }
        menus[0].SetActive(true);
    }
    private void Update()
    {
        //DEBUG
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetPointsSlowly(UnityEngine.Random.Range(1, 1000));
        }
    }
    public void DeductPointsSlowly(int pointsToDeduct)
    {
        SetPointsSlowly(lastPoints - pointsToDeduct);
    }
    public void AddPointsSlowly(int pointsToDeduct)
    {
        SetPointsSlowly(lastPoints + pointsToDeduct);
    }
    private void SetPointsSlowly(int target)
    {
        if (target == lastPoints)
            return;

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            if (isFirstTime)
            {
                yield return GameScene.OneSecondWaiter;
                isFirstTime = false;
            }

            int current = lastPoints;

            while (current != target)
            {
                int diff = target - current;

                int absDiff = Mathf.Abs(diff);

                int multiplier = Mathf.Max(1, (int)Mathf.Pow(10, Mathf.FloorToInt(Mathf.Log10(absDiff))));

                current += (int)Mathf.Sign(diff) * multiplier;

                pointsText.text = $"{current}<sprite=0>";
                aSource.PlayOneShot(pointsClip);
                yield return pointsWaiter;
            }

            lastPoints = current;
        }
    }
}

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private enum RegisteredSerializations
    {
        SteamId,
    }

    const int MAX_PLAYERS = 3;

    public static InstantEvent OnConnectedToPhoton = new InstantEvent(() => IsConnected, false);

    /// <summary>Oda adýný döner.</summary>
    public static InstantEvent<string> OnJoinedToRoom = new InstantEvent<string>(() => InRoom, () => PhotonNetwork.CurrentRoom.Name, false);
    public static PhotonManager Instance { get; private set; }
    public static WaitUntil NotConnecting = new WaitUntil(() => !IsConnecting);

    public static bool IsConnected { get; private set; }
    public static bool IsConnecting { get => isConnecting; private set 
        { 
            isConnecting = value; 
            Debug.Log($"<color=cyan>isConnecting is now {value}</color>"); 
        } 
    }
    private static bool isConnecting;
    public static bool InRoom { get; set; }
    public static bool InLobby { get; set; }

    public const string GAME_STARTING_KEY = "gameStarting";
    public const string GAME_STARTED_KEY = "gameStarted";
    public const string WAVES_STARTED_KEY = "wavesStarted";
    public const string ROOM_OWNER_NAME_KEY = "ron";

    public const string IM_READY_KEY = "imReady";

    public static List<RoomInfo> CurrentRoomsInfo { get; private set; }
    public static bool IsJoiningRoom { get; private set; }
    public static event Action<List<RoomInfo>> OnCurrentRoomsChanged;
    public static event Action OnStartedJoiningRoom;
    public static event Action OnStartedLeavingRoom;
    public static event Action OnRoomLeft;
    public static event Action OnHostLeft;

    [SerializeField] private PlayerController plyPrefab;

    private void Awake()
    {
        Instance = this;

        Initialize();
    }
    public void Initialize()
    {
        PhotonPeer.RegisterType(typeof(CSteamID), (byte)RegisteredSerializations.SteamId, PhotonSerializers.SerializeCSteamID, PhotonSerializers.DeserializeCSteamID);

        StartCoroutine(WaitTillConnection(true));
    }
    public IEnumerator WaitTillConnection(bool tryQuickMatch)
    {
        Debug.LogWarning("waiting for connection to end");
        yield return NotConnecting;
        Debug.LogWarning("waited enough for connection to end");

        bool connectedToMaster = false;

        if (!IsConnected && !PhotonNetwork.OfflineMode)
        {
            Debug.Log("started connecting to master server");
            IsConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
            connectedToMaster = true;
            yield return new WaitUntil(() => IsConnected);
        }

        if (connectedToMaster && !InLobby)
        {
            PhotonNetwork.JoinLobby();
            
            Debug.Log("started connecting to lobby");
            IsConnecting = true;
            yield return new WaitUntil(() => InLobby);
            
        }

        if (!tryQuickMatch)
            yield break;

        if (GameEvents.GameState == Game_State.GameScene)
        {
            QuickMatch();
            yield break;
        }
    }
    public IEnumerator Disconnect()
    {
        if (InLobby)
        {
            IsConnecting = true;
            PhotonNetwork.LeaveLobby();
            yield return new WaitUntil(() => !InLobby);
        }
            
        IsConnecting = true;
        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !IsConnected);
    }
    private void OnSteamInitialized()
    {
        PhotonNetwork.LocalPlayer.NickName = SteamManager.LocalSteamNickName;
    }
    public void QuickMatch()
    {
        Debug.Log("started from the game scene, creating a lobby!");
        PhotonNetwork.JoinOrCreateRoom("goBrr", new RoomOptions() { MaxPlayers = 8, IsOpen = true, IsVisible = true }, TypedLobby.Default);
    }
    public void SpawnPlayer()
    {
        var gameScene = SceneLoadedHandler.GetSceneAs<GameScene>();

        int actorNumber = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % 3;

        if (PhotonNetwork.OfflineMode)
        {
            actorNumber = 0;
        }

        var spawnPoint = gameScene.SpawnPoints[actorNumber];

        var spawnedObject = PhotonNetwork.Instantiate(plyPrefab.name, spawnPoint.position, spawnPoint.rotation);

        var ply = spawnedObject.GetComponent<PlayerController>();

        ply.photonView.RPC(nameof(ply.InitializePlayerRpc), RpcTarget.AllBufferedViaServer, GameEvents.SelectedClass);

        ImReady();
    }
    private void ImReady()
    {
        var hashtable = new ExitGames.Client.Photon.Hashtable
        {
            { IM_READY_KEY, true }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

        if (!PhotonNetwork.IsMasterClient)
            return;

        StartCoroutine(WaitForAllPlayersGetReady());
    }
    public IEnumerator WaitForAllPlayersGetReady()
    {
        yield return new WaitUntil(IsAllPlayersReady);

        var customProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        customProperties[GAME_STARTING_KEY] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }
    private bool IsAllPlayersReady()
    {
        foreach (var item in PhotonNetwork.CurrentRoom.Players)
        {
            if (!item.Value.CustomProperties.TryGetValue(IM_READY_KEY, out object value) || !(bool)value)
            {
                Debug.Log("Adamýmýz hazýr deðildi!");
                return false;
            }
        }
        return true;
    }
    public void CreateRoom(bool isPublic)
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return StartCoroutine(WaitTillConnection(false));

            string publicPrivateText = isPublic ? "public" : "private";

            Debug.Log($"<color=lime>creating a {publicPrivateText} lobby.</color>");

            IsJoiningRoom = true;
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MAX_PLAYERS, IsOpen = true, IsVisible = isPublic, CustomRoomProperties = GetDefaultHash(), CustomRoomPropertiesForLobby = new string[] { ROOM_OWNER_NAME_KEY } }, TypedLobby.Default);
            OnStartedJoiningRoom?.Invoke();
            yield break;
        }
    }
    public void JoinRoom(string roomName)
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return StartCoroutine(WaitTillConnection(false));

            IsJoiningRoom = true;
            PhotonNetwork.JoinRoom(roomName);
            OnStartedJoiningRoom?.Invoke();
        }
    }
    public bool LeaveRoom()
    {
        if (InRoom)
        {
            PhotonNetwork.LeaveRoom();
            OnStartedLeavingRoom?.Invoke();
            return true;
        }
        Debug.Log("not in a room...");
        OnRoomLeft?.Invoke();
        return false;
    }
    public override void OnConnectedToMaster()
    {
        //offline olunca bu çaðýrýlýyor...
        if (PhotonNetwork.OfflineMode)
        {
            Debug.LogWarning("skipping OnConnectedToMaster call because we are offline!");
            return;
        }

        base.OnConnectedToMaster();

        Debug.Log("we are now connected to master server");

        SteamManager.OnSteamInitialized.SubscribeToEvent(OnSteamInitialized);

        IsConnected = true;
        IsConnecting = false;
        OnConnectedToPhoton.Invoke();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        IsConnecting = false;
        InLobby = true;
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        InLobby = false;
        Debug.Log("left lobby!");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        InRoom = true;
        IsJoiningRoom = false;

        if (!PhotonNetwork.OfflineMode)
        {
            var roomName = PhotonNetwork.CurrentRoom.Name;

            if (roomName.Length <= 12)
            {
                roomName += "***********";
            }

#if UNITY_EDITOR
            Debug.Log($"connected room name: {roomName}");
#else
            Debug.Log($"connected room name: {roomName[..^12]}***********");
#endif
            }


        OnJoinedToRoom.Invoke();
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        Debug.Log("left room!");

        InRoom = false;

        OnRoomLeft?.Invoke();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        SteamManager.OnSteamInitialized.UnsubscribeToEvent(OnSteamInitialized);

        IsConnected = false;
        IsConnecting = false;

        Debug.Log("we are now offline!");
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.TryGetValue(GAME_STARTING_KEY, out object value) && (bool)value)
        {
            SceneLoadedHandler.GetSceneAs<GameScene>().StartCountdown(3);
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        OnHostLeft?.Invoke();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        CurrentRoomsInfo ??= new List<RoomInfo>();

        for (int i = 0; i < roomList.Count; i++)
        {
            var item = roomList[i];
            RoomInfo matchingInfo = null;
            try
            {
                matchingInfo = CurrentRoomsInfo.Where(x => x.Name == item.Name).First();
            }
            catch (Exception)
            {

            }

            if (matchingInfo != null)
            {
                if (item.RemovedFromList)
                {
                    Debug.LogError("removed a room!");
                    CurrentRoomsInfo.Remove(matchingInfo);
                }
                else
                {
                    Debug.LogError("updated a room!");
                    CurrentRoomsInfo[CurrentRoomsInfo.IndexOf(matchingInfo)] = item;
                }
            }
            else if (item.IsVisible && item.IsOpen)
            {
                Debug.LogError("added a new room!");
                CurrentRoomsInfo.Add(item);
            }
        }
        OnCurrentRoomsChanged?.Invoke(CurrentRoomsInfo);
    }
    public ExitGames.Client.Photon.Hashtable GetDefaultHash()
    {
        return new ExitGames.Client.Photon.Hashtable
        {
            { GAME_STARTING_KEY, false },
            { GAME_STARTED_KEY, false },
            { WAVES_STARTED_KEY, false},
            { ROOM_OWNER_NAME_KEY, PhotonNetwork.NickName }
        };
    }
}
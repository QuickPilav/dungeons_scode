using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct TransformWithIndex
{
    public int spawnIndex;
    public Transform transform;
}
/*
[System.Serializable]
public class ListWrapper<T>
{
    public List<T> list;
}
*/
[System.Serializable]
public struct EnemySpawnData
{
    public Enemies enemyType;
    public int spawnAmount;
    public TransformWithIndex[] overridesSpawnPoint;
}

[System.Serializable]
public struct Wave
{
    public EnemySpawnData[] enemiesThisWave;
}

[System.Serializable]
public struct WaveEventFlood
{
    public int floodIndex;
    public int floodWaveIndex;
}

[System.Serializable]
public struct WaveEvent
{
    public bool isDisabled;
    public Optional<int> overallWaveIndex;
    public Optional<WaveEventFlood> currentFloodWaveIndex;

    public UnityEvent onWaveStarted;
}


public class WaveManager : MonoBehaviourPunCallbacks
{
    public static bool GameStarted { get; private set; }
    public static WaveManager Instance;

    public const int WAVE_AMOUNT = 50; //50
    private const int XP_ADDITION = 2;
#if UNITY_EDITOR
    private const float RANDOM_PICKUP_SPAWN_RATE = 10f;
    private const float WAVE_MAX_LENGTH = 40f;
#else
    private const float RANDOM_PICKUP_SPAWN_RATE = 15f;
    private const float WAVE_MAX_LENGTH = 125f;
#endif

    private readonly WaitForSeconds roundPauseWaiter = new WaitForSeconds(1);
    private readonly WaitForSeconds waveBetweenTimer = new WaitForSeconds(5);
    private readonly WaitForSeconds enemySpawnWaiter = new WaitForSeconds(.25f);
#if UNITY_EDITOR
    private readonly WaitForSeconds antiRefund = new WaitForSeconds(5);
#else
    private readonly WaitForSeconds antiRefund = new WaitForSeconds(20);
#endif

    public InstantEvent OnGameStarted = new InstantEvent(() => GameStarted, false);
    public event Action<int> OnNewWaveStarted;
    public int CurrentFloodIndex => currentFloodIndex;
    public int OverallWaveIndex
    {
        get => overallWaveIndex;
        private set
        {
            int diff = value - overallWaveIndex;
            overallWaveIndex = value;

            SaveSocket.CurrentSave.stats.WavesSurvived += diff;
            SaveSocket.Save();
        }
    }
    public int CurrentFloodWaveIndex { get; private set; }
    private bool PauseWaveStarting
    {
        get => pauseWaveStarting;
        set
        {
            pauseWaveStarting = value;

            if (value)
            {
                foreach (var item in pausedWaveArrows)
                    item.Play();
            }
            else
            {
                foreach (var item in pausedWaveArrows)
                    item.Stop();
            }

        }
    }
    private int ExperienceToGive
    {
        get => experienceToGive;
        set
        {
            int diff = value - experienceToGive;
            experienceToGive = value;

            InGameUI.Instance.ShowExperiencePoint(diff);
            ProgressionSystem.AddExperience(diff);
        }
    }


    [SerializeField] private Transform randomPickupsHolder;
    [SerializeField] private Transform gasFloodsParent;
    [SerializeField] private Transform protectPointsParent;
    [SerializeField] private ParticleSystem[] pausedWaveArrows;
    [SerializeField] private Wave[] waves;
    [SerializeField] private List<Transform> spawnPointsParents;

    [SerializeField] private WaveEvent[] waveEvents;

    private bool pauseWaveStarting;

    private float waveTimer;
    private float randomPickupSpawnTimer;

    private int currentFloodIndex;

    private static bool gameEnded;
    private List<EnemyAI> enemies;
    private int overallWaveIndex;
    private List<PlayerController> activePlayers;
    private bool everybodySpawned;
    private int experienceToGive = 1;
    private bool inWave;

    private void Awake()
    {
        gameEnded = false;
        Instance = this;
        activePlayers = new List<PlayerController>();
        PauseWaveStarting = false;


        for (int i = 0; i < gasFloodsParent.childCount; i++)
        {
            gasFloodsParent.GetChild(i).gameObject.SetActive(false);
        }

    }
    private void Start()
    {
        PhotonManager.OnHostLeft += OnHostLeft;
    }
    private void OnDestroy()
    {
        PhotonManager.OnHostLeft -= OnHostLeft;
        Instance = null;
    }
    private void OnHostLeft()
    {
        photonView.RPC(nameof(FinishGameRpc), RpcTarget.AllViaServer, false);
    }
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        enemies = new List<EnemyAI>();

        StartCoroutine(StartWave(0));
        Debug.Log("Oyun baþlýyor!");

        currentFloodIndex = 0;

        StartFlooding(currentFloodIndex);

        var properties = PhotonNetwork.CurrentRoom.CustomProperties;
        properties[PhotonManager.WAVES_STARTED_KEY] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }
    private void OnProtectPointExploded(IDamagable protectPoint)
    {
        PauseWaveStarting = true;
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("patladý ancak biz master client deðiliz");
            return;
        }

        CurrentFloodWaveIndex = 0;

        currentFloodIndex++;

        foreach (var item in activePlayers)
        {
            item.IsSafe = false;
        }

        if (currentFloodIndex > protectPointsParent.childCount)
        {
            CheckGameState(out _);
        }
        else
        {
            StartFlooding(currentFloodIndex);
        }
    }
    private void Update()
    {
        if (!inWave || !PhotonNetwork.IsMasterClient)
            return;

        waveTimer += Time.deltaTime;
        randomPickupSpawnTimer += Time.deltaTime;

        if (randomPickupSpawnTimer >= RANDOM_PICKUP_SPAWN_RATE)
        {
            randomPickupSpawnTimer -= RANDOM_PICKUP_SPAWN_RATE;
            SpawnRandomPickup();
        }

        if (waveTimer >= WAVE_MAX_LENGTH)
        {
            Debug.Log("Dalga çok uzun sürdü, ölün!!!!");
            EndWave();
        }
    }
    private void SpawnRandomPickup()
    {
        Vector3 pos = randomPickupsHolder.GetChild(UnityEngine.Random.Range(0, randomPickupsHolder.childCount)).position;

        RandomPickupType randomType = (RandomPickupType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(RandomPickupType)).Length);

        SpawnManager.Instance.SpawnRandomPickup(pos, randomType);
    }
    public IEnumerator StartWave(int waveIndex)
    {
        bool waitedPause = PauseWaveStarting;
        while (PauseWaveStarting)
        {
            Debug.Log("beklemedeyiz...");
            yield return roundPauseWaiter;
        }

        if (waitedPause)
        {
            yield return waveBetweenTimer;
        }

        photonView.RPC(nameof(StartWaveRpc), RpcTarget.AllViaServer, waveIndex);
    }
    public void EndWave()
    {
        photonView.RPC(nameof(EndWaveRpc), RpcTarget.AllViaServer);
    }
    [PunRPC]
    public void EndWaveRpc()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            inWave = false;

            if (PhotonNetwork.IsMasterClient && enemies.Count != 0)
            {
                //to avoid list modification in foreach loop
                List<EnemyAI> enemiesCopied = new(enemies);
                foreach (var item in enemiesCopied)
                {
                    item.OnDeath -= OnEnemyDied;
                    item.PhotonView.RPC(nameof(item.TakeDamageRpc), RpcTarget.MasterClient, PhotonNetwork.MasterClient, int.MaxValue, DamageType.Explosion);
                }
                enemies.Clear();
            }

            OverallWaveIndex++;
            CurrentFloodWaveIndex++;

            if (!PhotonNetwork.IsMasterClient)
                yield break;

            if(CheckGameState(out _))
            {
                //oyun bitti kazandýk!
                yield break;
            }

            yield return waveBetweenTimer;

            StartCoroutine(StartWave(overallWaveIndex));
            /*
            if (WaveIndex < waves.Length)
            {
                StartCoroutine(StartWave(WaveIndex));
            }
            else
            {
                //OYUN BITTI
                //DEBUG
                CheckGameState();
                WaveIndex = 0;
                StartCoroutine(StartWave(WaveIndex));
            }
            */
        }
    }
    [PunRPC]
    private void StartWaveRpc(int waveIndex)
    {
        Wave waveToStart = waves[Mathf.Min(waveIndex, waves.Length - 1)];

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            ExperienceToGive += XP_ADDITION;

            inWave = true;

            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < waveEvents.Length; i++)
                {
                    var waveE = waveEvents[i];
                    if (waveE.isDisabled)
                        continue;

                    if (waveE.overallWaveIndex.Enabled && waveE.overallWaveIndex.Value != OverallWaveIndex)
                        continue;

                    if (waveE.currentFloodWaveIndex.Enabled && (waveE.currentFloodWaveIndex.Value.floodWaveIndex != CurrentFloodWaveIndex || waveE.currentFloodWaveIndex.Value.floodIndex != CurrentFloodIndex))
                        continue;

                    photonView.RPC(nameof(InvokeWaveEventEventRpc), RpcTarget.AllViaServer, i);
                }
            }

            OnNewWaveStarted?.Invoke(OverallWaveIndex);
            yield return antiRefund;

            randomPickupSpawnTimer = 0f;
            waveTimer = 0f;

            if (PhotonNetwork.IsMasterClient)
            {
                for (int ix = 0; ix < waveToStart.enemiesThisWave.Length; ix++)
                {
                    var waveEnemies = waveToStart.enemiesThisWave[ix];
                    for (int i = 0; i < waveEnemies.spawnAmount; i++)
                    {
                        if (!IsOverrideSpawnPoint(ref waveEnemies.overridesSpawnPoint, i, out Transform spawnPoint))
                        {
                            spawnPoint = GetRandomSpawnPoint();
                        }
                        var item = waveToStart.enemiesThisWave[i % waveToStart.enemiesThisWave.Length];
                        var enemySpawned = SpawnManager.Instance.SpawnEnemy<EnemyAI>(waveEnemies.enemyType, spawnPoint);

                        enemySpawned.SetTarget(protectPointsParent.GetChild(currentFloodIndex));
                        enemies.Add(enemySpawned);
                        enemySpawned.OnDeath += OnEnemyDied;
                        yield return enemySpawnWaiter;

                    }
                }
            }
        }
    }
    public bool IsOverrideSpawnPoint(ref TransformWithIndex[] list, int spawnIndex, out Transform outItem)
    {
        if (list.Length == 0)
        {
            goto noneFound;
        }

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].spawnIndex == spawnIndex)
            {
                outItem = list[i].transform;
                return true;
            }
        }

    noneFound:

        outItem = null;
        return false;
    }
    private void OnEnemyDied(IDamagable died)
    {
        enemies.Remove(died.ObjectTransform.GetComponent<EnemyAI>());
        if (enemies.Count == 0)
        {
            EndWave();
        }
    }
    public void AddActivePlayer(PlayerController ply)
    {
        activePlayers.Add(ply);
        ply.OnDeath += OnAnyPlayerDied;
        ply.OnIsSafeChanged += OnAnyPlayerIsSafe;
    }
    public void RemoveActivePlayer(PlayerController ply)
    {
        activePlayers.Remove(ply);
        ply.OnDeath -= OnAnyPlayerDied;
        ply.OnIsSafeChanged -= OnAnyPlayerIsSafe;
        CheckGameState(out _);
    }
    private void OnAnyPlayerDied(IDamagable ply)
    {
        CheckGameState(out _);
    }
    private bool CheckGameState(out bool winState)
    {
        winState = false;

        if (!PhotonNetwork.IsMasterClient || gameEnded)
            return false;

        if(overallWaveIndex >= WAVE_AMOUNT)
        {
            winState = true;
            photonView.RPC(nameof(FinishGameRpc), RpcTarget.AllViaServer, true);
            return true;
        }

        if (currentFloodIndex >= gasFloodsParent.childCount)
        {
            photonView.RPC(nameof(FinishGameRpc), RpcTarget.AllViaServer, false);
            return true;
        }

        if (IsEverybodyDead())
        {
            try
            {
                photonView.RPC(nameof(FinishGameRpc), RpcTarget.AllViaServer, false);
            }
            catch (Exception)
            {
            }
            return true;
        }
        return false;
    }
    [PunRPC]
    private void FinishGameRpc(bool didWin)
    {
        PhotonManager.OnHostLeft -= OnHostLeft;
        PlayerController.ClientInstance.Value.GameStarted = false;

        InGameUI.Instance.EndGame(didWin);

        gameEnded = true;
        GameStarted = false;
    }
    public void StartFlooding(int sectorIndex)
    {
        photonView.RPC(nameof(RpcFlood), RpcTarget.AllViaServer, sectorIndex);
    }
    [PunRPC]
    private void RpcFlood(int floodIndex)
    {
        currentFloodIndex = floodIndex;

        for (int i = 0; i < floodIndex; i++)
        {
            gasFloodsParent.GetChild(i).gameObject.SetActive(true);
        }

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            while (PauseWaveStarting)
            {
                yield return roundPauseWaiter;
            }
            protectPointsParent.GetChild(floodIndex).
                GetComponent<ProtectPoint>().SetAsCurrentProtectionPoint();
        }
    }
    private bool IsEverybodyDead()
    {
        if (!everybodySpawned)
            return false;

        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (!activePlayers[i].IsDead)
            {
                Debug.Log("en az 1 kiþi ölü deðil!");
                return false;
            }
        }
        return true;
    }
    public Transform GetRandomSpawnPoint()
    {
        Transform parent = spawnPointsParents[currentFloodIndex];
        if (parent.GetChild(parent.childCount - 1).gameObject.activeSelf)
        {
            return parent.GetChild(UnityEngine.Random.Range(0, parent.childCount));
        }

        var children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                children.Add(child);
            }
        }

        return children[UnityEngine.Random.Range(0, children.Count)];
    }
    private void OnAnyPlayerIsSafe(PlayerController ply)
    {
        foreach (var item in activePlayers)
        {
            if (!item.IsSafe)
            {
                return;
            }
        }

        //herkes güvende, dalgayý baþlatabiliriz...
        PauseWaveStarting = false;
    }
    private void OnDrawGizmos()
    {
        float i = 0;
        foreach (var item in spawnPointsParents)
        {
            Gizmos.color = Color.Lerp(Color.cyan, Color.magenta, i);

            try
            {
                foreach (Transform itemn in item)
                {
                    try
                    {
                        Gizmos.DrawWireSphere(itemn.position, .3f);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception)
            {

            }
            i += 1f / (spawnPointsParents.Count - 1);
        }
    }
    public override void OnJoinedRoom()
    {
        OnRoomPropertiesUpdate(PhotonNetwork.CurrentRoom.CustomProperties);
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (GameStarted)
            return;

        if (propertiesThatChanged.TryGetValue(PhotonManager.WAVES_STARTED_KEY, out object value))
        {
            GameStarted = (bool)value;
            if (GameStarted)
            {
                foreach (Transform item in protectPointsParent)
                {
                    item.GetComponent<ProtectPoint>().OnDeath += OnProtectPointExploded;
                }

                OnGameStarted?.Invoke();
                InGameUI.Instance.ShowWaitingForHost(false);
                InGameUI.Instance.ShowWaitingToStart(false);
            }
            else
            {
                InGameUI.Instance.ShowWaitingForHost(!PhotonNetwork.IsMasterClient);
                InGameUI.Instance.ShowWaitingToStart(PhotonNetwork.IsMasterClient);
            }
        }
    }
    public void CheckEverybodySpawned()
    {
        foreach (var item in PhotonNetwork.PlayerList)
        {
            if (item.TagObject == null)
            {
                return;
            }
        }
        everybodySpawned = true;
    }
    [PunRPC]
    public void InvokeWaveEventEventRpc(int eventIndex)
    {
        waveEvents[eventIndex].onWaveStarted.Invoke();
    }
}

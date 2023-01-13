using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback, IDamagable, IInteractableHold
{
    private const string HASH_CURRENT_WEAPON = "currentWeaponSlotIndex";
    private const string PROMPT_ENG = "Help";
    private const string PROMPT_TR = "Kaldýr";

    public const string STEAM_ID_KEY = "steamId";

    public event Action<PlayerController> OnDestroyed;

    /// <summary>Hasar velien düþman, hasar</summary>
    public Action<EnemyAI, int> OnHitEnemy;
    public Action<EnemyAI> OnKilledEnemy;

    /// <summary>Þu anki can, max can</summary>
    public Action<int, int> OnHealthChanged;

    public enum Player_Anims
    {
        Speed,
        Roll,
        TakeDamage,
        Interact,
        GunShoot,
        Downed,
        GetUp,
        Helping
    }

    public int InteractionPriority => 7;
    public bool GameStarted { get; set; }
    public bool IsPlayer => true;
    public bool IsInvisible
    {
        get => isInvisible
#if UNITY_EDITOR
            || isInvincible
#endif
            ;
        set
        {
            //this sets isInvisible to state
            photonView.RPC(nameof(SetInvisibilityRpc), RpcTarget.All, value);
        }
    }
    public bool Noclip => noclip;

    public SurfaceMaterial SurfaceMaterial => SurfaceMaterial.Human;

    public Vector3 EyePosition { get => transform.position + Vector3.up * 1.45f; }

    public int GemMultiplier { get; set; } = 1;

    public int GemsHave
    {
        get => gemsHave;
        set
        {
            gemsHave = value;
            if (photonView.IsMine)
            {
                InGameUI.Instance.UpdateGemAmount(gemsHave);
            }
        }
    }

    public int BatteryLeft
    {
        get => batteryLeftInteger;
        set
        {
            batteryLeftInteger = value;
            if (photonView.IsMine)
            {
                InGameUI.Instance.UpdateBatteryText(batteryLeftInteger);
            }
        }
    }

    private int gemsHave;
    private AudioSource aSource;

    private float toxicTimer;
    private readonly float toxicInflictionRate = 1.666f;
    private readonly int toxicInflictionDamage = 5;


    private CharacterController cc;
    private PlayerInput playerInput;
    private CameraSystem camSystem;
    private ItemSystem itemSystem;
    private InventorySystem inv;

    public PlayerStateBase CurrentState
    {
        get => currentState.Value;
        set
        {
            if (currentState.Enabled)
            {
                currentState.Value.OnStateExit();
            }

            currentState.Value = value;

            if (currentState.Enabled)
            {
                currentState.Value.OnStateEnter();
            }
        }
    }

    private Optional<PlayerStateBase> currentState;

    [SerializeField] private bool isMainMenuPlayer;
    [SerializeField] private PlayerStateGrounded groundedState;
    [SerializeField] private PlayerStateDead deadState;
    [SerializeField] private PlayerStateDowned downedState;
    [SerializeField] private PlayerStateAnimation animationState;

    [SerializeField] private LineRenderer laser;

    public LineRenderer LaserTransform { get => laser; }
    public PlayerStateGrounded GroundedState { get => groundedState; }
    public PlayerStateDead DeadState { get => deadState; }
    public PlayerStateDowned DownedState { get => downedState; }
    public PlayerStateAnimation AnimationState { get => animationState; }
    public PlayerClassHandler ClassHandler { get => classHandler; }

    public bool AffectedByToxicity => true;
    public ObservableList<ToxicGas> ToxicInflictors => toxicInflictors;

    private readonly ObservableList<ToxicGas> toxicInflictors = new ObservableList<ToxicGas>();


    [SerializeField] private PlayerClassHandler classHandler;
    [SerializeField] private PlayerAnimations playerAnims;
    [SerializeField] private Transform followTransform;
    [SerializeField] private PlayerInteraction interaction;
    [Space]
    [SerializeField] private Light flashLight;
    [SerializeField] private float batteryDepletionRate = .1f;
    [SerializeField] private float batteryLeft = 100f;
    [SerializeField] private Optional<Outline> outline;


    public Action OnRolled;

    public Vector3 NormalizedVelocity { get; set; }
    private ExitGames.Client.Photon.Hashtable hashTable;

    private float dotX;
    private float dotZ;
    public static bool DeletedFakeInstance { get; set; }

    public Transform ObjectTransform => transform;

    public int Health
    {
        get => health;
        set
        {
            if (IsDead && value > 0)
            {
                GetRevived();
            }
            else if (!IsDead && value <= 0)
            {
                Die();
            }

            health = Mathf.Clamp(value, 0, classHandler.CurrentClass.PlayerHealth);
            OnHealthChanged?.Invoke(health, classHandler.CurrentClass.PlayerHealth);
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(FlashlightRpc), RpcTarget.All, false);
                InGameUI.Instance.UpdateHealthBar(health, classHandler.CurrentClass.PlayerHealth);
            }
        }
    }

    public List<BuffsNames> ActiveBuffs { get; private set; }

    public PhotonView PhotonView { get => photonView; }
    public PlayerInteraction Interaction { get => interaction; }
    public InventorySystem Inventory { get => inv; }
    public ItemSystem ItemSystem { get => itemSystem; }
    public bool IsDead => health == 0;
    public Action<IDamagable> OnDeath { get; set; }
    public float DamageMultiplier { get; private set; } = 1f;
    public ItemSystem.Items CurrentWeaponSlotIndex
    {
        get => _currentWeaponSlotIndex;
        set
        {
            if (_currentWeaponSlotIndex == value)
                return;

            _currentWeaponSlotIndex = value;
            SetHashtable();
        }
    }

    public float HoldForSeconds => 5;

    public Optional<Outline> ObjectOutline { get => outline; }

    public float YOffset => .1f;

    public bool CantInteractWhen => photonView.IsMine || !IsDead;

    public Player_Anims CustomAnimation => Player_Anims.Helping;
    public int CustomAnimationLayer => 0;

    public bool CanMoveWhileInteracting => false;

    public List<SmokeGrenadeProjectile> SmokeInflictors => smokeInflictors;

    public bool IsSafe
    {
        get => isSafe;
        set
        {
            isSafe = value;
            OnIsSafeChanged?.Invoke(this);
        }
    }


    public static Optional<PlayerController> ClientInstance;

    public static InstantEvent<PlayerController> OnClientSpawned = new InstantEvent<PlayerController>(() => ClientInstance.Enabled, () => ClientInstance.Value, false);

    public event Action<PlayerController> OnIsSafeChanged;
    private bool isSafe = true;

    private readonly List<SmokeGrenadeProjectile> smokeInflictors = new List<SmokeGrenadeProjectile>();
    private List<PlayerController> CloseProximity;

    private ItemSystem.Items _currentWeaponSlotIndex;

    private int batteryLeftInteger = 100;
    private int health;
    private Optional<IDamagable> lastClosestEnemy;
    private Vector3 smoothedMousePos;

    private readonly WaitForSeconds getUpWaiter = new WaitForSeconds(1f);
    private WaitForSeconds damageTimer;

    private Optional<IEnumerator> speedBuffRoutine;
    private Collider[] sphereCastCols;

#if UNITY_EDITOR
    private bool isInvincible;
#endif
    private bool cantTakeDamage;
    private bool classInitialized;
    private bool isInvisible;
    private bool inControl;
    private bool inMouseControl;
    private bool noclip;


    private void Awake()
    {
        if (isMainMenuPlayer)
            return;

        cc = GetComponent<CharacterController>();
        itemSystem = GetComponent<ItemSystem>();
        inv = GetComponent<InventorySystem>();
        aSource = GetComponent<AudioSource>();
        ActiveBuffs = new List<BuffsNames>();

        outline.Remap();
        outline.Value.OutlineWidth = 0f;

        CloseProximity = new List<PlayerController>();

        hashTable = new ExitGames.Client.Photon.Hashtable
        {
            { HASH_CURRENT_WEAPON, ItemSystem.Items.None }
        };
    }
    private void Start()
    {
        if (isMainMenuPlayer)
            return;

        if (!DeletedFakeInstance)
        {
            Debug.Log("deleting prefab...");
            Destroy(gameObject);
            return;
        }

        playerAnims.Initialize(this);
        itemSystem.Initialize(this, inv, photonView);
        inv.Initialize(this, itemSystem);

        animationState.Initialize(this);
        deadState.Initialize(this);
        groundedState.Initialize(this);
        downedState.Initialize(this);

        WaveManager.Instance.OnGameStarted.SubscribeToEvent(OnWavesStarted);

        WaveManager.Instance.AddActivePlayer(this);

        if (!photonView.IsMine)
        {
            if (photonView.Owner.CustomProperties.ContainsKey(STEAM_ID_KEY))
            {
                InGameUI.Instance.UpdateGamePlayerUI(this, false);
            }

            return;
        }
        else
        {
            gameObject.AddComponent<AudioListener>();
            gameObject.AddComponent<AudioDistortionFilter>().distortionLevel = .2f;
        }

        try
        {
            InGameUI.Instance.ActivateInGameUI(true);
            Camera.main.gameObject.SetActive(false);
        }
        catch (Exception exc)
        {
            Debug.LogException(exc);
        }

        toxicInflictors.ItemAdded += OnToxicAdded;
        toxicInflictors.ItemRemoved += OnToxicRemoved;

        sphereCastCols = new Collider[30];

        damageTimer = new WaitForSeconds(.5f);


        //hashTable.Add(HASH_X, 0f);
        //hashTable.Add(HASH_Z, 0f);

        interaction.Initialize(this);

        camSystem = CameraSystem.Instance;
        camSystem.Follow(followTransform);

        ClientInstance.Value = this;
        OnClientSpawned.Invoke();

        playerInput = new PlayerInput();

        GetComponent<CircleScript>().Initialize(camSystem.Camera);

        SaveSocket.OnSettingsChanged.SubscribeToEvent(OnSettingsChanged);

        CurrentState = groundedState;

        GemsHave = 0;

        OnRoomUpdate(PhotonNetwork.CurrentRoom.CustomProperties);

        SteamManager.OnSteamInitialized.SubscribeToEvent(OnSteamInitialized);
    }

    private void OnSettingsChanged(SettingsSave x)
    {
        playerInput.Sensitivity = x.sensitivity;
        playerInput.ToggleAds = x.toggleAds;
    }

    private void OnDestroy()
    {
        if (isMainMenuPlayer)
            return;

        if (!DeletedFakeInstance)
        {
            DeletedFakeInstance = true;
            return;
        }

        if (photonView.IsMine)
        {
            ClientInstance.Value = null;
            SaveSocket.OnSettingsChanged.UnsubscribeToEvent(OnSettingsChanged);
            SteamManager.OnSteamInitialized.UnsubscribeToEvent(OnSteamInitialized);
            try
            {
                CameraSystem.Instance.Follow(null);
            }
            catch (Exception)
            {
            }
        }

        try
        {
            InGameUI.Instance.UpdateGamePlayerUI(this, isDestroyed: true);
        }
        catch (Exception)
        {

        }

        try
        {
            WaveManager.Instance.RemoveActivePlayer(this);
            WaveManager.Instance.OnGameStarted.UnsubscribeToEvent(OnWavesStarted);
        }
        catch (Exception)
        {

        }
        OnDestroyed?.Invoke(this);
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this;
    }
    private void OnSteamInitialized()
    {
        var customProps = photonView.Controller.CustomProperties;
        if (customProps.ContainsKey(STEAM_ID_KEY))
        {
            return;
        }

        customProps.Add(STEAM_ID_KEY, Steamworks.SteamUser.GetSteamID());

        photonView.Controller.SetCustomProperties(customProps);
    }
    private void OnWavesStarted()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return new WaitUntil(() => classInitialized);
            classHandler.IsPassiveAbilityActive = true;
            classHandler.CurrentClass.PassiveAbilityStart();
        }
    }
    private void Update()
    {
        if (isMainMenuPlayer)
            return;

        if (!classInitialized)
            return;

        playerAnims.Update();

        if (!IsDead)
        {
            classHandler.Update();
        }

        if (!photonView.IsMine)
        {
            // sadece biz deðil isek updatede olacak özel þeyler...

            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            TakeDamageRpc(photonView.Controller, 100, DamageType.Explosion);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            photonView.RPC(nameof(AddHealthRpc), RpcTarget.All, 100);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Vector3 v = UnityEngine.Random.insideUnitSphere * 5f;
            v.y = 5f;
            SpawnManager.Instance.SpawnRandomPickup(v, (RandomPickupType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(RandomPickupType)).Length));
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            classHandler.AbilityBar += 50;
        }
#if UNITY_EDITOR
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            isInvincible = !isInvincible;
            Debug.LogError("ölümsüzlük deðiþti!");
        }
#endif

        InputPayload lastInput = playerInput.Get(inControl, inMouseControl);
        Vector3 mousePos = HandleMousePosition(lastInput);

        HandleAnimations();
        if (inControl)
        {
            HandleFlashlight();
        }

        if (currentState.Enabled)
        {
            currentState.Value.OnStateUpdate(lastInput, mousePos);
        }

        inv.UpdateByInput(lastInput);
        itemSystem.UpdateByInput(lastInput);
        if (!IsDead)
        {
            classHandler.UpdateOwner(lastInput);
        }

        /*
        if ((float)hashTable[HASH_X] != dotX || (float)hashTable[HASH_Z] != dotZ)
        {
            SetHashtable();
        }*/

        HandleToxicity();

        inControl = GameStarted && !ClientUI.IsGamePaused;
        inMouseControl = GameStarted && !ClientUI.IsMousePaused;

#if UNITY_EDITOR
        if (DavidFDev.DevConsole.DevConsole.IsOpenAndFocused)
        {
            inControl = false;
        }
#endif
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(dotX);
            stream.SendNext(dotZ);
        }
        else if (stream.IsReading)
        {
            float dotX = (float)stream.ReceiveNext();
            float dotZ = (float)stream.ReceiveNext();

            if (!photonView.IsMine)
            {
                this.dotX = dotX;
                this.dotZ = dotZ;
                playerAnims.SetXZ(dotX, dotZ);
            }
        }
    }
    private Vector3 HandleMousePosition(InputPayload lastInput)
    {
        Vector3 mousePos = new Vector3(lastInput.mousePosition.x, 0, lastInput.mousePosition.y);

        if (lastInput.lastZoom != lastInput.zoom && lastInput.zoom)
        {
            IDamagable closestEnemy = GetClosestEnemy(10);

            lastClosestEnemy.Value = closestEnemy;
        }

        if (lastInput.zoom)
        {
            try
            {
                lastClosestEnemy.Enabled = lastClosestEnemy.Value.ObjectTransform != null;
            }
            catch (Exception)
            {
                lastClosestEnemy.Enabled = false;
            }

            if (!lastClosestEnemy.Enabled || lastClosestEnemy.Value.IsDead || Vector3.Distance(lastClosestEnemy.Value.ObjectTransform.position, transform.position) >= 10f)
            {
                IDamagable closestEnemy = GetClosestEnemy(10);

                lastClosestEnemy.Value = closestEnemy;
            }
            else
            {
                mousePos = lastClosestEnemy.Value.ObjectTransform.position;
                mousePos.x -= transform.position.x;
                mousePos.z -= transform.position.z;
            }
        }

        smoothedMousePos = Vector3.MoveTowards(smoothedMousePos, mousePos, Time.deltaTime * 100f);

        Vector3 smoothAdded = smoothedMousePos;

        smoothAdded.x += transform.position.x;
        smoothAdded.z += transform.position.z;
        smoothAdded.y = 0;

        camSystem.SetAttackPoint(lastInput, smoothAdded);
        return smoothAdded;
    }
    private void HandleAnimations()
    {
        Debug.DrawRay(transform.position, NormalizedVelocity, Color.cyan);
        dotX = Vector3.Dot(NormalizedVelocity, transform.right);
        dotZ = Vector3.Dot(NormalizedVelocity, transform.forward);

        playerAnims.SetXZ(dotX, dotZ);
    }
    private void HandleFlashlight()
    {
        if (batteryLeft <= 0 || health == 0 || !WaveManager.GameStarted)
            return;

        batteryLeft -= Time.deltaTime * batteryDepletionRate;

        int rounded = Mathf.RoundToInt(batteryLeft);
        if (rounded != BatteryLeft)
        {
            BatteryLeft = rounded;
        }

        if (batteryLeft <= 0)
        {
            photonView.RPC(nameof(FlashlightRpc), RpcTarget.All, false);
        }
        else if (!flashLight.enabled)
        {
            photonView.RPC(nameof(FlashlightRpc), RpcTarget.All, true);
        }
    }
    private void HandleToxicity()
    {
        if (ToxicInflictors.Count == 0)
            return;

        toxicTimer += Time.deltaTime;

        if (toxicTimer >= toxicInflictionRate)
        {
            TakeDamageRpc(photonView.Controller, toxicInflictionDamage, DamageType.Toxicity);

            toxicTimer -= toxicInflictionRate;
        }
    }
    private void GetRevived()
    {
        CurrentState = groundedState;

        if (!photonView.IsMine)
            return;

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            groundedState.IsGettingUp = true;
            yield return getUpWaiter;
            groundedState.IsGettingUp = false;
        }
    }
    private void Die()
    {
        CurrentState = downedState;
    }
    public void SetHashtable()
    {
        //hashTable[HASH_X] = dotX;
        //hashTable[HASH_Z] = dotZ;
        hashTable[HASH_CURRENT_WEAPON] = CurrentWeaponSlotIndex;

        PhotonNetwork.SetPlayerCustomProperties(hashTable);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != photonView.Owner)
            return;

        InGameUI.Instance.UpdateGamePlayerUI(this, false);

        if (photonView.IsMine)
            return;

        /*
        if (changedProps.Count == 1)
            return;
        for (int i = 0; i < changedProps.Count; i++)
        {
            var element = changedProps.ElementAt(i);
            Debug.LogError($"number: {targetPlayer.ActorNumber}, {element.Key} : {element.Value}");
        }

        dotX = (float)changedProps[HASH_X];
        dotZ = (float)changedProps[HASH_Z];

        playerAnims.SetXZ(dotX, dotZ);
        */

        if (changedProps.TryGetValue(HASH_CURRENT_WEAPON, out object weaponObject))
        {
            itemSystem.SetVisibility((ItemSystem.Items)weaponObject);
        }

    }
    public void AddProximityPlayer(PlayerController ply)
    {
        if (CloseProximity.Contains(ply))
            return;

        ply.OnDestroyed += RemoveProximityPlayer;

        CloseProximity.Add(ply);
        Debug.Log($"Yaklaþtý, {ply.classHandler.CurrentClass}!");

        if (ply.classHandler.CurrentClass is PlayerClassIbo ibo)
        {
            DamageMultiplier += ibo.AdditiveDamageMultiplierToProximity;
            InGameUI.Instance.SetBuffState(BuffsNames.IboBuff, true);
            ActiveBuffs.Add(BuffsNames.IboBuff);
        }
    }
    public void RemoveProximityPlayer(PlayerController ply)
    {
        if (!CloseProximity.Contains(ply))
            return;

        ply.OnDestroyed -= RemoveProximityPlayer;

        CloseProximity.Remove(ply);
        Debug.Log($"Uzaklaþtý, {ply.classHandler.CurrentClass}!");

        if (ply.classHandler.CurrentClass is PlayerClassIbo ibo)
        {
            ActiveBuffs.Remove(BuffsNames.IboBuff);
            InGameUI.Instance.SetBuffState(BuffsNames.IboBuff, false);
            DamageMultiplier -= ibo.AdditiveDamageMultiplierToProximity;

        }
    }
    public void Move(Vector3 moveDir) 
    {
        if(noclip)
        {
            transform.position += moveDir * Time.deltaTime;
        }
        else
        {
            cc.Move(moveDir * Time.deltaTime);
        }
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (!photonView.IsMine)
            return;

        OnRoomUpdate(propertiesThatChanged);
    }
    private void OnRoomUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        /*
        for (int i = 0; i < propertiesThatChanged.Count; i++)
        {
            var element = propertiesThatChanged.ElementAt(i);
            //Debug.LogError($"{element.Key} : {element.Value}");
        }
        */
        if (propertiesThatChanged.TryGetValue(PhotonManager.GAME_STARTED_KEY, out object value) && (bool)value)
        {
            GameStarted = true;
        }
    }
    public void PlayDialogue(PlayerClassBase.CharacterSoundIndex dialogueIndex)
    {
        try
        {
            aSource.Stop();
            aSource.clip = classHandler.GetSound(dialogueIndex);
            aSource.Play();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    public void Ping()
    {
        photonView.RPC(nameof(PingRpc), RpcTarget.All, camSystem.GetMousePosition());
    }
    public void RefillBattery()
    {
        batteryLeft = 100;
    }
    public void GiveSpeedBuff(float multiplier, float activeTime)
    {
        if (speedBuffRoutine.Enabled)
        {
            StopCoroutine(speedBuffRoutine.Value);
        }

        speedBuffRoutine.Value = enumerator();

        StartCoroutine(speedBuffRoutine.Value);

        IEnumerator enumerator()
        {
            groundedState.SetMoveBuff(multiplier);
            InGameUI.Instance.SetBuffState(BuffsNames.SpeedBuff, true);
            ActiveBuffs.Add(BuffsNames.SpeedBuff);

            yield return new WaitForSeconds(activeTime);

            InGameUI.Instance.SetBuffState(BuffsNames.SpeedBuff, false);
            ActiveBuffs.Remove(BuffsNames.SpeedBuff);
            groundedState.SetMoveBuff(0f);

        }
    }
    public bool FindEnemies(float range, out List<IDamagable> damagables)
    {
        damagables = new List<IDamagable>();
        IDamagable myDmg = this;

        int colsLength = Physics.OverlapSphereNonAlloc(transform.position, range, sphereCastCols, LayerManager.OnlyEnemies);
        for (int i = 0; i < colsLength; i++)
        {
            if (sphereCastCols[i] == null)
                break;

            var t = sphereCastCols[i].transform;

            Vector3 diff = t.position - EyePosition;
            Debug.DrawRay(EyePosition, diff, Color.blue, 1f);
            if (Physics.Raycast(EyePosition, diff.normalized, diff.magnitude, LayerManager.GroundLayer, QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            IDamagable dmg = sphereCastCols[i].GetComponentInParent<IDamagable>();
            if (dmg != null)
            {
                if (dmg.IsDead || dmg == myDmg || dmg.IsPlayer)
                    continue;

                damagables.Add(dmg);
            }
        }

        if (damagables.Count == 0)
            return false;

        return true;
    }
    public IDamagable GetClosestEnemy(float range)
    {
        if (FindEnemies(range, out List<IDamagable> damagables))
        {
            return damagables.OrderBy((d) => (d.ObjectTransform.position - transform.position).sqrMagnitude).First();
        }
        return null;
    }
    public void OnInteractionStart(PlayerController ply)
    {

    }
    public void OnInteractionFailed(PlayerController ply)
    {

    }
    public void OnInteraction(PlayerController ply)
    {
        photonView.RPC(nameof(AddHealthRpc), RpcTarget.All, 10);
    }
    private void OnToxicAdded(ObservableList<ToxicGas> sender, ListChangedEventArgs<ToxicGas> e)
    {
        InGameUI.Instance.UpdateToxicState(true);
    }
    private void OnToxicRemoved(ObservableList<ToxicGas> sender, ListChangedEventArgs<ToxicGas> e)
    {
        bool stillInToxic = sender.Count > 0;
        InGameUI.Instance.UpdateToxicState(stillInToxic);
        if (!stillInToxic)
        {
            toxicTimer = 0;
        }
    }
    public string GetPrompt(Language language)
    {
        string first = language == Language.Turkish ? PROMPT_TR : PROMPT_ENG;
        string second = photonView.Controller.NickName;

        return $"{first} {second}";
    }
    [PunRPC]
    public void InitializePlayerRpc(PlayerClass plyClass)
    {
        classHandler.Initialize(this, plyClass);
        Health = classHandler.CurrentClass.PlayerHealth;
        groundedState.SetMoveSpeed(classHandler.CurrentClass.MoveSpeed);

        classInitialized = true;

        itemSystem.OnClassInitialized(classHandler.CurrentClass);

        WaveManager.Instance.CheckEverybodySpawned();
    }
    //runs on all
    [PunRPC]
    public void OnTakeDamageRpc(int damage, int currentHealth, DamageType dmgType)
    {
        animationState.Play(animationState.GetAnimationHash((int)Player_Anims.TakeDamage), layer: 2);

        SpawnManager.Instance.SpawnDamagePopup(transform.position, damage, dmgType);

        if (currentHealth <= 0)
        {
            Debug.Log($"{photonView.OwnerActorNr} is dead.");
            OnDeath?.Invoke(this);
        }

        if (photonView.IsMine)
            return;

        Health = currentHealth;
    }
    //runs on owner
    [PunRPC]
    public void TakeDamageRpc(Player inflictor, int amount, DamageType damageType)
    {
        if (groundedState.IsRolling)
            return;

        if (cantTakeDamage || IsDead || groundedState.IsSwingingAxe || groundedState.IsGettingUp)
            return;

#if UNITY_EDITOR
        if (isInvincible)
            return;
#endif

        Health -= amount;

        camSystem.ShakeOnce(1f);

        photonView.RPC(nameof(OnTakeDamageRpc), RpcTarget.All, amount, Health, damageType);

        StartCoroutine(noDamage());

        IEnumerator noDamage()
        {
            cantTakeDamage = true;
            yield return damageTimer;
            cantTakeDamage = false;
        }
    }

    [PunRPC]
    public void PlayAnimationRpc(Player_Anims anim, int layer)
    {
        animationState.Play(animationState.GetAnimationHash((int)anim), layer: layer);
    }
    [PunRPC]
    public void AddAbilityBarRpc(int amountFills)
    {
        classHandler.AbilityBar += amountFills;
    }
    [PunRPC]
    private void PingRpc(Vector3 pos)
    {
        SpawnManager.Instance.SpawnPing(pos);
        PlayDialogue(PlayerClassBase.CharacterSoundIndex.Ping);
    }
    [PunRPC]
    public void AddHealthRpc(int ultimateHealAmount)
    {
        Health += ultimateHealAmount;
    }
    [PunRPC]
    private void SetInvisibilityRpc(bool state)
    {
        isInvisible = state;
        Debug.Log($"gelen görünmezlik {state}");
        classHandler.SetInvisibility(state);
    }
    [PunRPC]
    private void FlashlightRpc(bool state)
    {
        flashLight.enabled = state;
    }
    public void ToggleClipping()
    {
        noclip = !noclip;
        Debug.Log($"noclip is now set to {noclip}");
    }
}

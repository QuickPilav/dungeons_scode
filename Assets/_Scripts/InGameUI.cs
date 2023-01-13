using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BuffsNames
{
    IboBuff,
    SpeedBuff
}

public class InGameUI : StaticInstance<InGameUI>
{
    public bool GameEndedUIActive { get; private set; }

    [SerializeField] private GameObject inGameUI;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [Space]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TextMeshProUGUI promptText;
    [Space]
    [SerializeField] private TextMeshProUGUI bulletsText;
    [Space]
    [SerializeField] private AudioClip[] hitSounds;
    [Space]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject alivePanel;
    [Space]
    [SerializeField] private AudioClip[] buttonSounds;
    [Space]
    [SerializeField] private CanvasGroup bossUI;
    [SerializeField] private Slider bossRealHealthSlider;
    [SerializeField] private Slider bossLerpedHealthSlider;
    [Space]
    [SerializeField] private AudioSource bossFightMusic;
    [Space]
    [SerializeField] private TextMeshProUGUI countdownText;
    [Space]
    [SerializeField] private RectTransform topBlackBar;
    [SerializeField] private RectTransform bottomBlackBar;
    [Space]
    [SerializeField] private TextMeshProUGUI waitingToStart;
    [SerializeField] private TextMeshProUGUI waitingForHost;
    [Space]
    [SerializeField] private Slider ultimateBar;
    [SerializeField] private TextMeshProUGUI ultimateBarText;
    [SerializeField] private GameObject ultiReadyObj;
    [Space]
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI batteryText;
    [Space]
    [SerializeField] private GameObject[] buffsUI;
    [SerializeField] private Animator healAnim;
    [SerializeField] private Animator damageAnim;
    [Space]
    [SerializeField] private Image castFill;
    [SerializeField] private GameObject castHolder;
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject winMenu;
    [Space]
    [SerializeField] private Transform gamePlayerUIParent;
    [SerializeField] private InGamePlayerUI gamePlayerUIPrefab;
    [Space]
    [SerializeField] private Animator toxicAnimator;
    [Space]
    [SerializeField] private Animator waveAnimator;
    [SerializeField] private LanguageTextExtra waveIndicator;
    [Space]
    [SerializeField] private ExperienceUI experienceUIPrefab;
    [SerializeField] private Transform experienceUIParent;
    [SerializeField] private Transform experienceUIPosition;

    private Dictionary<int, InGamePlayerUI> gamePlayerUIs;
    private readonly int toxicHash = Animator.StringToHash("Toxic");
    private readonly int toxicOutHash = Animator.StringToHash("ToxicOut");
    private readonly int healHash = Animator.StringToHash("Heal");
    private readonly int emptyHash = Animator.StringToHash("Empty");

    private float castTimer;
    private float castMax;

    private bool IsCasting
    {
        get => isCasting;
        set
        {
            isCasting = value;
            castHolder.SetActive(value);
        }
    }

    private bool isCasting;

    private readonly int newWaveAnimationHash = Animator.StringToHash("WaveStarting");
    private readonly WaitForSeconds newWaveWaiter = new WaitForSeconds(1.63f);

    private AudioSource aSource;
    private bool inBossFight;
    private IEnumerator blackBarRoutine;
    private int lastHealth = -1;
    private bool lastToxicState;

    private void Start()
    {
        ActivateInGameUI(false);
        promptObject.SetActive(false);

        bossUI.alpha = 0;

        aSource = GetComponent<AudioSource>();

        for (int i = 0; i < buffsUI.Length; i++)
        {
            buffsUI[i].SetActive(false);
        }

        GetComponent<ShopUI>().Initialize();
        GetComponent<HeroSelectorUI>().Initialize();

        IsCasting = false;

        winMenu.SetActive(false);
        loseMenu.SetActive(false);

        WaveManager.Instance.OnNewWaveStarted += OnNewWaveStarted;

        waveIndicator.DontSetAutomatically = true;

        PoolManager.Instance.CreatePool(experienceUIPrefab.gameObject, 6, experienceUIParent, experienceUIPosition.position, Quaternion.identity);

    }
    private void Update()
    {
        if (inBossFight)
        {
            bossLerpedHealthSlider.value = Mathf.Lerp(bossLerpedHealthSlider.value, bossRealHealthSlider.value, Time.deltaTime * 5f);
        }

        if (IsCasting)
        {
            castTimer -= Time.deltaTime;
            castFill.fillAmount = castTimer / castMax;

            if (castTimer <= 0)
            {
                IsCasting = false;
            }
        }
    }
    public void ActivateInGameUI(bool state)
    {
        inGameUI.SetActive(state);
    }
    public void UpdateHealthBar(int newHealth, int maxHealth)
    {
        if (healthBar.value < maxHealth)
        {
            healthBar.gameObject.SetActive(true);
        }

        if (lastHealth != -1 && !lastToxicState)
        {
            if (newHealth > lastHealth)
            {
                healAnim.CrossFadeInFixedTime(healHash, .1f);
            }
            else if (newHealth < lastHealth)
            {
                damageAnim.CrossFadeInFixedTime(healHash, .1f);
            }
        }

        healthBar.maxValue = maxHealth;
        healthBar.value = newHealth;

        deathPanel.SetActive(newHealth <= 0);
        alivePanel.SetActive(newHealth > 0);
        healthText.text = newHealth.ToString();

        lastHealth = newHealth;

    }
    public void ShowPrompt(string promptName)
    {
        promptText.text = promptName;

        promptObject.SetActive(promptName != string.Empty);
    }
    public void UpdateBullets(int currentBullets, int bulletsLeft)
    {
        if (bulletsLeft == 0)
        {
            bulletsText.text = currentBullets.ToString();
        }
        else
        {
            if (currentBullets == -1)
            {
                bulletsText.text = string.Empty;
                return;
            }
            string bulletsLeftText = bulletsLeft > 999 ? "\u221E" : bulletsLeft.ToString();

            bulletsText.text = $"{currentBullets}/{bulletsLeftText}";
        }

        healthBar.gameObject.SetActive(true);
        bulletsText.gameObject.SetActive(true);
    }
    public void PlaySound(AudioClip clip)
    {
        aSource.PlayOneShot(clip);
    }
    public void StartBossfight(float currentHealth, float maxHealth)
    {
        inBossFight = true;

        bossRealHealthSlider.maxValue = maxHealth;
        bossLerpedHealthSlider.maxValue = maxHealth;
        bossLerpedHealthSlider.value = currentHealth;

        UpdateBossHealth(currentHealth);
        bossUI.alpha = 1;

        //bossFightMusic.Play();
        //SlowlyGainAudio(bossFightMusic, 0, .5f, 2f);
    }
    public static void SlowlyGainAudio(AudioSource aSource, float from, float to, float inSeconds)
    {
        Instance.StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            float timePassed = 0;
            while (timePassed < 1f)
            {
                timePassed += Time.deltaTime / inSeconds;

                aSource.volume = Mathf.Lerp(from, to, timePassed);

                yield return null;
            }
            if (to == 0f)
            {
                aSource.Stop();
            }
        }
    }
    public void StopBossfight()
    {
        inBossFight = false;
        bossUI.alpha = 0;

        //SlowlyGainAudio(bossFightMusic, .5f, 0, 1f);
    }
    public void UpdateBossHealth(float targetHealth)
    {
        bossRealHealthSlider.value = targetHealth;
    }
    public void ShowCountdown(int countdown)
    {
        countdownText.text = countdown == -1 ? string.Empty : countdown.ToString();
    }
    public void ShowBlackbars(bool state, float inSeconds = .2f)
    {
        if (blackBarRoutine != null)
        {
            StopCoroutine(blackBarRoutine);
        }

        blackBarRoutine = BlackBarMovement(state, inSeconds);

        StartCoroutine(blackBarRoutine);
    }
    private IEnumerator BlackBarMovement(bool state, float inSeconds)
    {
        float timePassed = 0f;

        float targetY = state ? -50 : 50;

        Vector2 top = topBlackBar.anchoredPosition;
        Vector2 bottom = bottomBlackBar.anchoredPosition;
        float startY = top.y;

        while (timePassed < 1f)
        {
            timePassed += Time.deltaTime / inSeconds;
            top.y = Mathf.Lerp(startY, targetY, timePassed);
            bottom.y = Mathf.Lerp(-startY, -targetY, timePassed);

            topBlackBar.anchoredPosition = top;
            bottomBlackBar.anchoredPosition = bottom;
            yield return null;
        }

        blackBarRoutine = null;
    }
    public void ShowWaitingToStart(bool state)
    {
        waitingToStart.gameObject.SetActive(state);
    }
    public void ShowWaitingForHost(bool state)
    {
        waitingForHost.gameObject.SetActive(state);
    }
    public void SetUltimateBar(int newValue, int maxValue)
    {
        bool ultiReady = newValue == maxValue;
        string ultiText = ultiReady ? "HAZIR" : $"%{newValue}";
        ultiReadyObj.SetActive(ultiReady);

        ultimateBar.value = (float)newValue / (float)maxValue;
        ultimateBarText.text = ultiText;
    }
    public void PlayButtonSound(ButtonSounds buttonSoundEnum)
    {
        PlaySound(buttonSounds[(int)buttonSoundEnum]);
    }
    public void UpdateGemAmount(int newValue)
    {
        gemsText.text = newValue.ToString();
    }
    public void UpdateBatteryText(int newValue)
    {
        batteryText.text = $"%{newValue}";
    }
    public void SetBuffState(BuffsNames buffName, bool state)
    {
        buffsUI[(int)buffName].SetActive(state);
    }
    public void StartNewCast(float timeTakes)
    {
        IsCasting = true;
        castMax = timeTakes;
        castTimer = timeTakes;
    }
    public void CancelCast()
    {
        IsCasting = false;
    }
    public void EndGame(bool didWin)
    {
        loseMenu.SetActive(!didWin);
        winMenu.SetActive(didWin);

        GameEndedUIActive = true;

        ClientUI.SetCursor(true);
    }
    public void UpdateGamePlayerUI(PlayerController ply, bool isDestroyed)
    {
        int actorNumber = ply.photonView.OwnerActorNr;

        gamePlayerUIs ??= new Dictionary<int, InGamePlayerUI>();

        if (gamePlayerUIs.TryGetValue(actorNumber, out InGamePlayerUI gameUi))
        {
            if (!isDestroyed)
            {
                gameUi.OnUpdateCustomProperties(ply);
            }
            else
            {
                Destroy(gameUi.gameObject);
                gamePlayerUIs.Remove(actorNumber);
            }
        }
        else
        {
            var spawned = Instantiate(gamePlayerUIPrefab, gamePlayerUIParent);
            spawned.Initialize(ply, ply.ClassHandler.PlayerClass, ply.Health, ply.ClassHandler.CurrentClass.PlayerHealth);
            gamePlayerUIs.Add(actorNumber, spawned);
        }
    }
    public void UpdateToxicState(bool state)
    {
        if (lastToxicState == state)
            return;

        toxicAnimator.CrossFadeInFixedTime(state ? toxicHash : toxicOutHash, .5f);
        lastToxicState = state;
    }
    public void OnNewWaveStarted(int overallWaveIndex)
    {
        waveAnimator.CrossFadeInFixedTime(newWaveAnimationHash, .25f);

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return newWaveWaiter;

            waveIndicator.DontSetAutomatically = false;
            waveIndicator.SetExtraText($"{overallWaveIndex + 1}/{WaveManager.WAVE_AMOUNT}");
        }

    }
    public void ShowExperiencePoint(int xp)
    {
        var expUi = PoolManager.Instance.ReuseObject<ExperienceUI>(experienceUIPrefab.gameObject);

        expUi.Initialize(xp);
    }
}

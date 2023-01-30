using System;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public bool IsNotification { get; private set; }
    [SerializeField] private TextMeshProUGUI topText;
    [SerializeField] private TextMeshProUGUI middleText;
    [Space]
    [SerializeField] private TextMeshProUGUI yesText;
    [SerializeField] private TextMeshProUGUI noText;
    [Space]
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject noButton;

    private Action OnClickedYes;
    private Action OnClickedNo;

    private bool haveNoButton;
    private float timer;

    //SPECIAL CASES
    private bool updateTimerArgText;
    private int timerArgIndex;

    private Language lng;
    private PopupTranslationScriptable trns;
    private string[] args;
    private int lastTimer;

    public void Initialize(PopupTranslationScriptable popupTranslation, Action OnClickedYes, Action OnClickedNo, float timer, string[] args)
    {
        this.args = args;
        this.trns = popupTranslation;
        this.lng = SaveSocket.CurrentSave.settings.language;

        this.IsNotification = popupTranslation.isNotification;
        this.topText.text = popupTranslation.titleText.GetTranslationOf(lng);
        this.middleText.text = popupTranslation.descriptionText.GetTranslationOfWithArgs(lng, args);

        this.OnClickedNo = OnClickedNo;
        this.OnClickedYes = OnClickedYes;

        this.yesText.text = popupTranslation.yesText.GetTranslationOf(lng);
        this.noText.text = popupTranslation.noText.GetTranslationOf(lng);

        this.haveNoButton = popupTranslation.haveNoButton;
        this.timer = timer;

        for (int i = 0; i < args.Length; i++)
        {
            try
            {
                if (int.Parse( args[i]) == timer)
                {
                    updateTimerArgText = true;
                    timerArgIndex = i;
                    break;
                }
            }
            catch (Exception)
            {
            }
        }

        yesButton.SetActive(popupTranslation.haveYesButton);
        noButton.SetActive(popupTranslation.haveNoButton);
        closeButton.SetActive(popupTranslation.haveCloseButton && haveNoButton);
    }
    private void Dispose()
    {
        ClientUI.PopupInstance.RemovePopup(this);
        Destroy(gameObject);
    }
    public void ClickYes_Btn()
    {
        OnClickedYes?.Invoke();
        Dispose();
    }
    public void ClickNo_Btn()
    {
        if (!haveNoButton)
        {
            ClickYes_Btn();
            return;
        }
        OnClickedNo?.Invoke();
        Dispose();
    }
    private void Update()
    {
        if (timer == -1)
            return;

        timer -= Time.deltaTime;

        if(updateTimerArgText && (int)timer != lastTimer)
        {
            args[timerArgIndex] = ((int)timer).ToString();
            this.middleText.text = trns.descriptionText.GetTranslationOfWithArgs(lng, args);
        }

        if (timer <= 0)
        {
            if (haveNoButton)
            {
                ClickNo_Btn();
            }
            else
            {
                ClickYes_Btn();
            }
            return;
        }
        lastTimer = (int)timer;
    }
}

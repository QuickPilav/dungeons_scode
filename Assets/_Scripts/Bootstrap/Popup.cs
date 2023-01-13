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

    public void Initialize(string topText, string middleText, string yesText, string noText, Action OnClickedYes, Action OnClickedNo,bool haveYesButton, bool haveNoButton, bool haveCloseButton, float timer, bool isNotification)
    {
        this.IsNotification = isNotification;
        this.topText.text = topText;
        this.middleText.text = middleText;

        this.OnClickedNo = OnClickedNo;
        this.OnClickedYes = OnClickedYes;

        this.yesText.text = yesText;
        this.noText.text = noText;

        this.haveNoButton = haveNoButton;
        this.timer = timer;


        yesButton.SetActive(haveYesButton);
        noButton.SetActive(haveNoButton);
        closeButton.SetActive(haveCloseButton && haveNoButton);
        
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

        if (timer <= 0)
        {
            if(haveNoButton)
            {
                ClickNo_Btn();
            }
            else
            {
                ClickYes_Btn();
            }
            return;
        }

    }


    private void Dispose()
    {
        ClientUI.PopupInstance.RemovePopup(this);
        Destroy(gameObject);
    }
}

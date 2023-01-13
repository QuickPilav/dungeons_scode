using RoboRyanTron.SearchableEnum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageText : MonoBehaviour
{
    protected TextMeshProUGUI text;
    [SerializeField,SearchableEnum] protected LanguageKeywords keyword;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        LanguageHandler.SubscribeLanguageText(this, keyword);
    }

    private void OnDestroy()
    {
        LanguageHandler.UnsubscribeLanguageText(this);
    }

    public virtual void SetText (string str)
    {
        text.text = str;
    }
}

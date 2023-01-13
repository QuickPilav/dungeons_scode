using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageTextExtra : LanguageText
{
    public bool DontSetAutomatically { get => dontSetAutomatically; set => dontSetAutomatically = value; }
    [SerializeField] private bool dontSetAutomatically;

    [SerializeField] private bool putExtraAtStart;
    private string extraText;
    private string lastStr;
    public override void SetText(string str)
    {
        this.lastStr = str;
        UpdateText();
    }

    public void SetExtraText (string extraText)
    {
        this.extraText = extraText;
        UpdateText();
    }

    private void UpdateText()
    {
        if (dontSetAutomatically)
            return;

        if(putExtraAtStart)
        {
            text.text = $"{extraText} {lastStr}";
            return;
        }
        text.text = $"{lastStr} {extraText}";
    }
}

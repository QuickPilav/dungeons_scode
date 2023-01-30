using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Language/New Popup Translation")]
public class PopupTranslationScriptable : ScriptableObject
{
    public TranslationScriptable titleText;
    public TranslationScriptable descriptionText;
    public TranslationScriptable yesText;
    public TranslationScriptable noText;

    public bool haveYesButton;
    public bool haveNoButton;
    public bool haveCloseButton;
    public bool isNotification;
}

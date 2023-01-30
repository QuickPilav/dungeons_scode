using UnityEngine;

[CreateAssetMenu(menuName = "Language/New Translation")]
public class TranslationScriptable : ScriptableObject
{
    [LabeledArray(typeof(Language)), TextArea]
    [SerializeField] private string[] translations = new string[System.Enum.GetNames(typeof(Language)).Length];

    public string GetTranslationOf(Language lng)
    {
        return translations[(int)lng];
    }
    public string GetTranslationOfWithArgs(Language lng, params string[] args)
    {
        return string.Format(translations[(int)lng], args);
    }
}

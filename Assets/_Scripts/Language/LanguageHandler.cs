using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LanguageHandler
{
    private static Optional<LanguageScriptable> currentScriptable;
    private static LanguageScriptable lastSelectedLanguage;

    private static Dictionary<LanguageText,LanguageKeywords> subscribedLanguageTexts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        subscribedLanguageTexts ??= new Dictionary<LanguageText, LanguageKeywords>();
        SaveSocket.OnSaveDataLoadedForTheFirstTime.SubscribeToEvent((s) =>
        {
            VisualizeLanguageChange(s.settings.language);
        });

    }

    public static void VisualizeLanguageChange (Language lang)
    {
        if(lastSelectedLanguage != null && lastSelectedLanguage.language == lang)
        {
            return;
        }
        subscribedLanguageTexts ??= new Dictionary<LanguageText, LanguageKeywords>();

        currentScriptable.Value = ResourceManager.GetLanguageScriptable(lang);

        foreach (var item in subscribedLanguageTexts)
        {
            item.Key.SetText(GetStringFromCurrentLanguage(item.Value));
        }
        lastSelectedLanguage = currentScriptable.Value;
    }

    public static string GetStringFromCurrentLanguage (LanguageKeywords keyword)
    {
        return currentScriptable.Value.words[(int)keyword];
    }

    public static void SubscribeLanguageText(LanguageText languageText, LanguageKeywords keyword)
    {
        subscribedLanguageTexts.Add(languageText, keyword);

        if(currentScriptable.Enabled)
        {
            languageText.SetText(GetStringFromCurrentLanguage(keyword));
        }
    }

    public static void UnsubscribeLanguageText(LanguageText languageText)
    {
        subscribedLanguageTexts.Remove(languageText);
    }
}


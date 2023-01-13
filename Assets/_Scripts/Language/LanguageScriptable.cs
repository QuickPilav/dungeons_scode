using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Language/New Language Wrapper")]
public class LanguageScriptable : ScriptableObject
{
    public Language language;

    [LabeledArray(typeof(LanguageKeywords))]
    public string[] words;
}

public enum LanguageKeywords
{
    //main menu
    menu_singleplayer = 0,
    menu_join,
    menu_createRoom,
    menu_characters,
    menu_settings,
    menu_quit,
    menu_back,
    menu_reset,

    //pause menu
    menu_pause_continue = 20,
    menu_pause_return_to_lobby,
    
    //settings menu
    menu_settings_controls = 30,
    menu_settings_graphics,
    menu_settings_audio,
    menu_settings_map,

    //SETTINGS
    menu_setting_sensitivity = 40,
    menu_setting_ads,
    menu_setting_auto_aim,

    menu_setting_language,
    menu_setting_resolution,
    menu_setting_fullscreen,

    menu_setting_volume,

    //characters menu
    menu_charui_buy = 70,
    menu_charui_not_enough_points,






















    //ingame

    ingame_wave = 200,


}
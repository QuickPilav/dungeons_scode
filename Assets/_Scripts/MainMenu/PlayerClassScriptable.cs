using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Classes/New Player Class")]
public class PlayerClassScriptable : ScriptableObject
{
    public PlayerClass playerClass;
    public string visualName;

    public TranslationScriptable descriptionScriptable;

    public int pointsCost;
}

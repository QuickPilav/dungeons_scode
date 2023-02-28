using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerClass
{
    classMami,
    classSissy,
    classIbo,
    classVlonderz,
    classDibaba,
    classAmy
}

[System.Serializable]
public class PlayerClassHandler
{
    public PlayerClass PlayerClass { get; private set; }
    public PlayerClassBase CurrentClass { get => currentClass; }

    public bool CanNotSetAbilityBar { get; set; }
    public bool IsPassiveAbilityActive { get; set; }
    public bool IsActiveAbilityActive { get; set; }

    public Renderer[] AllMeshRenderers => meshRenderers; 

    [SerializeField] private Renderer[] meshRenderers;
    [Space]
    [SerializeField] private PlayerClassMami classMami;
    [SerializeField] private PlayerClassSissy classSissy;
    [SerializeField] private PlayerClassIbo classIbo;
    [SerializeField] private PlayerClassVLonderz classVlonderz;
    [SerializeField] private PlayerClassDibaba classDibaba;
    [SerializeField] private PlayerClassAmy classAmy;

    public PlayerClassMami ClassMami => classMami;
    public PlayerClassSissy ClassSissy => classSissy;
    public PlayerClassIbo ClassIbo => classIbo;
    public PlayerClassVLonderz ClassVlonderz => classVlonderz;
    public PlayerClassDibaba ClassDibaba => classDibaba;
    public PlayerClassAmy ClassAmy => classAmy;

    public int AbilityBar
    {
        get => abilityBar;
        set
        {
            if (CanNotSetAbilityBar && value > abilityBar)
                return;

            value = Mathf.Clamp(value, 0, currentClass.AbilityBarNeededToFill);
            abilityBar = value;
            if (ply.photonView.IsMine)
            {
                InGameUI.Instance.SetUltimateBar(value, currentClass.AbilityBarNeededToFill);
            }
        }
    }

    private PlayerClassBase currentClass;
    private int abilityBar;
    private PlayerController ply;
    private readonly int invisibilityId = Shader.PropertyToID("_Invisibility");

    public void Initialize (PlayerController ply, PlayerClass plyClass)
    {
        this.ply = ply;

        this.PlayerClass = plyClass;

        currentClass = GetClass(plyClass);

        currentClass.Initialize(ply, this);

        Debug.Log($"started as {currentClass}");

        foreach (var item in meshRenderers)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in currentClass.CharacterRenderers)
        {
            item.gameObject.SetActive(true);
        }

        AbilityBar = 0;
    }

    public PlayerClassBase GetClass (PlayerClass plyClass)
    {
        return (PlayerClassBase)typeof(PlayerClassHandler).GetProperty(plyClass.ToString().FirstCharacterToUpper()).GetValue(this, null);
    }

    public void UpdateOwner (InputPayload input)
    {
        currentClass.OnStateUpdateOwner(input);
        if (IsPassiveAbilityActive)
        {
            currentClass.PassiveAbilityUpdate(input);
        }
        if (IsActiveAbilityActive)
        {
            currentClass.ActiveAbilityUpdate(input);
        }

        bool isAbilityBarFull = AbilityBar >= currentClass.AbilityBarNeededToFill;

#if UNITY_EDITOR

        isAbilityBarFull = true;

#endif

        if (input.ultimate && isAbilityBarFull)
        {
            currentClass.ActiveAbilityStart();
        }
    }

    public void Update (float dt)
    {
        currentClass.OnStateUpdateNormal();
    }

    public AudioClip GetSound (PlayerClassBase.CharacterSoundIndex reloading)
    {
        return currentClass.SoundsUsing[(int)reloading];
    }

    public void SetInvisibility (bool state)
    {
        ply.StartCoroutine(enumerator(1f, state ? 1f : 0f));

        IEnumerator enumerator (float timeItTakes, float target)
        {
            float current = 1 - target;

            float timePassed = 0f;
            while (timePassed < 1f)
            {
                foreach (var item in currentClass.CharacterRenderers)
                {
                    item.material.SetFloat(invisibilityId, Mathf.Lerp(current, target, timePassed));
                }

                timePassed += Time.deltaTime / timeItTakes;
                yield return null;
            }
        }
    }
}

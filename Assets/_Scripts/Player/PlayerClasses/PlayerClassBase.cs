using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerClassBase
{
    public enum CharacterSoundIndex 
    {
        NeedToHeal,
        Ping,
        NeedSupport,
        LowAmmo,
        Reloading,
        ULTI
    }

    [SerializeField] private int bulletPenetrationAdditive = 0;
    [SerializeField] private int playerHealth = 100;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int _abilityBarNeedToFill = 100;
    [Space]
    [SerializeField] private Renderer[] characterRenderers;
    [Space]
    [SerializeField] private AudioClip[] turkishSounds;
    [SerializeField] private AudioClip[] englishSounds;

    public int BulletPenetrationAdditive { get => bulletPenetrationAdditive; }
    public AudioClip[] SoundsUsing { get => soundsUsing; }
    public int AbilityBarNeededToFill { get => _abilityBarNeedToFill; }
    public int PlayerHealth { get => playerHealth; }
    public float MoveSpeed { get => moveSpeed; }
    public Renderer[] CharacterRenderers { get => characterRenderers; }

    [NonSerialized] protected PlayerController ply;
    [NonSerialized] protected PlayerClassHandler classHandler;
    private AudioClip[] soundsUsing;

    public virtual void Initialize (PlayerController ply, PlayerClassHandler classHandler)
    {
        this.ply = ply;
        this.classHandler = classHandler;

        SaveSocket.OnSettingsChanged.SubscribeToEvent(OnSettingChanged);

    }

    protected virtual void OnSettingChanged (SettingsSave obj)
    {
        switch (SaveSocket.CurrentSave.settings.language)
        {
            case Language.Turkish:
                soundsUsing = turkishSounds;
                break;
            case Language.English:
                soundsUsing = englishSounds;
                break;
        }
    }

    ~PlayerClassBase ()
    {
        SaveSocket.OnSettingsChanged.UnsubscribeToEvent(OnSettingChanged);
    }

    public abstract void OnStateUpdateOwner (InputPayload input);
    public abstract void OnStateUpdateNormal ();
    public abstract void OnStateExit ();
    public abstract void OnDrawGizmos ();

    /// <summary>Ayný zamanda dalgalarýn baþladýðý an...</summary>
    public abstract void PassiveAbilityStart ();
    public abstract void PassiveAbilityUpdate (InputPayload input);
    
    public abstract void ActiveAbilityStart ();
    public abstract void ActiveAbilityUpdate (InputPayload input);
    public virtual void ActiveAbilityEnd ()
    {
        classHandler.AbilityBar = 0;
    }

}

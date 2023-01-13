using System.Collections;
using UnityEngine;

[System.Serializable]
public class EnemyStateAnimation : EnemyStateBase
{
    [SerializeField] private Animator anim;

    [SerializeField] private string[] animationNames;
    private int[] animationNameHashes;

    [SerializeField] private string[] animationParameters;
    private int[] animationParametersHashes;

    public int GetAnimationHash (int index)
    {
        return animationNameHashes[index];
    }

    public int GetParameterHash (int index)
    {
        return animationParametersHashes[index];
    }

    public override void Initialize (EnemyAI ai)
    {
        base.Initialize(ai);

        animationNameHashes = new int[animationNames.Length];
        for (int i = 0; i < animationNames.Length; i++)
        {
            animationNameHashes[i] = Animator.StringToHash(animationNames[i]);
        }

        animationParametersHashes = new int[animationParameters.Length];
        for (int i = 0; i < animationParameters.Length; i++)
        {
            animationParametersHashes[i] = Animator.StringToHash(animationParameters[i]);
        }
    }

    public void Play (int animationHash, int layer = 0, float crossFade = .25f)
    {
        anim.CrossFadeInFixedTime(animationHash, crossFade, layer);
    }

    public IEnumerator Play (int animationHash,float timeTakes, int layer = 0, float crossFade = .25f)
    {
        anim.CrossFadeInFixedTime(animationHash, crossFade, layer);

        yield return new WaitForSeconds(timeTakes);
    }

    public void SetParameter (int parameterHash, object value)
    {
        if (value is float floatValue)
        {
            anim.SetFloat(parameterHash, floatValue);
        }
        else if (value is bool boolValue)
        {
            anim.SetBool(parameterHash, boolValue);
        }
        else if (value is int intValue)
        {
            anim.SetInteger(parameterHash, intValue);
        }
    }

    public override bool OnStateEnter ()
    {
        return true;
    }

    public override bool OnStateExit ()
    {
        return true;
    }

    public override void NonCurrentStateUpdate ()
    {
    }

    public override void OnStateUpdateNormal ()
    {
    }

    public override void OnStateUpdateLateNormal ()
    {
    }

    public override void OnDrawGizmos ()
    {
    }
}
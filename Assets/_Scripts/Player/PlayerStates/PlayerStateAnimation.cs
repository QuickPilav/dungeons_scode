using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStateAnimation : PlayerStateBase
{
    [SerializeField] private Animator anim;

    [SerializeField] private string[] animationNames;
    private int[] animationNameHashes;

    [SerializeField] private string[] animationParameters;
    private int[] animationParametersHashes;

    public Vector3 moveDirection;
    public float rotationSpeed = 5f;

    public int GetAnimationHash (int index)
    {
        return animationNameHashes[index];
    }

    public int GetParameterHash (int index)
    {
        return animationParametersHashes[index];
    }

    public override void Initialize (PlayerController ply)
    {
        base.Initialize(ply);

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

    public IEnumerator Play (int animationHash, float timeTakes, int layer = 0, float crossFade = .25f)
    {
        anim.CrossFadeInFixedTime(animationHash, crossFade, layer);
        if(timeTakes == 0f)
        {
            yield break;
        }

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

    public override void OnStateEnter ()
    {

    }

    public override void OnStateExit ()
    {
    }

    public override void OnStateUpdate (InputPayload input, Vector3 mousePos)
    {
        if (moveDirection == Vector3.zero)
            return;

        ply.Move(moveDirection);

        ply.transform.rotation = Quaternion.Slerp(ply.transform.rotation, Quaternion.LookRotation(moveDirection.normalized), Time.deltaTime * rotationSpeed);
    }

    public override void OnDrawGizmos ()
    {
    }
}

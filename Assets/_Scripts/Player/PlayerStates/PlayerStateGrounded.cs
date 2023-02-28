using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class PlayerStateGrounded : PlayerStateBase
{
    public bool IsRolling { get => rollRoutine.Enabled && ply.CurrentState == ply.AnimationState; }

    public bool CanMovePlayer { get => !IsGettingUp && !interaction.CanNotMove && !IsPlacingAxe; }
    public bool IsPlacingAxe { get; set; }

    public bool CanRotatePlayer { get => !IsSwingingAxe && !IsGettingUp && !interaction.CanNotMove; }
    public bool IsSwingingAxe { get; set; }
    public bool IsGettingUp { get; set; }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [Space]
    [SerializeField] private float rollSpeed = 7f;

    private PlayerInteraction interaction;

    private Optional<IEnumerator> rollRoutine;

    public const float ROLL_TIME = 1f;

    private float pingTimer;
    private float moveBuff;

    public override void Initialize(PlayerController ply)
    {
        base.Initialize(ply);

        interaction = ply.Interaction;
    }

    public override void OnStateEnter()
    {
    }

    public override void OnStateUpdate(InputPayload input, Vector3 mousePos)
    {
        HandleMovement(input, mousePos);
        MovesUpdate(input);
        interaction.Update(input);
    }

    public override void OnStateExit()
    {
        interaction.ForceStopHoldInteraction();
    }

    public override void OnDrawGizmos()
    {
    }

    private void MovesUpdate(InputPayload lastInput)
    {
        pingTimer += Time.deltaTime;

        if (lastInput.ping && pingTimer > 1f)
        {
            ply.Ping();
            pingTimer = 0f;
        }

        if (lastInput.roll && !rollRoutine.Enabled && CanMovePlayer && CanRotatePlayer)
        {
            rollRoutine.Value = Roll();
            ply.StartCoroutine(rollRoutine.Value);
        }
    }

    private IEnumerator Roll()
    {
        Vector3 normalizedVector = ply.NormalizedVelocity;
        if (normalizedVector == Vector3.zero)
        {
            normalizedVector = ply.transform.forward;
        }

        ply.OnRolled?.Invoke();

        ply.AnimationState.moveDirection = normalizedVector * rollSpeed;
        ply.AnimationState.rotationSpeed = 12f;
        ply.CurrentState = ply.AnimationState;

        ply.photonView.RPC(nameof(ply.RollRpc), Photon.Pun.RpcTarget.All);

        yield return ply.StartCoroutine(ply.AnimationState.Play(ply.AnimationState.GetAnimationHash((int)PlayerController.Player_Anims.Roll), ROLL_TIME));

        if (ply.CurrentState != ply.AnimationState)
        {
            rollRoutine.Value = null;
            yield break;
        }

        ply.CurrentState = ply.GroundedState;

        yield return new WaitForSeconds(2f);
        rollRoutine.Value = null;

    }

    private void HandleMovement(InputPayload lastInput, Vector3 mousePos)
    {
        Vector3 moveDir = CanMovePlayer ? new Vector3(lastInput.x, 0, lastInput.z).normalized : Vector3.zero;

        ply.NormalizedVelocity = moveDir;
        Vector3 finalMoveDir = moveDir * moveSpeed;

        finalMoveDir += finalMoveDir * moveBuff;

        if(!ply.Noclip)
            finalMoveDir.y = Physics.gravity.y;


        ply.Move(finalMoveDir);

        Vector3 dirTowardsMousePos = mousePos - ply.transform.position;
        dirTowardsMousePos.y = 0;
        dirTowardsMousePos = dirTowardsMousePos.normalized;

        if (dirTowardsMousePos != Vector3.zero && CanRotatePlayer)
        {
            ply.transform.rotation = Quaternion.Slerp(ply.transform.rotation, Quaternion.LookRotation(dirTowardsMousePos), Time.deltaTime * rotationSpeed);
        }
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public void SetMoveBuff(float moveBuff)
    {
        this.moveBuff = moveBuff;
    }
}

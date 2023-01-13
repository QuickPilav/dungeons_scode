using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemyStateWalking : EnemyStateBase
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float pathCheckRate = .1f;
    [SerializeField] private bool dontPathfind;
    [SerializeField] private bool moveOnlyForward;
    //Son noktadan ne kadar uzakta duracaðý
    [SerializeField] private float finalRadiusOffset = 3; 
    
    private float pathCheckTimer;

    public bool IsStopped { get => NormalizedSpeed == 0; }

    private int cornerIndex;
    private NavMeshAgent agent;
    private Transform target;

    private Vector3[] path;
    private Vector3[] zeroPath;
    private NavMeshPath navPath;

    private float jumpTimer;

    public Action OnReachedDestination;

    private float lastNormalizedSpeed;
    public float NormalizedSpeed { get; set; }
    public float NormalizedLerpedSpeed { get; private set; }
    public bool CanNotMove { get; set; }

    public override void Initialize (EnemyAI ai)
    {
        base.Initialize(ai);
        zeroPath = new Vector3[0];
        path = new Vector3[0];
        navPath = new NavMeshPath();
        agent = ai.GetComponent<NavMeshAgent>();

        agent.isStopped = true;
        agent.autoBraking = false;
        agent.autoRepath = false;
        agent.updatePosition = false;
        agent.updateRotation = false;
    }


    public override bool OnStateEnter ()
    {
        return true;
    }

    public override void OnStateUpdateLateNormal ()
    {
        agent.nextPosition = ai.transform.position;
        if (target != null)
        {
            pathCheckTimer += Time.deltaTime;
            if (pathCheckTimer > pathCheckRate)
            {
                if (TryFindPath(target.position, ref path))
                {
                    //yol bulduq
                }

                pathCheckTimer -= pathCheckRate;
            }

        }

        if (cornerIndex == 0)
            cornerIndex = 1;

        NormalizedSpeed = 0;
        if (path.Length >= 2 && cornerIndex < path.Length)
        {
            try
            {
                Vector3 nextPos = path[cornerIndex];
                Vector3 currentPos = ai.transform.position;

                Vector3 diff = nextPos - currentPos;
                diff.y = 0;

                float dist = diff.magnitude;

                ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * rotateSpeed);
                
                if(!CanNotMove)
                {
                    if(moveOnlyForward)
                    {
                        ai.Move(ai.transform.forward * moveSpeed);
                    }
                    else
                    {
                        ai.Move(diff.normalized * moveSpeed);
                    }
                }
                NormalizedSpeed = 1;

                float additiveRadius = cornerIndex == path.Length - 1 ? finalRadiusOffset : 0f;

                if (dist < agent.radius + additiveRadius)
                {
                    cornerIndex++;
                    if (cornerIndex >= path.Length)
                    {
                        OnReachedDestination?.Invoke();
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        if (lastNormalizedSpeed != NormalizedSpeed)
        {
            ai.photonView.RPC(nameof(ai.SetSpeedParameterRPC), RpcTarget.All, NormalizedSpeed);
        }

        lastNormalizedSpeed = NormalizedSpeed;

    }

    public override void OnStateUpdateNormal ()
    {
        NormalizedLerpedSpeed = Mathf.Lerp(NormalizedLerpedSpeed, NormalizedSpeed, Time.deltaTime * 25f);
    }

    public override bool OnStateExit ()
    {
        NormalizedSpeed = 0f;
        NormalizedLerpedSpeed = 0f;
        ai.photonView.RPC(nameof(ai.SetSpeedParameterRPC), RpcTarget.All, NormalizedSpeed);
        lastNormalizedSpeed = 0f;
        return true;
    }

    public override void NonCurrentStateUpdate ()
    {
    }

    public void SetTarget (Transform target)
    {
        this.target = target;
    }

    public bool SetTarget (Vector3 pos)
    {
        target = null;
        return TryFindPath(pos, ref path);
    }

    public void SetTarget (Vector3[] newPath)
    {
        target = null;

        if (newPath[0] != ai.transform.position)
        {
            path = new Vector3[newPath.Length + 1];

            path[0] = ai.transform.position;

            for (int i = 1; i < newPath.Length + 1; i++)
            {
                path[i] = newPath[i - 1];
            }
        }
        else
        {
            path = newPath;
        }
        
        cornerIndex = 0;
    }

    private bool TryFindPath (Vector3 pos, ref Vector3[] outPath)
    {
        if (dontPathfind)
            return false;

        float dist = Vector3.Distance(pos, ai.transform.position);

        if (dist < agent.radius + .6f)
        {
            outPath = zeroPath;
            return false;
        }

        if (agent.CalculatePath(pos, navPath))
        {
            outPath = navPath.corners;
            cornerIndex = 0;
            return true;
        }

        return false;
    }

    public override void OnDrawGizmos ()
    {
        if (path == null || path.Length == 0)
            return;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 pos = path[i] + Vector3.up * .5f;
            Vector3 nextPos = path[i + 1] + Vector3.up * .5f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pos, .5f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, nextPos);

        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(path[^1], .5f);
    }

}
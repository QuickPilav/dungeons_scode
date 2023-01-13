using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using RoboRyanTron.QuickButtons;
#endif

public class RoomTrigger : MonoBehaviour
{
    private static readonly int darknessId = Shader.PropertyToID("_Darkness");

#if UNITY_EDITOR
    public QuickButton SwitchTriggers = new QuickButton(nameof(Switch));

    private async void Switch()
    {
        RoomTrigger[] lightRoomsOnEnterCopied = new RoomTrigger[lightRoomsOnEnter.Length];
        Array.Copy(lightRoomsOnEnter, lightRoomsOnEnterCopied, lightRoomsOnEnter.Length);

        lightRoomsOnEnter = darkRoomsOnEnter;
        darkRoomsOnEnter = lightRoomsOnEnterCopied;

        Selection.activeGameObject = null;

        await System.Threading.Tasks.Task.Delay(1);

        Selection.activeGameObject = gameObject;
    }
#endif

    [SerializeField] private bool lightSelfOnEnter;
    [SerializeField] private RoomTrigger[] lightRoomsOnEnter;
    [SerializeField] private RoomTrigger[] darkRoomsOnEnter;
    private MeshRenderer mRenderer;
    private BoxCollider col;
    private void Awake()
    {
        mRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        col = GetComponent<BoxCollider>();
    }

    private bool isDark = true;

    public void GoDark()
    {
        if (isDark)
            return;

        isDark = true;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = SetDarkness(1, 0, 1f);
        StartCoroutine(currentRoutine);
    }

    public void GoLight()
    {
        if (!isDark)
            return;

        isDark = false;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = SetDarkness(0, 1, 1f);
        StartCoroutine(currentRoutine);
    }

    IEnumerator currentRoutine;

    private IEnumerator SetDarkness(float from, float to, float inSeconds)
    {
        float timePassed = 0;
        while (timePassed < 1)
        {
            mRenderer.material.SetFloat(darknessId, Mathf.Lerp(from, to, timePassed));
            timePassed += Time.deltaTime / inSeconds;
            yield return null;
        }
        currentRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out IDamagable dmg) || !dmg.IsPlayer || (dmg as PlayerController) != PlayerController.ClientInstance.Value)
        {
            return;
        }

        if (lightSelfOnEnter)
        {
            GoLight();
        }

        foreach (var item in lightRoomsOnEnter)
        {
            item.GoLight();
        }
        foreach (var item in darkRoomsOnEnter)
        {
            item.GoDark();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (col == null)
            OnEnable();


        if (lightSelfOnEnter)
        {
            DrawGizmoOfBoxCollider(col, Color.cyan);

            Handles.Label(transform.position, gameObject.name);
        }
        else
        {
            DrawGizmoOfBoxCollider(col, Color.green);
        }
    }

    private void OnDrawGizmosSelected()
    {
        ColorUtility.TryParseHtmlString("#eb9e04", out Color lightEnter);
        ColorUtility.TryParseHtmlString("#593b00", out Color lightExit);
        ColorUtility.TryParseHtmlString("#280055", out Color darkExit);
        ColorUtility.TryParseHtmlString("#470097", out Color darkEnter);

        foreach (var item in lightRoomsOnEnter)
        {
            if (item == null)
                continue;

            DrawGizmoOfBoxCollider(item.col, lightEnter);

            Handles.color = lightExit;
            Handles.DrawLine(item.transform.position, transform.position, .1f);
        }

        foreach (var item in darkRoomsOnEnter)
        {
            if (item == null)
                continue;

            DrawGizmoOfBoxCollider(item.col, darkEnter);

            Handles.color = darkExit;
            Handles.DrawLine(item.transform.position, transform.position, .1f);
        }
    }

    public static void DrawGizmoOfBoxCollider(BoxCollider boxCollider, Color col)
    {
        Gizmos.color = col;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCollider.transform.position, boxCollider.transform.rotation, boxCollider.transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
    }
#endif
}

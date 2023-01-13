using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputPayload
{
    public int x;
    public int z;

    public int itemSwitchRequest;

    public Vector2 mousePosition;

    public bool fire;
    public bool zoom;
    public bool lastZoom;
    public bool reload;

    public bool ultimate;

    public bool roll;
    public bool ping;
    public bool interaction;

    public bool drop;
}

[System.Serializable]
public class PlayerInput
{
    private float sensitivity = 1f;

    public float Sensitivity { get => sensitivity; set => sensitivity = value; }


    private bool toggleAds = false;

    public bool ToggleAds { get => toggleAds; set => toggleAds = value; }

    private InputPayload lastInput;

    public InputPayload Get (bool inControl, bool inMouseControl)
    {
        lastInput.lastZoom = lastInput.zoom;
        if(!inControl)
        {
            lastInput.x = 0;
            lastInput.z = 0;

            lastInput.itemSwitchRequest = -1;

            lastInput.mousePosition = Vector2.zero;

            lastInput.fire = false;
            lastInput.zoom = false;
            lastInput.reload = false;

            lastInput.roll = false;
            lastInput.interaction = false;

            lastInput.ping = false;

            lastInput.drop = false;
            lastInput.ultimate = false;

            return lastInput;
        }

        lastInput.itemSwitchRequest = -1;

        for (int index = (int)KeyCode.Alpha0; index < (int)KeyCode.Alpha9; ++index)
        {
            if (Input.GetKey((KeyCode)index))
            {
                lastInput.itemSwitchRequest = index - ((int)KeyCode.Alpha0) - 1;
            }
        }

        lastInput.x = (int)Input.GetAxisRaw("Horizontal");
        lastInput.z = (int)Input.GetAxisRaw("Vertical");

        
        lastInput.reload = Input.GetButton("Reload");

        lastInput.roll = Input.GetButtonDown("Roll");
        lastInput.interaction = Input.GetButton("Interaction");

        lastInput.ping = Input.GetButtonDown("Ping");

        lastInput.drop = Input.GetButtonDown("Drop");

        lastInput.ultimate = Input.GetButtonDown("Ultimate");


        if (inMouseControl)
        {
            lastInput.fire = Input.GetButton("Fire1");

            if (toggleAds)
            {
                if (Input.GetButtonDown("Fire2"))
                    lastInput.zoom = !lastInput.zoom;
            }
            else
            {
                lastInput.zoom = Input.GetButton("Fire2");
            }


            float currentMousePosX = Input.GetAxisRaw("Mouse X") * sensitivity;
            float currentMousePosY = Input.GetAxisRaw("Mouse Y") * sensitivity;

            lastInput.mousePosition.x += currentMousePosX;
            lastInput.mousePosition.y += currentMousePosY;

            lastInput.mousePosition = Vector3.ClampMagnitude(lastInput.mousePosition, 10f);
        }
        else
        {
            lastInput.zoom = false;
            lastInput.fire = false;
        }
        
        return lastInput;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingletonBehaviour<InputManager>
{
    public float deadzone = 0.25f;

    public float CheckAxis(PlayerInput player, string type = "Horizontal_")
    {
        Vector2 stickInput = new Vector2(Input.GetAxis("Horizontal_" + player), Input.GetAxis("Vertical_" + player));
        if (stickInput.magnitude < deadzone)
            stickInput = Vector2.zero;
        else
            stickInput = stickInput.normalized * ((stickInput.magnitude - deadzone) / (1 - deadzone));
        // float result = Input.GetAxis(type + player);

        return stickInput.x;
    }

    public bool CheckInput(PlayerInput player, ControllerInput input, MethodInput method)
    {
        bool result = false;
        switch (method)
        {
            case MethodInput.Down:
                result = Input.GetButtonDown(input.ToString() + "_" + player);
                break;
            case MethodInput.Up:
                result = Input.GetButtonUp(input.ToString() + "_" + player);
                break;
            case MethodInput.Hold:
                result = Input.GetButton(input.ToString() + "_" + player);
                break;
        }

        return result;
    }
}

public enum PlayerInput
{
    P1,
    P2,
    P3,
    P4
}

public enum ControllerInput
{
    Jump,
    Interact,
    Hug,
}

public enum MethodInput
{
    Up,
    Down,
    Hold
}
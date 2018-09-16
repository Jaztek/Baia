
using System.Collections.Generic;
using UnityEngine;

public interface ICrossPlatformInputController
{
    List<IInputData> GetInputs();
}

public enum InputDataState
{
    PRESSED, HOLD, RELEASED
};

public enum InputButton
{
    LEFT, RIGHT, JUMP, ROLL
}

public interface IInputData
{
}

public class ButtonInputData : IInputData
{
    public readonly InputButton button;
    public readonly InputDataState state;

    public ButtonInputData(InputButton button, InputDataState state)
    {
        this.button = button;
        this.state = state;
    }
}

public enum MouseButton
{
    LEFT, MIDDLE, RIGHT
}

public class MouseInputData : IInputData
{
    public readonly float startTime;
    public readonly int device;

    public InputDataState state;
    public float duration = -1f;

    public Vector2 currentPosition;
    public readonly Vector2 pressedPosition;
    public Vector2 releasedPosition;

    public GameObject GUIElement;

    static Dictionary<int, MouseInputData> idxToMouseData = new Dictionary<int, MouseInputData>();

    MouseInputData(int device, InputDataState state, Vector3 position)
    {
        this.device = device;
        pressedPosition = position;
        startTime = Time.time;
        GUIElement = GUIUtils.GetGUIObjectAtPosition(position);
    }

    public static MouseInputData Get(int device, InputDataState state, Vector3 position)
    {
        MouseInputData mouseInputData;
        if (state == InputDataState.PRESSED || !idxToMouseData.ContainsKey(device))
        {
            mouseInputData = new MouseInputData(device, state, position);
            idxToMouseData[device] = mouseInputData;
        }
        else
        {
            mouseInputData = idxToMouseData[device];
            mouseInputData.duration = Time.time - mouseInputData.startTime;
        }

        mouseInputData.state = state;

        if (state == InputDataState.RELEASED)
        {
            mouseInputData.releasedPosition = position;
            idxToMouseData.Remove(device);
        }
        mouseInputData.currentPosition = position;
        return mouseInputData;
    }
}
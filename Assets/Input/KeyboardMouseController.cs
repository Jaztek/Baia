using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardMouseController : ICrossPlatformInputController
{
    public List<IInputData> GetInputs()
    {
        var data = new List<IInputData>();

        if (Input.GetKey(KeyCode.RightArrow))
        {
            data.Add(new ButtonInputData(InputButton.RIGHT, InputDataState.HOLD));
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            data.Add(new ButtonInputData(InputButton.LEFT, InputDataState.HOLD));
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            data.Add(new ButtonInputData(InputButton.ROLL, InputDataState.PRESSED));
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            data.Add(new ButtonInputData(InputButton.JUMP, InputDataState.PRESSED));
        }
        else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space))
        {
            data.Add(new ButtonInputData(InputButton.JUMP, InputDataState.HOLD));
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space))
        {
            data.Add(new ButtonInputData(InputButton.JUMP, InputDataState.RELEASED));
        }

        if (Input.GetMouseButtonDown(0))
        {
            data.Add(MouseInputData.Get(0, InputDataState.PRESSED, Input.mousePosition));
        }
        else if (Input.GetMouseButton(0))
        {
            data.Add(MouseInputData.Get(0, InputDataState.HOLD, Input.mousePosition));
        }
        else if (Input.GetMouseButtonUp(0))
        {
            data.Add(MouseInputData.Get(0, InputDataState.RELEASED, Input.mousePosition));
        }

        return data;
    }
}

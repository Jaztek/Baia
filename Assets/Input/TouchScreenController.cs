using System.Collections.Generic;
using UnityEngine;

public class TouchScreenController : ICrossPlatformInputController
{
    public List<IInputData> GetInputs()
    {
        var data = new List<IInputData>();

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            InputDataState inputState;

            if (touch.phase == TouchPhase.Began)
            {
                inputState = InputDataState.PRESSED;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                inputState = InputDataState.RELEASED;
            }
            else
            {
                inputState = InputDataState.HOLD;
            }

            data.Add(MouseInputData.Get(i, inputState, touch.position));
        }
        return data;
    }
}

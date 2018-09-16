using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public float movementRadiusOffset = 100f;
    public float longTouchDuration = 0.2f;
    public float stationaryDeltaSqr = 62500f;

    Player player;
    CharacterController2D characterController;
    ICrossPlatformInputController inputController;

    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        player = p.GetComponent<Player>();
        characterController = p.GetComponent<CharacterController2D>();

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        inputController = new KeyboardMouseController();
#elif UNITY_ANDROID || UNITY_IOS || UNITY_WP_8_1
        inputController = new TouchScreenController();
#endif
    }

    void Update()
    {
        List<IInputData> data = inputController.GetInputs();

        IEnumerable<ButtonInputData> buttonData = data.OfType<ButtonInputData>();
        IEnumerable<MouseInputData> mouseData = data.OfType<MouseInputData>();

        HandleButtonsData(buttonData);
        HandleMouseDevicesData(mouseData);
    }

    void HandleButtonsData(IEnumerable<ButtonInputData> buttonData)
    {
        foreach (ButtonInputData i in buttonData)
        {
            if (i.button == InputButton.LEFT)
            {
                MoveLeftAction(i.state);
            }
            else if (i.button == InputButton.RIGHT)
            {
                MoveRightAction(i.state);
            }
            else if (i.button == InputButton.JUMP)
            {
                JumpAction(i.state);
            }
            else if (i.button == InputButton.ROLL)
            {
                AttackAction(i.state);
            }
        }
    }

    Dictionary<int, GameObject> inputDeviceToGUIElement = new Dictionary<int, GameObject>();

    void HandleMouseDevicesData(IEnumerable<MouseInputData> mouseDevicesData)
    {
        foreach (MouseInputData m in mouseDevicesData)
        {
            // El evento comenzó en un GUIElement, por lo que tiene tratamiento especial
            if (m.GUIElement != null)
            {
                HandleGUIEvent(m);
                continue;
            }

            if (m.state == InputDataState.PRESSED)
            {
                HandleMouseButtonPressed(m);
            }
            else if (m.state == InputDataState.HOLD)
            {
                HandleMouseButtonHold(m);
            }
            else if (m.state == InputDataState.RELEASED)
            {
                HandleMouseButtonReleased(m);
            }
        }
    }

    void HandleMouseButtonPressed(MouseInputData mouseData)
    {
        return;
    }

    void HandleMouseButtonHold(MouseInputData mouseData)
    {
        player.OnUsingItemHold(mouseData.device, mouseData.currentPosition);

        Vector2 offset = mouseData.currentPosition - mouseData.pressedPosition;

        float duration = mouseData.duration;

        if (offset.sqrMagnitude < stationaryDeltaSqr)
        {
            // Si estamos por debajo de un cierto umbral (para que no registre un gesto rápido como preciso)
            if (duration > longTouchDuration)
            {
                // Si se trata de un touch de larga duración activamos el item 1  
                if (player.IsActive(1)) { return; }
                player.OnUsingItemStart(1, mouseData.device, mouseData.currentPosition);
            }
        }
        else
        {
            if (player.IsActive(0)) { return; }
            // En caso de que superemos el umbral se considerará que está usándose el ítem 0
            player.OnUsingItemStart(0, mouseData.device, mouseData.currentPosition);
        }
    }

    void HandleMouseButtonReleased(MouseInputData mouseData)
    {
        player.OnUsingItemStop(mouseData.device);
    }

    void HandleGUIEvent(MouseInputData data)
    {
        GameObject obj = data.GUIElement;

        if (obj.CompareTag("UIMovement"))
        {
            HandleMouseMovement(data);
        }
        else if (obj.CompareTag("UIButtonJump"))
        {
            JumpAction(data.state);
        }
        else if (obj.CompareTag("UIButtonAttack"))
        {
            AttackAction(data.state);
        }
    }

    void HandleMouseMovement(MouseInputData data)
    {
        if (data.state == InputDataState.RELEASED)
        {
            GameObject guiElement = data.GUIElement;
            MoveButton mb = guiElement.GetComponent<MoveButton>();
            mb.HideMovementHelp();
            return;
        }

        if (data.state == InputDataState.PRESSED)
        {
            GameObject guiElement = data.GUIElement;
            MoveButton mb = guiElement.GetComponent<MoveButton>();
            mb.ShowMovementHelp(data.pressedPosition.x, movementRadiusOffset);
        }

        Vector2 start = data.pressedPosition;
        Vector2 current = data.currentPosition;

        Vector2 diff = current - start;

        float absDiffX = Mathf.Abs(diff.x);
        float absDiffY = Mathf.Abs(diff.y);

        // entra aqui si es un movimiento en x
        if (absDiffX > absDiffY && absDiffX > movementRadiusOffset)
        {
            if (diff.x > 0)
            {
                MoveRightAction(data.state);
            }
            else
            {
                MoveLeftAction(data.state);
            }
        }
    }

    void MoveLeftAction(InputDataState inputState)
    {
        if (inputState == InputDataState.RELEASED) return;

        characterController.Request(ControllerAction.MOVE_LEFT);
    }

    void MoveRightAction(InputDataState inputState)
    {
        if (inputState == InputDataState.RELEASED) return;

        characterController.Request(ControllerAction.MOVE_RIGHT);
    }

    void AttackAction(InputDataState inputState)
    {
        if (inputState == InputDataState.PRESSED)
        {
            // TODO Attack
            characterController.Request(ControllerAction.ROLL);
        }
    }

    void JumpAction(InputDataState inputState)
    {
        if (inputState == InputDataState.PRESSED)
        {
            characterController.Request(ControllerAction.JUMP);
        }
        else if (inputState == InputDataState.HOLD)
        {
            characterController.Request(ControllerAction.JETPACK);
        }
        else if (inputState == InputDataState.RELEASED)
        {
            characterController.Request(ControllerAction.CANCEL_JUMP);
        }
    }
}

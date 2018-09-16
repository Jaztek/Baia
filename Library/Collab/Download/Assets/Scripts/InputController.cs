using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    public Button btLeft;
    public Button btRight;
    public Button btUp;
    public Button btDown;
    public LayerMask UIMask;

    private CharacterController2D playerController;

    private int normalizedHorizontalSpeed;

    void Awake()
    {
        playerController = GetComponent<CharacterController2D>();
    }

    void Start()
    {
        Assert.IsNotNull(playerController);
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WP_8_1
        HandleTouchScreen();
        if (playerController.CanClimb)
        {
            btUp.gameObject.SetActive(true);
            btDown.gameObject.SetActive(true);
        }
        else
        {
            btUp.gameObject.SetActive(false);
            btDown.gameObject.SetActive(false);
        }
#else
        HandleKeyboard();
#endif
    }

    void HandleTouchScreen()
    {
        normalizedHorizontalSpeed = 0;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {

                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = touch.position;

                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                if (raycastResults.Count > 0)
                {
                    GameObject result = raycastResults[0].gameObject;
                    if (result == btLeft.gameObject)
                    {
                        normalizedHorizontalSpeed = -1;
                    }
                    else if (result == btRight.gameObject)
                    {
                        normalizedHorizontalSpeed = 1;
                    }
                    else if (playerController.CanClimb)
                    {
                        if (result == btUp.gameObject)
                        {
                            playerController.ClimbUp();
                        }
                        else if (result == btDown.gameObject)
                        {
                            playerController.ClimbDown();
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Began)
                {
                    if (playerController.CanClimb)
                    {
                        playerController.WallJump(normalizedHorizontalSpeed);
                    }
                    else if (playerController.CanJump)
                    {
                        playerController.Jump();
                    }
                }
            }
        }

        playerController.MoveHorizontally(normalizedHorizontalSpeed);
    }

    void HandleKeyboard()
    {

        if (Input.GetKey(KeyCode.RightArrow))
        {
            normalizedHorizontalSpeed = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            normalizedHorizontalSpeed = -1;
        }
        else
        {
            normalizedHorizontalSpeed = 0;
        }

        playerController.MoveHorizontally(normalizedHorizontalSpeed);

        if (playerController.CanClimb)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                playerController.ClimbUp();
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                playerController.ClimbDown();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playerController.CanClimb)
            {
                playerController.WallJump(normalizedHorizontalSpeed);
            }
            else if (playerController.CanJump)
            {
                playerController.Jump();
            }
        }
    }

    void HandleMouse()
    {
        normalizedHorizontalSpeed = 0;

        if (Input.GetMouseButton(0))
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                GameObject result = raycastResults[0].gameObject;
                if (result == btLeft.gameObject)
                {
                    normalizedHorizontalSpeed = -1;
                }
                else if (result == btRight.gameObject)
                {
                    normalizedHorizontalSpeed = 1;
                }
            }
        }

        playerController.MoveHorizontally(normalizedHorizontalSpeed);
    }
}

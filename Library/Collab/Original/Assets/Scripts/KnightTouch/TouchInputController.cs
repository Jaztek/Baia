using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class TouchInputController : MonoBehaviour
{

    //variables de pruebas, borrables:

    Vector3 posicionInicialMouse;


    private CharacterController2D playerController;
    private int normalizedHorizontalSpeed;
    private Dictionary<int, TouchClazz> touchIdTopressedPosition;
    private Animator animator;

    private GameController gameController;
    public int touchCountKey = 0;
    public int offsetTouch = 30;
    public int delaySlowTime = 17;

    public GameObject uiTouch;
    public LayerMask blockingLayer;
    public float dashForce = 20;

    public Canvas canvas;
    public Button padMove;
    public Button padJump;
    public Button padAtack;

    //Log del pardillo
    public Text infoEnded;
    public Text infoTime;
    public Text infoError;

    public enum EnumTouch { none, horizontalMove, verticalMove, oneTouch, timeTouch, dashTouch }

    public class TouchClazz
    {
        private int fingerId;
        private EnumTouch move;
        Vector2 position;
        private int touchCount = 0;
        GameObject uiTouch;
        GameObject touchLine;

        public TouchClazz(int fingerId, EnumTouch mov, Vector2 posit)
        {
            this.fingerId = fingerId;
            move = mov;
            position = posit;
        }

        public int getFingerId()
        {
            return fingerId;
        }

        public void setMove(EnumTouch mov)
        {
            move = mov;
        }
        public EnumTouch getMove()
        {
            return move;
        }
        public Vector2 getPosition()
        {
            return position;
        }
        public void setTouchCount(int count)
        {
            touchCount = count;
        }
        public int getTouchCount()
        {
            return touchCount;
        }

        public void setUiTouch(GameObject uiTouch)
        {
            this.uiTouch = uiTouch;
        }
        public GameObject getUiTouch()
        {
            return uiTouch;
        }

        public void setTouchLine(GameObject touchLine)
        {
            this.touchLine = touchLine;
        }
        public GameObject getTouchLine()
        {
            return touchLine;
        }

        public void destroyUiTouch()
        {
            GameObject.Destroy(uiTouch);
        }

        public bool isValid(EnumTouch moveToValid)
        {
            if (move.Equals(EnumTouch.none) || move.Equals(moveToValid))
            {
                return true;
            }
            return false;
        }
    }

    void Awake()
    {
        gameController = GetComponent<GameController>();
        playerController = GetComponent<CharacterController2D>();
    }

    void Start()
    {
        Assert.IsNotNull(playerController);

        //TODO sacar esto a un delegado
        animator = GetComponent<Animator>();

#if UNITY_EDITOR
        touchIdTopressedPosition = new Dictionary<int, TouchClazz>(1);
#elif UNITY_ANDROID
        touchIdTopressedPosition = new Dictionary<int, TouchClazz>(5);
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleKeyboard();
        HandleMouse();
#elif UNITY_ANDROID
        HandleTouchScreen();
#endif

    }

    void HandleTouchScreen()
    {
        normalizedHorizontalSpeed = 0;

        infoTime.text = "isTimeSlow: " + gameController.isTimeSlowedDown;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // began
            if (touch.phase == TouchPhase.Began)
            {
                TouchClazz touchInz = new TouchClazz(touch.fingerId, EnumTouch.none, touch.position);



                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = touch.position;

                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                if (raycastResults.Count > 0)
                {
                    GameObject result = raycastResults[0].gameObject;
                    if (result == padMove.gameObject)
                    {
                        touchInz.setMove(EnumTouch.horizontalMove);
                    }
                    else if (result == padJump.gameObject)
                    {
                        if (playerController.CanJump)
                        {
                            touchInz.setMove(EnumTouch.verticalMove);
                            playerController.Jump();
                        }
                    }
                    else if (result == padAtack.gameObject)
                    {
                        touchInz.setMove(EnumTouch.oneTouch);
                        animator.SetTrigger("triggerFrontAttack");
                    }
                }

                touchIdTopressedPosition.Add(touch.fingerId, touchInz);
                touchIdTopressedPosition[touch.fingerId].setUiTouch(DrawCircle(touch.position));

            }

            //middle

            if (touchIdTopressedPosition.ContainsKey(touch.fingerId))
            {
                TouchClazz touchInz = touchIdTopressedPosition[touch.fingerId];

                Vector2 mouseMovement = touch.position - touchInz.getPosition();

                DrawTouchLine(touch.fingerId, touch.position);

                if (!gameController.isTimeSlowedDown)
                {
                    if (touchInz.isValid(EnumTouch.dashTouch))
                    {                       
                        touchInz.setMove(EnumTouch.dashTouch);
                    }
                    if (touchInz.isValid(EnumTouch.horizontalMove))
                    {
                        if (Mathf.Abs(mouseMovement.y) < Mathf.Abs(mouseMovement.x) && Mathf.Abs(mouseMovement.x) > offsetTouch)
                        {
                            // entra aqui si es un movimiento en x

                            horizontalMove(touchInz, mouseMovement);

                        }
                        else if (Mathf.Abs(mouseMovement.y) < Mathf.Abs(mouseMovement.x) && Mathf.Abs(mouseMovement.x) < offsetTouch)
                        {
                            // entra aqui si es un movimiento en x

                            playerController.MoveHorizontally(0);

                        }
                    }

                   
                    else if (Mathf.Abs(mouseMovement.y) > Mathf.Abs(mouseMovement.x) && Mathf.Abs(mouseMovement.y) > offsetTouch
                        && touchInz.isValid(EnumTouch.verticalMove))
                    {
                        //entra aqui si el movimiento es en y
                        verticalMove(touch, touchInz, mouseMovement);

                    }
                    else if (Mathf.Abs(mouseMovement.y) < offsetTouch && Mathf.Abs(mouseMovement.x) < offsetTouch &&
                        touchInz.isValid(EnumTouch.none))
                    {
                        touchProlongado(touchInz);

                    }
                }
                else
                {
                    // estamos en slowIime
                    if (touchInz.isValid(EnumTouch.dashTouch))
                    {
                        infoEnded.text = "setea" + EnumTouch.dashTouch;
                        touchInz.setMove(EnumTouch.dashTouch);
                    }
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    infoEnded.text = touchInz.getMove().ToString();


                    if (touchInz.getMove().Equals(EnumTouch.dashTouch))
                    {
                        touchDash(touch, touchInz);
                    }
                    /*
                    else if (Mathf.Abs(mouseMovement.y) < offsetTouch && Mathf.Abs(mouseMovement.x) < offsetTouch &&
                         !touchInz.getMove().Equals(EnumTouch.timeTouch))
                    {
                        animator.SetTrigger("triggerFrontAttack");
                    }
                    */
                    else if (touchInz.getMove().Equals(EnumTouch.timeTouch) && gameController.isTimeSlowedDown)
                    {
                        gameController.SwapTimeScale();
                    }
                    else if (touchInz.getMove().Equals(EnumTouch.horizontalMove))
                    {
                        playerController.MoveHorizontally(0);
                    }
                    removeTouch(touch.fingerId);
                }
            }


        }
        if (Input.touchCount <= 0)
        {
            playerController.MoveHorizontally(0);
        }

        if (!playerController.CanJumpAttack)
        {
            animator.SetBool("boolJumpAttack", false);
        }
    }

    private Touch touchDash(Touch touch, TouchClazz touchInz)
    {
        try
        {
            Vector2 vectorMovimiento = touch.position - touchInz.getPosition();
            Vector3 normalizado = vectorMovimiento.normalized;

            RaycastHit2D hit = Physics2D.Linecast(transform.position, transform.position + normalizado * 5, blockingLayer);
            if (hit)
            {
                // Vector3 posicionFinal = new Vector3(hit.point.x, hit.point.y, 0);
                // vectorMovimiento = posicionFinal - transform.position;
                //  normalizado = vectorMovimiento.normalized;
            }
            Vector3 vectorIniFinal = transform.position + normalizado * 5;

            DrawDashLine(transform.position, vectorIniFinal, 0.2f);
            this.gameObject.transform.Translate(normalizado * 6);

            playerController.MoveHorizontally(0);
            playerController.SetVerticalForce(0);
            // playerController.AddForce(normalizado.x * 10, normalizado.y * 10);
            if (gameController.isTimeSlowedDown)
            {
                // gameController.SwapTimeScale();
            }
        }
        catch (Exception e)
        {
            infoError.text = "Error" + e.Message;

        }

        return touch;
    }

    private void touchProlongado(TouchClazz touchInz)
    {
        touchInz.setTouchCount(touchInz.getTouchCount() + 1);
        // estamos con el dedo pretado en la posicion inicial;
        if (touchInz.getTouchCount() > delaySlowTime && !gameController.isTimeSlowedDown)
        {
            gameController.SwapTimeScale();
            touchInz.setMove(EnumTouch.timeTouch);
            touchInz.setTouchCount(0);
        }
    }

    private void verticalMove(Touch touch, TouchClazz touchInz, Vector2 mouseMovement)
    {
        infoEnded.text = EnumTouch.verticalMove.ToString() + " " + playerController.CanJump;
        if (mouseMovement.y > 0)
        {
            // el movimiento es para arriba.
            if (playerController.CanClimb)
            {
                bool verticalDir = true;
                playerController.ClimbUpDown(verticalDir);
                /*  
                Este es el salto/ dejarse caer de la pared segun se esta mirando a la pared o no, tendre que hacerlo de otra manera 
                if (playerController.CanClimb)
                   {
                       playerController.WallJump(normalizedHorizontalSpeed);
                   }
                */
            }
            else if (playerController.CanJump)
            {
                playerController.Jump();
                removeTouch(touch.fingerId);
            }
        }
        else
        {
            if (playerController.CanClimb)
            {
                bool verticalDir = false;
                playerController.ClimbUpDown(verticalDir);
            }
            else
            {
                if (playerController.CanJumpAttack)
                {
                    animator.SetBool("boolJumpAttack", true);
                    playerController.MoveHorizontally(0);
                }
                else if (!playerController.CanClimb)
                {
                    // el movimiento es para abajo.
                    animator.SetTrigger("triggerDown");
                }
                removeTouch(touch.fingerId);

            }

        }
        touchInz.setMove(EnumTouch.verticalMove);

    }

    private void horizontalMove(TouchClazz touchInz, Vector2 mouseMovement)
    {
        normalizedHorizontalSpeed = (mouseMovement.x) > 0 ? 1 : -1;
        playerController.MoveHorizontally(normalizedHorizontalSpeed);
        touchInz.setMove(EnumTouch.horizontalMove);

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
        else if (playerController.CanJump)
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
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (playerController.CanJumpAttack)
            {
                animator.SetBool("boolJumpAttack", true);
                playerController.MoveHorizontally(0);
            }
            else if (!playerController.CanClimb)
            {
                animator.SetTrigger("triggerDown");
            }

        }
        if (!playerController.CanJumpAttack)
        {
            animator.SetBool("boolJumpAttack", false);
        }


        if (Input.GetKeyDown(KeyCode.UpArrow) && playerController.CanJump)
        {
            playerController.Jump();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playerController.CanClimb)
            {
                playerController.WallJump(normalizedHorizontalSpeed);
            }
            else
            {
                animator.SetTrigger("triggerFrontAttack");
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (touchCountKey > delaySlowTime && !gameController.isTimeSlowedDown)
            {
                gameController.SwapTimeScale();
            }
            touchCountKey++;


        }
        else {

            touchCountKey = 0;
            if (gameController.isTimeSlowedDown)
            {
                gameController.SwapTimeScale();
            }
        }

        if (gameController.isTimeSlowedDown)
        {
            // estamos en slowIime
            if (Input.GetKeyDown(KeyCode.A))
            {
                /*
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.position + transform.position / 2);
                    if (hit.collider == null)
                    {
                    */
                Vector3 posicionFinal = new Vector3(transform.position.x + 70, transform.position.y + 60, 0);
                Vector2 mouseMovement = posicionFinal - transform.position;

                Vector3 normalizado = mouseMovement.normalized;

                /*
                RaycastHit2D hit = Physics2D.Linecast(transform.position, transform.position + normalizado * 5, blockingLayer);
                if (!hit)
                {
                */
                DrawDashLine(transform.position, transform.position + normalizado * 5, 1f);
                transform.Translate(normalizado * 5);

                // playerController.AddForce(normalizado.x * 200, normalizado.y * 200);
                gameController.SwapTimeScale();


                // }

            }
        }
    }

    void HandleMouse()
    {

        normalizedHorizontalSpeed = 0;

        if (Input.GetMouseButtonDown(0))
        {
            DrawCircle(Input.mousePosition);
            posicionInicialMouse = Input.mousePosition;
        }


        if (Input.GetMouseButton(0))
        {
            Color color = Color.red;
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            GameObject touchLine = new GameObject();
            touchLine.layer = uiTouch.layer;
            // touchLine.transform.SetParent(canvas.transform, false);
            touchLine.transform.position = posicionInicialMouse;
            touchLine.AddComponent<LineRenderer>();
            LineRenderer lr = touchLine.GetComponent<LineRenderer>();
            lr.startColor = color;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            lr.SetPosition(0, posicionInicialMouse);
            lr.SetPosition(1, Input.mousePosition);

        }

        playerController.MoveHorizontally(normalizedHorizontalSpeed);

    }
    private void DrawDashLine(Vector3 start, Vector3 end, float duration = 0.2f)
    {
        try
        {
            Color color = Color.blue;
            GameObject myLine = new GameObject();
            myLine.transform.SetParent(canvas.transform, false);
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();

            lr.startColor = color;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            GameObject.Destroy(myLine, duration);
        }
        catch (Exception e)
        {
            infoError.text = "Error" + e.Message + e.StackTrace;

        }
    }

    private void DrawTouchLine(int touchId, Vector3 end)
    {
        try
        {
            Color color = Color.blue;

            TouchClazz touchIntz = touchIdTopressedPosition[touchId];
            //uiTouch.GetComponentInChildren<GameObject>();

            if (!touchIntz.getTouchLine())
            {
                GameObject touchLine = new GameObject();
                touchLine.transform.position = touchIntz.getPosition();
                touchLine.AddComponent<LineRenderer>();
                touchLine.transform.SetParent(touchIntz.getUiTouch().transform, false);
                touchIntz.setTouchLine(touchLine);

                LineRenderer lr = touchLine.GetComponent<LineRenderer>();
                lr.startColor = color;
            
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, touchIntz.getPosition());
                lr.SetPosition(1, end);


            }
            else
            {
                GameObject touchLine = touchIntz.getTouchLine();
                LineRenderer lr = touchLine.GetComponent<LineRenderer>();
                lr.SetPosition(1, end);
            }

            //  GameObject.Destroy(myLine, duration);
        }
        catch (Exception e)
        {
            infoError.text = "Error" + e.Message + e.StackTrace;

        }
    }

    private GameObject DrawCircle(Vector2 center)
    {
        // isntanciamos el objeto uiToucho que se vera cuando cliquemos
        GameObject iuTouch = Instantiate(uiTouch);
        iuTouch.transform.SetParent(canvas.transform, false);
        iuTouch.transform.localScale = new Vector3(2, 2, 1);
        iuTouch.transform.position = center;

        return iuTouch;
    }

    public void removeTouch(int touchId)
    {
        touchIdTopressedPosition[touchId].destroyUiTouch();
        touchIdTopressedPosition.Remove(touchId);
    }
}

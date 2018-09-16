using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Knight2 : MonoBehaviour
{

    public float movementForce = 20f;
    public float airMovementForce = 20f;

    public float jumpForce = 20000f;

    public float maxVelocity = 10f;
    public int offsetTouch = 75;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private GroundCheck groundCheck;
    private GrabCheck grabCheck;
    private Animator animator;
    private bool grabbingEdge = false;

    private Vector2[] mousePressedPosition;

   private string DOWN_KEY = "274";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        grabCheck = GetComponentInChildren<GrabCheck>();
        animator = GetComponent<Animator>();

        Assert.IsNotNull(groundCheck);
        Assert.IsNotNull(rb);
        Assert.IsNotNull(sr);

        //rb.constraints = RigidbodyConstraints2D.FreezeRotation;

#if UNITY_EDITOR
        mousePressedPosition = new Vector2[1];
#elif (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8)
        mousePressedPosition = new Vector2[5];
#endif

    }

    void FixedUpdate()
    {
        float hVelocity = rb.velocity.x;

        if (Mathf.Abs(hVelocity) > maxVelocity)
        {
            Vector2 counterForce = -Vector2.right * rb.mass * Mathf.Sign(hVelocity) * (Mathf.Abs(hVelocity) - maxVelocity) / Time.deltaTime;
            rb.AddForce(counterForce);
        }
    }

    void Update()
    {

#if UNITY_EDITOR
        // HandleMouseInput();
#elif (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8)
        HandleTouchInput();
#endif
       // HandleTouchInput();
        /*

                KeyEvent(null);

                if (rb.velocity.y < 0 && !groundCheck.isGrounded && grabCheck.canGrab)
                {
                    grabbingEdge = true;
                    rb.AddForce(-rb.velocity * rb.mass, ForceMode2D.Impulse);
                    rb.gravityScale = 0f;
                }


                if (rb.velocity.x > 1f)
                {
                    sr.flipX = true;

                }
                else if (rb.velocity.x < -1f)
                {
                    sr.flipX = false;
                }

            */
    }
    public void KeyEvent(string key)
    {
        if (!grabbingEdge)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (groundCheck.isGrounded)
                {
                    rb.AddForce(movementForce * Vector2.left, ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(airMovementForce * Vector2.left, ForceMode2D.Impulse);
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (groundCheck.isGrounded)
                {
                    rb.AddForce(movementForce * Vector2.right, ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(airMovementForce * Vector2.right, ForceMode2D.Impulse);
                }

            }

            if (Input.GetKeyDown(DOWN_KEY) && groundCheck.isGrounded)
            {
                animator.SetTrigger("triggerDown");
                
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                animator.SetTrigger("triggerFrontAttack");
            }


        }
        if (grabbingEdge)
        {
            //si se esta cojiendo a la pared saltamos.
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                grabbingEdge = false;
                rb.gravityScale = 3f;
                rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                grabbingEdge = false;
                rb.gravityScale = 3f;
                rb.AddForce(1000 * Vector2.up, ForceMode2D.Impulse);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                grabbingEdge = false;
                rb.gravityScale = 3f;
                rb.AddForce(1000 * Vector2.down, ForceMode2D.Impulse);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightControl))
        {
            Jump();
        }
    }

    void HandleTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            {
                // capturamos el comienzo del touch
                touchInicial(i, touch);

                // gestionamos los touches despues de tener un primero
                touchMedio(i, touch);

                // finalizamos el touch y lo limpiamos.
                touchFinal(i, touch);
            }
        }


        if (rb.velocity.y < 0 && !groundCheck.isGrounded && grabCheck.canGrab)
        {
            grabbingEdge = true;
            rb.AddForce(-rb.velocity * rb.mass, ForceMode2D.Impulse);
            rb.gravityScale = 0f;
        }


        if (rb.velocity.x > 1f)  {
            sr.flipX = true; }
        else if (rb.velocity.x < -1f) {
            sr.flipX = false;
        }
    }

    private void touchInicial(int i, Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            mousePressedPosition[i] = touch.position;
        }
    }


    private void touchMedio(int i, Touch touch)
    {
        //si no tenemos touch inicial, o estamos cogidos, nos salimos
        if (mousePressedPosition[i] == null || mousePressedPosition[i] == Vector2.zero) {
            return;
        }

        Vector2 mouseMovement = touch.position - mousePressedPosition[i];
        if (Mathf.Abs(mouseMovement.y) < Mathf.Abs(mouseMovement.x)&& !grabbingEdge && Mathf.Abs(mouseMovement.x) > offsetTouch)
        {
            // entra aqui si es un movimiento en x
            float distance = 0.8f * mouseMovement.x / maxVelocity;
            rb.AddForce(movementForce * distance * Vector2.right, ForceMode2D.Impulse);

        }else if (Mathf.Abs(mouseMovement.y) > Mathf.Abs(mouseMovement.x) && Mathf.Abs(mouseMovement.y) > offsetTouch)
        {
            //entra aqui si el movimiento es en y
            ContextualAction(touch.position - mousePressedPosition[i], mousePressedPosition[i].y);
            mousePressedPosition[i] = Vector2.zero;
        }
        
    }
    private void touchFinal(int i, Touch touch)
    {
        if (touch.phase == TouchPhase.Ended)
        {
            Vector2 mouseMovement = touch.position - mousePressedPosition[i];
            if (Mathf.Abs(mouseMovement.y)<50 && Mathf.Abs(mouseMovement.x)<50)
            {
                animator.SetTrigger("triggerFrontAttack");
            }
            mousePressedPosition[i] = Vector2.zero;
        }
    }


    void ContextualAction(Vector2 mouseMovement, float y)
    {      
        if (Mathf.Abs(mouseMovement.y) > Mathf.Abs(mouseMovement.x))
        {
            if (grabbingEdge)
            {
                //si se esta cojiendo a la pared saltamos.     
                    grabbingEdge = false;
                    rb.gravityScale = 3f;
                    rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            }
            else if (mouseMovement.y < 0 && groundCheck.isGrounded)
            {
                    animator.SetTrigger("triggerDown");
            }
            else if (groundCheck.isGrounded)
            {
                Jump();
            }
            
        }
    }



    void ApplyForce(Vector2 mouseMovement)
    {
        float distance = 0.8f * mouseMovement.x / maxVelocity;
        rb.AddForce(distance * Time.deltaTime * Vector2.right, ForceMode2D.Force);
    }

    public void Jump()
    {
        if (groundCheck.isGrounded)
        {
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        }
    }
}

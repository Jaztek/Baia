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

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private GroundCheck groundCheck;
    private GrabCheck grabCheck;
    private Animator animator;
    private bool grabbingEdge = false;

    private Vector2[] mousePressedPosition;
    private Dictionary<int, Vector2> fingerIdToMousePressedPosition;

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
        HandleMouseInput();
#elif (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8)
        // HandleTouchInput();
#endif

        KeyEvent();

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
    }

    public void KeyEvent()
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
            if (Input.GetKeyDown(KeyCode.DownArrow) && groundCheck.isGrounded)
            {
                animator.SetTrigger("triggerDown");
                
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
                if (touch.phase == TouchPhase.Began)
                {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(touch.position);
                    RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

                    // TODO constante
                    if (!hit || hit.collider.gameObject.layer != 5)
                    {
                        fingerIdToMousePressedPosition[touch.fingerId] = touch.position;
                        mousePressedPosition[i] = touch.position;
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (groundCheck.isGrounded)
                    {
                        ApplyForce(touch.position - fingerIdToMousePressedPosition[touch.fingerId]);
                        // ApplyForce(touch.position - mousePressedPosition[i]);
                    }
                    fingerIdToMousePressedPosition[touch.fingerId] = Vector2.zero;
                    mousePressedPosition[i] = Vector2.zero;
                }
            }
        }
    }

    void HandleMouseInput()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            if (!hit || hit.collider.gameObject.layer != 5)
            {
                mousePressedPosition[0] = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (groundCheck.isGrounded)
            {
                Vector2 mouseReleasedPosition = Input.mousePosition;
                ApplyForce(mouseReleasedPosition - mousePressedPosition[0]);
            }
            mousePressedPosition[0] = Vector2.zero;
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

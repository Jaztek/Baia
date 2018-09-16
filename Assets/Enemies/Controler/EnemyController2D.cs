using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController2D : MonoBehaviour
{

    [Header("Raycasting")]
    public int totalHorizontalRays = 8;
    public int totalVerticalRays = 6;

    [Space(5)]

    public LayerMask groundMask;
    public ControllerParameters2D Parameters;
    public ControllerState2D state = new ControllerState2D();

    public bool IsFacingRight { get; private set; }

    float skinWidth = 0.002f;
    float horizontalDistanceBetweenRays;
    float verticalDistanceBetweenRays;

    Vector2 raycastTopLeft;
    Vector2 raycastBottomLeft;
    Vector2 raycastBottomRight;

    BoxCollider2D boxCollider;
    Vector2 velocity;
    List<Intent> intents;

    void Start()
    {
        IsFacingRight = false;

        intents = new List<Intent>();

        boxCollider = GetComponent<BoxCollider2D>();

        float colliderWidthNoSkin = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - 2 * skinWidth;
        float colliderHeightNoSkin = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - 2 * skinWidth;

        horizontalDistanceBetweenRays = colliderWidthNoSkin / (totalVerticalRays - 1);
        verticalDistanceBetweenRays = colliderHeightNoSkin / (totalHorizontalRays - 1);
    }

    void FixedUpdate()
    {

        state.Reset();
        CalculateRayOrigins();

        velocity.y += Parameters.gravity.y * Time.deltaTime;
        if (velocity.y < 0 && Mathf.Abs(velocity.y) > Parameters.maxGravityMagnitude)
        {
            velocity.y = -Parameters.maxGravityMagnitude;
        }

        Vector2 deltaMovement = velocity * Time.deltaTime;

        ApplyGravity(ref deltaMovement);
        // SnapToSlope(ref deltaMovement);
        HandleVerticalMovement(ref deltaMovement);
        if (Mathf.Abs(deltaMovement.x) > 0.0001f)
        {
            HandleHorizontalMovement(ref deltaMovement);
        }

        transform.Translate(deltaMovement);

        if (Time.deltaTime != 0)
        {
            velocity = deltaMovement / Time.deltaTime;
        }

        if (state.isMovingUpSlope)
        {
            velocity.y = 0f;
        }

        HandlePendingIntents();
    }

    void CalculateRayOrigins()
    {
        Vector3 _localScale = transform.localScale;

        Vector2 size = new Vector2(
            boxCollider.size.x * Mathf.Abs(_localScale.x),
            boxCollider.size.y * Mathf.Abs(_localScale.y))
            / 2;

        Vector2 center = new Vector2(
        boxCollider.offset.x * _localScale.x,
        boxCollider.offset.y * _localScale.y);

        raycastTopLeft = transform.position + new Vector3(center.x - size.x + skinWidth, center.y + size.y - skinWidth);
        raycastBottomLeft = transform.position + new Vector3(center.x - size.x + skinWidth, center.y - size.y + skinWidth);
        raycastBottomRight = transform.position + new Vector3(center.x + size.x - skinWidth, center.y - size.y + skinWidth);
    }

    void ApplyGravity(ref Vector2 deltaMovement)
    {
        deltaMovement += Parameters.gravity * Time.deltaTime * Time.deltaTime;
    }

    void HandleVerticalMovement(ref Vector2 deltaMovement)
    {
        bool isMovingUp = deltaMovement.y > 0;

        if (isMovingUp)
        {
            // UP
            Vector2 rayOrigin = raycastTopLeft;
            Vector2 rayDirection = Vector2.up;
            float rayDistance = Mathf.Abs(deltaMovement.y) + skinWidth;

            for (int i = 0; i < totalVerticalRays; i++)
            {
                Vector2 rayVector = rayOrigin + i * horizontalDistanceBetweenRays * Vector2.right;
                Debug.DrawRay(rayVector, rayDistance * rayDirection, Color.red);
                RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, groundMask);

                if (!raycastHit)
                {
                    continue;
                }

                state.isCollidingAbove = true;

                deltaMovement.y = raycastHit.point.y - rayVector.y;
                rayDistance = Mathf.Abs(deltaMovement.y);
                deltaMovement.y -= skinWidth;

                if (rayDistance < 0.0001f)
                {
                    break;
                }
            }

        }
        else
        {
            // DOWN
            Vector2 rayOrigin = raycastBottomLeft;
            Vector2 rayDirection = Vector2.down;
            float rayDistance = Mathf.Abs(deltaMovement.y) + skinWidth;

            for (int i = 0; i < totalVerticalRays; i++)
            {
                Vector2 rayVector = rayOrigin + i * horizontalDistanceBetweenRays * Vector2.right;
                Debug.DrawRay(rayVector, rayDistance * rayDirection, Color.red);
                RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, groundMask);

                if (!raycastHit)
                {
                    continue;
                }

                state.isCollidingBelow = true;

                if (i == 0 || i == totalVerticalRays - 1)
                {
                    float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
                    if (angle != 90)
                    {
                        if (angle >= Parameters.maxSlopeAscendingAngle)
                        {
                            bool isCollidingRight = raycastHit.normal.x < 0;
                            int direction = isCollidingRight ? -1 : 1;

                            if (isCollidingRight)
                            {
                                state.isCollidingRight = true;
                            }
                            else
                            {
                                state.isCollidingLeft = true;
                            }

                            state.isMovingDownSlope = true;
                            state.slopeAngle = angle;

                            deltaMovement.x = direction * Mathf.Abs(deltaMovement.y / Mathf.Tan(angle * Mathf.Deg2Rad));
                            continue;
                        }
                    }
                }

                deltaMovement.y = raycastHit.point.y - rayVector.y;
                rayDistance = Mathf.Abs(deltaMovement.y);
                deltaMovement.y += skinWidth;

                if (rayDistance < 0.0001f)
                {
                    break;
                }
            }
        }
    }

    void HandleHorizontalMovement(ref Vector2 deltaMovement)
    {
        bool isMovingRight = deltaMovement.x > 0;
        float defaultDistance = Mathf.Max(Mathf.Abs(deltaMovement.x) + skinWidth, 2 * skinWidth);

        if (!isMovingRight)
        {

            // LEFT
            Vector2 rayOrigin = raycastBottomLeft;
            rayOrigin.y += deltaMovement.y;


            Vector2 rayDirection = Vector2.left;
            float rayDistance = defaultDistance;

            for (int i = 0; i < totalHorizontalRays; i++)
            {
                Vector2 rayVector = rayOrigin + i * verticalDistanceBetweenRays * Vector2.up;
                Debug.DrawRay(rayVector, rayDistance * rayDirection, Color.yellow);
                RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, groundMask);

                if (!raycastHit)
                {
                    continue;
                }


                if (i == 0)
                {
                    float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
                    if (angle != 90)
                    {
                        if (angle <= Parameters.maxSlopeAscendingAngle)
                        {
                            state.isCollidingBelow = true;
                            state.isMovingUpSlope = true;
                            state.slopeAngle = angle;
                            deltaMovement.y = -deltaMovement.x * Mathf.Tan(angle * Mathf.Deg2Rad);
                            break;
                        }
                    }
                }

                state.isCollidingLeft = true;
                deltaMovement.x = raycastHit.point.x - rayVector.x;
                rayDistance = Mathf.Abs(deltaMovement.x);
                deltaMovement.x += skinWidth;

                if (rayDistance < 0.0001f)
                {
                    break;
                }
            }

        }
        else
        {
            // RIGHT

            Vector2 rayOrigin = raycastBottomRight;
            rayOrigin.y += deltaMovement.y;

            Vector2 rayDirection = Vector2.right;
            float rayDistance = defaultDistance;

            for (int i = 0; i < totalHorizontalRays; i++)
            {
                Vector2 rayVector = rayOrigin + i * verticalDistanceBetweenRays * Vector2.up;
                Debug.DrawRay(rayVector, rayDistance * rayDirection, Color.yellow);
                RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, groundMask);

                if (!raycastHit)
                {
                    continue;
                }

                if (i == 0)
                {
                    float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
                    if (angle != 90)
                    {
                        if (angle < Parameters.maxSlopeAscendingAngle)
                        {
                            state.isCollidingBelow = true;
                            state.isMovingUpSlope = true;
                            state.slopeAngle = angle;
                            deltaMovement.y = deltaMovement.x * Mathf.Tan(angle * Mathf.Deg2Rad);
                            break;
                        }
                    }
                }

                state.isCollidingRight = true;
                deltaMovement.x = raycastHit.point.x - rayVector.x;
                rayDistance = Mathf.Abs(deltaMovement.x);
                deltaMovement.x -= skinWidth;

                if (rayDistance < 0.0001f)
                {
                    break;
                }
            }
        }
    }

    void SnapToSlope(ref Vector2 deltaMovement)
    {
        if (state.isMovingUpSlope || deltaMovement.y >= 0 || Mathf.Abs(deltaMovement.x) < 0.0001f)
        {
            return;
        }

        float center = (raycastBottomLeft.x + raycastBottomRight.x) / 2;
        Vector2 direction = Vector2.down;

        float slopeLimitTangent = Mathf.Tan(Parameters.maxSlopeDescendingAngle * Mathf.Deg2Rad);

        float slopeDistance = slopeLimitTangent * (raycastBottomRight.x - center);
        Vector2 slopeRayVector = new Vector2(center, raycastBottomLeft.y);

        Debug.DrawRay(slopeRayVector, direction * slopeDistance, Color.yellow);

        RaycastHit2D raycastHit = Physics2D.Raycast(slopeRayVector, direction, slopeDistance, groundMask);

        if (!raycastHit)
        {
            return;
        }

        float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        if (Mathf.Abs(angle) < 0.0001f)
        {
            return;
        }

        bool isMovingdownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);

        if (!isMovingdownSlope)
        {
            return;
        }

        state.slopeAngle = angle;
        state.isMovingDownSlope = true;
        state.isCollidingBelow = true;

        deltaMovement.y = raycastHit.point.y - slopeRayVector.y;
    }

    void HandlePendingIntents()
    {
        foreach (Intent intent in intents)
        {
            switch (intent.type)
            {
                case Intent.Type.H_MOVE_LEFT:
                    AddHorizontalVelocity(-1);
                    if (transform.localScale.x > 0)
                    {
                        IsFacingRight = false;
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    }
                    break;
                case Intent.Type.H_MOVE_RIGHT:
                    AddHorizontalVelocity(1);
                    if (transform.localScale.x < 0)
                    {
                        IsFacingRight = true;
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    }
                    break;
                 case Intent.Type.H_MOVE_NONE:
                     AddHorizontalVelocity(0);
                    break;
                case Intent.Type.JUMP:

                    if (CanJump())
                    {
                        Jump();
                    }
                    break;
            }
        }
        intents.Clear();
    }

    void AddHorizontalVelocity(int direction)
    {
        float acceleration = state.isCollidingBelow ? Parameters.speedAccelerationOnGround : Parameters.speedAccelerationOnAir;
        velocity.x = Mathf.Lerp(velocity.x, direction * Parameters.maxHorizontalVelocity, Time.deltaTime * acceleration);
    }

    // TODO revisar estos métodos
    bool CanMoveHorizontally()
    {
        if (state.slopeAngle > Parameters.maxSlopeAscendingAngle)
        {
            return false;
        }
        return true;
    }

    public bool CanJump()
    {
        if (!state.isCollidingBelow)
        {
            return false;
        }

        if (state.slopeAngle > Parameters.maxSlopeAscendingAngle)
        {
            return false;
        }

        return true;
    }

    void Jump()
    {
        velocity.y += Parameters.jumpVelocity;
    }

    void UnJump()
    {
        if (velocity.y > 0)
        {
            velocity.y = velocity.y / 2;
        }

    }

    public void AddIntent(Intent intent)
    {
        intents.Add(intent);
    }

    void OnValidate()
    {
        if (totalHorizontalRays % 2 == 0)
        {
            totalHorizontalRays += 1;
        }

        if (totalVerticalRays % 2 == 0)
        {
            totalVerticalRays += 1;
        }
    }
}



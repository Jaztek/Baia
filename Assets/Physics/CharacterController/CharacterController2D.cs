using System;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerAction
{
    MOVE_LEFT, MOVE_RIGHT, JUMP, CANCEL_JUMP, JETPACK, ROLL
}

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Raycasting")]
    public int totalHorizontalRays = 8;
    public int totalVerticalRays = 6;

    [Space(5)]

    public LayerMask groundMask;
    public ControllerParameters2D Parameters;
    public ControllerState2D state = new ControllerState2D();

    public bool IsFacingRight { get; private set; }

    public bool MovementDisabled { get; set; }

    float skinWidth = 0.002f;
    float horizontalDistanceBetweenRays;
    float verticalDistanceBetweenRays;

    float disabledMovementTimer = 0f;
    float wallJumpTimer = 0f;

    Vector2 raycastTopLeft;
    Vector2 raycastBottomLeft;
    Vector2 raycastBottomRight;

    BoxCollider2D boxCollider;
    AnimatorController animatorController;
    Vector2 velocity;

    Jetpack jetpack;

    bool maxJumpHeightAchieved;

    bool moveLeftRequest;
    bool moveRightRequest;
    bool jumpRequest;
    bool jetpackRequest;
    bool cancelJumpRequest;
    bool attackRequest;
    bool rollRequest;

    public void Request(ControllerAction action)
    {
        if (action == ControllerAction.MOVE_LEFT)
        {
            moveLeftRequest = true;
        }
        else if (action == ControllerAction.MOVE_RIGHT)
        {
            moveRightRequest = true;
        }
        else if (action == ControllerAction.JUMP)
        {
            jumpRequest = true;
        }
        else if (action == ControllerAction.ROLL)
        {
            rollRequest = true;
        }
        else if (action == ControllerAction.CANCEL_JUMP)
        {
            cancelJumpRequest = true;
        }
        else if (action == ControllerAction.JETPACK)
        {
            jetpackRequest = true;
        }
    }

    void Start()
    {
        IsFacingRight = true;

        jetpack = GetComponentInChildren<Jetpack>();

        boxCollider = GetComponent<BoxCollider2D>();
        animatorController = GetComponent<AnimatorController>();

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
        if (Mathf.Abs(velocity.y) > Parameters.maxGravityMagnitude)
        {
            if (velocity.y > 0)
            {
                velocity.y = Parameters.maxGravityMagnitude;
            }
            else
            {
                velocity.y = -Parameters.maxGravityMagnitude;
            }
        }

        Vector2 deltaMovement = velocity * Time.deltaTime;

        ApplyGravity(ref deltaMovement);
        // SnapToSlope(ref deltaMovement);
        HandleVerticalMovement(ref deltaMovement);

        // if (Mathf.Abs(deltaMovement.x) > 0.0001f)
        // {
        HandleHorizontalMovement(ref deltaMovement);
        // }

        transform.Translate(deltaMovement);

        if (Time.deltaTime != 0)
        {
            velocity = deltaMovement / Time.deltaTime;
        }

        if (state.isMovingUpSlope)
        {
            velocity.y = 0f;
        }

        if (!maxJumpHeightAchieved && !IsGrounded() && velocity.y < 0)
        {
            maxJumpHeightAchieved = true;
        }

        if (!IsGrounded() && (state.isCollidingRight || state.isCollidingLeft))
        {
            wallJumpTimer = Parameters.wallJumpLeeway;
        }

        if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        if (disabledMovementTimer > 0f)
        {
            disabledMovementTimer -= Time.deltaTime;
        }
    }

    private void Update()
    {
        if (jumpRequest)
        {
            if (CanJump())
            {
                Jump();
            }
            else if (CanWallJump())
            {
                WallJump();
                disabledMovementTimer = Parameters.wallJumpDisableMovementTime;
            }
        }

        if (jetpackRequest)
        {
            if (CanJetPack())
            {
                // JetPack();
            }
        }

        if (cancelJumpRequest)
        {
            if (!CanJump())
            {
                UnJump();
            }
        }

        if (CanMove())
        {
            if (moveLeftRequest)
            {
                AddHorizontalVelocity(-1);
            }
            else if (moveRightRequest)
            {
                AddHorizontalVelocity(1);
            }
        }

        if (!moveRightRequest && !moveLeftRequest && disabledMovementTimer <= 0f)
        {
            AddHorizontalVelocity(0);
        }

        if (attackRequest && CanMove())
        {
            // TODO Attack Req.
        }

        if (rollRequest && CanMove())
        {
            Roll();
        }

        if (moveRightRequest && CanMove())
        {
            IsFacingRight = true;
            animatorController.SetAnimation(PlayerAnim.MOVE);
        }
        else if (moveLeftRequest && CanMove())
        {
            IsFacingRight = false;
            animatorController.SetAnimation(PlayerAnim.MOVE);
        }
        else if (disabledMovementTimer <= 0f)
        {
            animatorController.SetAnimation(PlayerAnim.IDLE);
        }

        if (IsFacingRight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        jumpRequest = false;
        cancelJumpRequest = false;
        moveLeftRequest = false;
        jetpackRequest = false;
        moveRightRequest = false;
        attackRequest = false;
        rollRequest = false;
    }

    private void JetPack()
    {
        jetpack.Tick();
        velocity.y += jetpack.CalculateAcceleration() * Time.deltaTime;
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

                maxJumpHeightAchieved = false;
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
        float defaultDistance = Mathf.Max(Mathf.Abs(deltaMovement.x) + skinWidth, 10 * skinWidth);

        if (!IsFacingRight)
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
                            maxJumpHeightAchieved = false;
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
                            maxJumpHeightAchieved = false;
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
        maxJumpHeightAchieved = false;
        state.isMovingDownSlope = true;
        state.isCollidingBelow = true;

        deltaMovement.y = raycastHit.point.y - slopeRayVector.y;
    }

    void AddHorizontalVelocity(int direction)
    {
        float acceleration = state.isCollidingBelow ? Parameters.speedAccelerationOnGround : Parameters.speedAccelerationOnAir;
        velocity.x = Mathf.Lerp(velocity.x, direction * Parameters.maxHorizontalVelocity, Time.deltaTime * acceleration);

    }

    void Roll()
    {
        animatorController.SetAnimation(PlayerAnim.ROLL);
        int facePosition = IsFacingRight ? 1 : -1;
        velocity.x = Parameters.maxHorizontalVelocity * 8 * facePosition;
    }

    bool CanMoveHorizontally()
    {
        return true;
    }

    public bool IsFalling()
    {
        return velocity.y < 0 && !IsGrounded();
    }

    public bool IsGrounded()
    {
        return state.isCollidingBelow;
    }

    public bool CanJetPack()
    {
        return maxJumpHeightAchieved && CanMove() && !IsGrounded();
    }

    public bool CanMove()
    {
        return !MovementDisabled && disabledMovementTimer <= 0f;
    }

    public bool CanJump()
    {
        if (!CanMove())
        {
            return false;
        }

        if (!IsGrounded())
        {
            return false;
        }

        return true;
    }

    public bool CanWallJump()
    {
        if (!CanMove())
        {
            return false;
        }

        if (IsGrounded())
        {
            return false;
        }

        return wallJumpTimer > 0f;
    }

    void Jump()
    {
        velocity.y += Parameters.jumpVelocity;
    }

    void WallJump()
    {
        velocity.x = 0f;
        if (state.latestKnownWallPosition == 1)
        {
            IsFacingRight = false;
            AddHorizontalVelocity(-4);
        }
        else
        {
            IsFacingRight = true;
            AddHorizontalVelocity(4);
        }
        velocity.y = Parameters.jumpVelocity;
    }

    void UnJump()
    {
        if (velocity.y > 0)
        {
            velocity.y = velocity.y / 2;
        }

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

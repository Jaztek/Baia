using UnityEngine;
using UnityEngine.Assertions;

public class CharacterController2D : MonoBehaviour
{

    public LayerMask groundMask;

    public int totalHorizontalRays = 8;
    public int totalVerticalRays = 4;

    public Vector2 Velocity { get { return _velocity; } }
    public bool CanJump { get { return state.IsGrounded || state.IsClimbing; } }
    public bool CanClimb { get { return state.IsClimbing; } }

    public ControllerParameters2D Parameters;

    private float skinWidth = 0.002f;

    private Vector2 _velocity;
    private float horizontalDistanceBetweenRays;
    private float verticalDistanceBetweenRays;

    private BoxCollider2D boxCollider;
    private Vector2 raycastTopLeft;
    private Vector2 raycastBottomLeft;
    private Vector2 raycastBottomRight;

    private ControllerState2D state;

    private bool isFacingRight = true;

    void Awake()
    {

        LoadParameters();
        state = new ControllerState2D();

        boxCollider = GetComponent<BoxCollider2D>();
        Assert.IsNotNull(boxCollider);

        float colliderWidthNoSkin = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - 2 * skinWidth;
        float colliderHeightNoSkin = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - 2 * skinWidth;

        horizontalDistanceBetweenRays = colliderWidthNoSkin / (totalVerticalRays - 1);
        verticalDistanceBetweenRays = colliderHeightNoSkin / (totalHorizontalRays - 1);
    }

    void LoadParameters()
    {
        Parameters.maxHorizontalVelocity = PlayerPrefs.GetFloat("maxHorizontalVelocity", 10f);
        Parameters.speedAccelerationOnGround = PlayerPrefs.GetFloat("speedAccelerationOnGround", 20f);
        Parameters.speedAccelerationOnAir = PlayerPrefs.GetFloat("speedAccelerationOnAir", 25f);
        Parameters.jumpMagnitude = PlayerPrefs.GetFloat("maxJumpMagnitude", 12f);
        Parameters.gravity = new Vector2(PlayerPrefs.GetFloat("gravityX", 0f), PlayerPrefs.GetFloat("gravityY", -25f));
        Parameters.maxGravityMagnitude = PlayerPrefs.GetFloat("maxGravityMagnitude", 30f);
        Parameters.maxSlopeAscendingAngle = PlayerPrefs.GetFloat("maxAscendingSlope", 30f);
        Parameters.MaxSlopeDescendingAngle = 65f;
    }

    void FixedUpdate()
    {

        bool log = false;

        bool wasGrounded = state.IsGrounded;
        bool wasClimbing = state.IsClimbing;

        state.Reset();

        CalculateRayOrigins();

        if (!wasClimbing)
        {
            ApplyGravity();
        }

        Vector2 deltaMovement = Velocity * Time.deltaTime;

        bool isGoingUpSlope = false;

        if (wasGrounded && _velocity.y < 0)
        {
            if (log)
            {
                Debug.Log(" BEGIN > HandleVerticalSlope: " + deltaMovement.y.ToString());
            }
            HandleDownSlope(ref deltaMovement);
            if (log)
            {
                Debug.Log(" BEGIN > HandleVerticalSlope: " + deltaMovement.y.ToString());
            }
        }


        if (Mathf.Abs(deltaMovement.x) > 0.0001f)
        {
            if (log)
            {
                Debug.Log(" BEGIN > HandleHorizontalMovement: " + deltaMovement.y.ToString());
            }
            isGoingUpSlope = HandleHorizontalMovement(ref deltaMovement, wasClimbing);
            if (log)
            {
                Debug.Log(" END > HandleHorizontalMovement: " + deltaMovement.y.ToString());
            }
        }


        if (log)
        {
            Debug.Log(" BEGIN > HandleVerticalMovement: " + deltaMovement.y.ToString());
        }
        HandleVerticalMovement(ref deltaMovement);
        if (log)
        {
            Debug.Log(" END > HandleVerticalMovement: " + deltaMovement.y.ToString());
        }

        HandleClimbing(ref deltaMovement);

        transform.Translate(deltaMovement);

        if (Time.deltaTime > 0)
        {
            _velocity = deltaMovement / Time.deltaTime;
        }

        if (state.IsClimbing || isGoingUpSlope)
        {
            _velocity.y = 0;
        }

        Debug.Log("isGrounded: " + state.IsGrounded);
        Debug.Log("IsDownSlope: " + state.IsMovingDownSlope);
        Debug.Log("IsUpSlope: " + state.IsMovingUpSlope);
        Debug.Log("Slope angle: " + state.SlopeAngle.ToString());
    }

    void HandleClimbing(ref Vector2 deltaMovement)
    {

        if (state.IsGrounded)
        {
            return;
        }

        float distance = 0.01f;

        Vector2 rayVector = raycastTopLeft;
        rayVector += deltaMovement;

        float horizontalDistance = raycastBottomRight.x - raycastBottomLeft.x;
        float verticalDistance = raycastTopLeft.y - raycastBottomLeft.y;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                bool rightside = j == 1;
                Vector2 rayOrigin = rayVector + new Vector2(j * horizontalDistance, -i * verticalDistance);
                Vector2 direction = rightside ? Vector2.right : Vector2.left;

                Debug.DrawRay(rayOrigin, direction * distance, Color.yellow);
                RaycastHit2D raycastHit = Physics2D.Raycast(rayOrigin, direction, distance, groundMask);

                if (raycastHit && raycastHit.collider.gameObject.CompareTag("Climbeable"))
                {
                    state.IsClimbing = true;
                    break;
                }
            }
        }
    }


    public void MoveHorizontally(int direction)
    {
        UpdateFacingDirection(direction);
        if (!state.IsClimbing)
        {
            float acceleration = state.IsGrounded ? Parameters.speedAccelerationOnGround : Parameters.speedAccelerationOnAir;
            _velocity.x = Mathf.Lerp(_velocity.x, direction * Parameters.maxHorizontalVelocity, Time.deltaTime * acceleration);
        }
    }

    private void UpdateFacingDirection(int newDirection)
    {
        if (newDirection > 0)
        {
            if (!isFacingRight)
            {
                Flip();
            }
        }
        else if (newDirection < 0)
        {
            if (isFacingRight)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        isFacingRight = transform.localScale.x > 0;
    }

    public void SetVerticalForce(float force)
    {
        _velocity.y = force;
    }

    public void AddVerticalForce(float force)
    {
        _velocity.y += force;
    }


    public void SetHorizontalForce(float force)
    {
        _velocity.x = force;
    }

    public void AddHorizontalForce(float force)
    {
        _velocity.x += force;
    }

    public void ClimbUp()
    {
        SetVerticalForce(Parameters.climbMagnitude);
    }

    public void ClimbDown()
    {
        SetVerticalForce(-Parameters.climbMagnitude);
    }

    public void WallJump(int movementIntentDirection)
    {
        if (movementIntentDirection == 0)
        {
            if (state.ClimbingWallAtRightSide)
            {
                AddHorizontalForce(-3f);
            }
            else
            {
                AddHorizontalForce(3f);
            }
        }
        else if (movementIntentDirection == 1 && state.ClimbingWallAtRightSide)
        {
            AddHorizontalForce(-3f);
        }
        else if (movementIntentDirection == -1 && !state.ClimbingWallAtRightSide)
        {
            AddHorizontalForce(3f);
        }
        else
        {
            Jump();
            if (state.ClimbingWallAtRightSide)
            {
                AddHorizontalForce(-Parameters.jumpMagnitude);
            }
            else
            {
                AddHorizontalForce(Parameters.jumpMagnitude);
            }
        }
    }

    public void Jump()
    {
        AddVerticalForce(Parameters.jumpMagnitude);
    }

    bool HandleHorizontalMovement(ref Vector2 deltaMovement, bool wasClimbing)
    {

        bool isGoingRight = deltaMovement.x > 0;

        Vector2 rayOrigin = isGoingRight ? raycastBottomRight : raycastBottomLeft;
        Vector2 rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        float rayDistance = Mathf.Abs(deltaMovement.x) + skinWidth;

        for (int i = 0; i < totalHorizontalRays; i++)
        {
            Vector2 rayVector = rayOrigin + i * verticalDistanceBetweenRays * Vector2.up;
            Debug.DrawRay(rayOrigin, rayDistance * rayDirection, Color.red);
            RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, groundMask);

            if (!raycastHit)
            {
                continue;
            }

            if (i == 0)
            {
                float angle = Vector2.Angle(raycastHit.normal, Vector2.up);

                if (angle != 90 && angle < Parameters.maxSlopeAscendingAngle)
                {
                    state.SlopeAngle = angle;
                    state.IsMovingUpSlope = true;
                    state.IsCollidingBelow = true;

                    deltaMovement.x += isGoingRight ? -skinWidth : skinWidth;

                    float slopeDeltaMovementY = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                    if (slopeDeltaMovementY > deltaMovement.y)
                    {
                        deltaMovement.y = slopeDeltaMovementY;
                        return true;
                    }
                    break;
                }
            }

            deltaMovement.x = raycastHit.point.x - rayVector.x;
            rayDistance = Mathf.Abs(deltaMovement.x);

            if (isGoingRight)
            {
                state.IsCollidingRight = true;
                deltaMovement.x -= skinWidth;
            }
            else
            {
                state.IsCollidingLeft = true;
                deltaMovement.x += skinWidth;
            }

            if (rayDistance < 0.0001f)
            {
                break;
            }
        }
        return false;
    }

    void HandleVerticalMovement(ref Vector2 deltaMovement)
    {
        bool isGoingUp = deltaMovement.y > 0;

        Vector2 rayOrigin = isGoingUp ? raycastTopLeft : raycastBottomLeft;
        Vector2 rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        float rayDistance = Mathf.Abs(deltaMovement.y) + skinWidth;

        rayOrigin.x += deltaMovement.x;

        for (int i = 0; i < totalVerticalRays; i++)
        {
            Vector2 rayVector = rayOrigin + i * horizontalDistanceBetweenRays * Vector2.right;
            Debug.DrawRay(rayVector, rayDistance * rayDirection, Color.red);
            RaycastHit2D raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, groundMask);

            if (!raycastHit)
            {
                continue;
            }

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);

            float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (!isGoingUp && angle != 90 && angle >= Parameters.maxSlopeAscendingAngle)
            {
                int correctedMovementDirection = raycastHit.normal.x > 0 ? 1 : -1;
                deltaMovement.x = correctedMovementDirection * Mathf.Abs(deltaMovement.y) / Mathf.Tan(angle * Mathf.Deg2Rad);
                state.IsMovingDownSlope = true;
                state.SlopeAngle = angle;
            }

            if (isGoingUp)
            {
                state.IsCollidingAbove = true;
                deltaMovement.y -= skinWidth;
            }
            else
            {
                state.IsCollidingBelow = true;
                deltaMovement.y += skinWidth;
            }

            if (rayDistance < 0.0001f)
            {
                break;
            }
        }
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

    void ApplyGravity()
    {
        _velocity += Parameters.gravity * Time.deltaTime;

        float currentGravityVelocityMagnitude = _velocity.magnitude;

        if (currentGravityVelocityMagnitude > Parameters.maxGravityMagnitude)
        {
            _velocity = _velocity.normalized * Parameters.maxGravityMagnitude;
        }
    }

    private void HandleDownSlope(ref Vector2 deltaMovement)
    {

        float center = (raycastBottomLeft.x + raycastBottomRight.x) / 2;
        Vector2 direction = Vector2.down;

        float slopeDistance = Parameters.SlopeLimitTangent * (raycastBottomRight.x - center);
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

        state.SlopeAngle = angle;
        state.IsMovingDownSlope = true;
        state.IsCollidingBelow = true;

        deltaMovement.y = raycastHit.point.y - slopeRayVector.y;
    }
}

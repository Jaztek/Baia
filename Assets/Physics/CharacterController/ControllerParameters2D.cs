using System;
using UnityEngine;

[Serializable]
public class ControllerParameters2D
{

    public float maxHorizontalVelocity;

    public float speedAccelerationOnGround;

    public float speedAccelerationOnAir;

    public float jumpVelocity;

    public float wallJumpDisableMovementTime = 0.2f;
    public float wallJumpLeeway = 0.2f;

    public float climbMagnitude;

    public Vector2 gravity = new Vector2(0, -9.81f);

    [Range(0, 200)]
    public float maxGravityMagnitude;

    [Range(0, 90)]
    public float maxSlopeAscendingAngle;

    [Range(0, 90)]
    public float maxSlopeDescendingAngle;
}

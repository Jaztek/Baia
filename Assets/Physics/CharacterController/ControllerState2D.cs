using System;
using UnityEngine;

[Serializable]
public class ControllerState2D
{
    [ReadOnly]
    public bool isCollidingRight;

    [ReadOnly]
    public bool isCollidingLeft;

    [ReadOnly]
    public bool isCollidingAbove;

    [ReadOnly]
    public bool isCollidingBelow;

    [ReadOnly]
    public bool isMovingUpSlope;

    [ReadOnly]
    public bool isMovingDownSlope;

    [ReadOnly]
    public float slopeAngle;

    [ReadOnly]
    public int latestKnownWallPosition;

    public void Reset()
    {
        if (isCollidingRight)
        {
            latestKnownWallPosition = 1;
        }
        else if (isCollidingLeft)
        {
            latestKnownWallPosition = -1;
        }

        slopeAngle = 0f;
        isCollidingRight = false;
        isCollidingLeft = false;
        isCollidingAbove = false;
        isCollidingBelow = false;
        isMovingUpSlope = false;
        isMovingDownSlope = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour
{

    [Header("Jetpack")]

    public float maxCharge = 1200f;
    public float acceleration = 60f;
    public float decayRatio = 10f;
    public float chargeRatio = 5f;

    public float CurrentCharge { get; private set; }

    private void Start()
    {
        CurrentCharge = maxCharge;
    }

    public bool IsFull()
    {
        return maxCharge == CurrentCharge;
    }

    public void Tick()
    {
        CurrentCharge = Mathf.Max(0, CurrentCharge - decayRatio);
    }

    public float CalculateAcceleration()
    {
        return acceleration * CalculateBoostfactor();
    }

    private float CalculateBoostfactor()
    {
        return Mathf.Min(1, CurrentCharge / decayRatio);
    }

    private void Update()
    {
        CurrentCharge = Mathf.Clamp(CurrentCharge + chargeRatio, 0, maxCharge);
    }
}

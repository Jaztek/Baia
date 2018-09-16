using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public GameObject currentHpBar;
    public float deadForceMultiplicator = 10f;
    public float deadAditionalTorque = 50f;

    [SerializeField]
    float maxHp = 5f;

    float currentHp;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHp = maxHp;
    }

    public void ReceiveDamage(float ammount, Vector2 direction)
    {
        currentHp = Mathf.Clamp(currentHp - ammount, 0, maxHp);
        float newXScale = currentHp / maxHp;

        Vector2 forceVector = rb.mass * ammount * direction;
        if (currentHp == 0)
        {
            forceVector = forceVector * deadForceMultiplicator;
            rb.freezeRotation = false;
            rb.AddTorque(-Mathf.Sign(direction.x) * deadAditionalTorque);
        }
        currentHpBar.transform.localScale = new Vector3(newXScale, currentHpBar.transform.localScale.y, currentHpBar.transform.localScale.z);
        rb.AddForce(forceVector, ForceMode2D.Impulse);
    }
}

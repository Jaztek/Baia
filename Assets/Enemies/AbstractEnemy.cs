using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemy : MonoBehaviour
{
    public GameObject currentHpBar;
    public float deathForceMultiplier = 0f;
    public float deathAdditionalTorque = 0f;

    public float maxHp = 0f;

    protected float currentHp;

    protected Rigidbody2D rb;

    [SerializeField]
    float _damage = 3f;

    public float Damage
    {
        get
        {
            return _damage;
        }
        set
        {
            _damage = value;
        }
    }

    public abstract void OnMove(GameObject player);

    public abstract void OnAttack(GameObject player);

    public abstract void OnExtremis();

    public abstract void OnIdle();

    public virtual void ReceiveDamage(float ammount, Vector2 direction)
    {
        currentHp = Mathf.Clamp(currentHp - ammount, 0, maxHp);
        float newXScale = currentHp / maxHp;

        Vector2 forceVector = ammount * direction / rb.mass;
        if (currentHp == 0)
        {
            forceVector = forceVector * deathForceMultiplier;
            rb.freezeRotation = false;
            rb.AddTorque(-Mathf.Sign(direction.x) * deathAdditionalTorque);
        }
        currentHpBar.transform.localScale = new Vector3(newXScale, currentHpBar.transform.localScale.y, currentHpBar.transform.localScale.z);
        rb.AddForce(forceVector, ForceMode2D.Impulse);
    }
}

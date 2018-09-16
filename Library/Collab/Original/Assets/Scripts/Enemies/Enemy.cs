using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : AbstractEnemy
{
    public ControllerStateEnemy controllerEnemy;
    public GameObject currentHpBar;
    public float deadForceMultiplicator = 10f;
    public float deadAditionalTorque = 50f;

    private CharacterController2Dv2 controllerMovement;
    private EnumStateEnemy.enemyStates state;

    [SerializeField]
    float maxHp = 5f;

    float currentHp;

    Rigidbody2D rb;

    private void Awake()
    {
        controllerEnemy = new ControllerStateEnemy();
        rb = GetComponent<Rigidbody2D>();
        controllerMovement = GetComponent<CharacterController2Dv2>();

        currentHp = maxHp;
    }

    private void Start()
    {
        state = EnumStateEnemy.enemyStates.IDLE;


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

    public override void OnMove(GameObject player)
    {

        state = EnumStateEnemy.enemyStates.MOVE;

        Vector2 vec = player.transform.position - transform.position;

        if (vec.x > 0)
        {
            controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_RIGHT));
        }
        else
        {
            controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_LEFT));
        }




    }

    public override void OnAttack()
    {
        throw new NotImplementedException();
    }

    public override void OnExtremis()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        controllerEnemy.doAction();
    }

    public override void OnIdle()
    {

        state = EnumStateEnemy.enemyStates.IDLE;


        controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_NONE));


    }
}

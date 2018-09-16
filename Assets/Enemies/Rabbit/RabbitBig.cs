using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitBig : AbstractEnemy
{
   
    private Animator animator;

    private EnemyController2D controllerMovement;
    private EnemyState state;

    [SerializeField]


    private void Awake()
    {
        deathForceMultiplier = 10f;
        deathAdditionalTorque = 50f;
        maxHp = 5f;
        currentHp = maxHp;

        rb = GetComponent<Rigidbody2D>();
        controllerMovement = GetComponent<EnemyController2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        state = EnemyState.IDLE;
    }

   

    public override void OnMove(GameObject player)
    {
       
        state = EnemyState.MOVE;
        animator.SetBool("movement", true);
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

    public override void OnAttack(GameObject player)
    {
        state = EnemyState.ATACK;
    }

    public override void OnExtremis()
    {
        throw new NotImplementedException();
    }

    public override void OnIdle()
    {
        animator.SetBool("movement", false);
        state = EnemyState.IDLE;
        // controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_NONE));
    }
}


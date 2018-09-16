using System;
using UnityEngine;

public class Rabbit : AbstractEnemy
{
    private Animator animator;

    private EnemyController2D controllerMovement;
    private EnemyState state;

    private void Awake()
    {

        deathForceMultiplier = 10f;
        deathAdditionalTorque = 50f;
        maxHp = 5f;
        currentHp = maxHp;
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
            controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_LEFT));
        }
        else
        {
            controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_RIGHT));
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




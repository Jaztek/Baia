using System;
using System.Collections;
using UnityEngine;

public enum EnemyState { IDLE, MOVE, ATACK, EXTREMIS, NONE }

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : AbstractEnemy
{

    private EnemyController2D controllerMovement;
    private Animator animator;
    private EnemyState state;
    public float prepareAttackDelay = 3f;
    public float attackDelay = 0.5f;
    public GameObject redKnightDamage;
    public bool isReady = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        controllerMovement = GetComponent<EnemyController2D>();
    }

    private void Start()
    {
        state = EnemyState.IDLE;
    }

    public override void OnMove(GameObject player)
    {
        if (state == EnemyState.ATACK || state == EnemyState.NONE)
        {
            return;
        }

        animator.SetBool("isMov", true);
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

    public override void OnExtremis()
    {
        throw new NotImplementedException();
    }

    public override void OnIdle()
    {
        if (state == EnemyState.ATACK || state == EnemyState.NONE)
        {
            return;
        }

        animator.SetBool("isInRange", false);
        animator.SetBool("isMov", false);
        state = EnemyState.IDLE;
        controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_NONE));
    }

    public override void OnAttack()
    {
        if (state.Equals(EnemyState.NONE))
        {
            return;
        }
        if (!isReady)
        {
            state = EnemyState.ATACK;
            isReady = true;
            animator.SetBool("isInRange", true);
            state = EnemyState.NONE;
            StartCoroutine(ReadyAttack());
        }
        controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_NONE));
    }

    IEnumerator ReadyAttack()
    {
        yield return new WaitForSeconds(prepareAttackDelay);
        state = EnemyState.NONE;
        animator.SetBool("isInRange", false);
        animator.SetTrigger("attack");
        redKnightDamage.SetActive(true);
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(attackDelay);
        redKnightDamage.SetActive(false);
        isReady = false;
        state = EnemyState.IDLE;
        OnIdle();
    }
}

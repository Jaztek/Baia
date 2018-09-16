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
        if (state == EnemyState.MOVE || state == EnemyState.IDLE)
        {
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
    }
    public override void OnExtremis()
    {
        throw new NotImplementedException();
    }

    public override void OnIdle()
    {
        if (state == EnemyState.MOVE || state == EnemyState.IDLE)
        {
            animator.SetBool("isInRange", false);
            animator.SetBool("isMov", false);
            state = EnemyState.IDLE;
            controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_NONE));
        }
    }

    public override void OnAttack(GameObject player)
    {
        if (state != EnemyState.NONE)
        {

            OnMove(player);


            state = EnemyState.ATACK;
            animator.SetBool("isInRange", true);
            state = EnemyState.NONE;
            StartCoroutine(ReadyAttack());
            controllerMovement.AddIntent(new Intent(Intent.Type.H_MOVE_NONE));
        }
    }

    IEnumerator ReadyAttack()
    {
        yield return new WaitForSeconds(prepareAttackDelay);

        // una vez acabe la corrutina de "preparacion del ataque" activamos el
        // collider de ataque, gestionamos las animaciones y lanzamos la corrutina
        // de lo que dure el ataque.

        state = EnemyState.NONE;
        animator.SetTrigger("attack");
        animator.SetBool("isInRange", false);
        redKnightDamage.SetActive(true);
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        //esta corrutina indicara el tiempo que el ataque esta activo (haciendo daño)
        yield return new WaitForSeconds(attackDelay);

        //Una vez finalice, dejamos el enemigo en el estado incial,
        // se volveran a calcular los estados.

        redKnightDamage.SetActive(false);
        state = EnemyState.IDLE;
        OnIdle();
    }
}

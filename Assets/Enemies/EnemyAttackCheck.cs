using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackCheck : MonoBehaviour
{

    Enemy enemy;
    EnemyController2D cc;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        cc = GetComponentInParent<EnemyController2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            int xDirection = cc.IsFacingRight ? 1 : -1;

            GameController.GetInstance().ApplyDamage(
                transform.parent.gameObject,
                collider.gameObject,
                enemy.Damage,
                new Vector2(xDirection * 0.6f, 0.6f));
        }
    }
}

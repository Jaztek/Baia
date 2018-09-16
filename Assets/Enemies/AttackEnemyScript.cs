using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyScript : MonoBehaviour {

    private AbstractEnemy enemyParent;

    private void Start()
    {
        GameObject enemyObject = gameObject.transform.parent.gameObject;

        if (enemyObject)
        {
            enemyParent = enemyObject.GetComponent<AbstractEnemy>();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Debug.Log("al aqtaque");
        if (collision.CompareTag("Player"))
        {
            enemyParent.OnAttack(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enemyParent.OnIdle();
        }
    }
}

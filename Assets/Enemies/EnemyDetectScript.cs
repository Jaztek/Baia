using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectScript : MonoBehaviour
{

    // Use this for initialization
    private AbstractEnemy enemyParent;

    private void Start()
    {
        enemyParent = GetComponentInParent<AbstractEnemy>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            enemyParent.OnMove(collision.gameObject);
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

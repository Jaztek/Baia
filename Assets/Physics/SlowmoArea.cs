using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SlowmoArea : MonoBehaviour
{

    GameController gameController;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        Assert.IsNotNull(gameController);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameController.RequestStartSlowmo(gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameController.RequestStopSlowmo(gameObject.name);
        }
    }
}

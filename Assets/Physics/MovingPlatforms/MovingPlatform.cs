using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public float platformSpeed = 10f;
    public Transform[] path;

    Dictionary<int, Transform> itemsOnPlatform;

    int currentIndex = 0;
    Transform myTransform;
    Collider2D myCollider;

    private void Awake()
    {
        itemsOnPlatform = new Dictionary<int, Transform>();
        myCollider = GetComponent<Collider2D>();
        myTransform = transform;
    }

    void FixedUpdate()
    {
        if (path.Length == 0)
        {
            return;
        }

        Vector3 increment = Vector3.zero;

        Transform target = path[currentIndex];

        if (Mathf.Abs(myTransform.position.x - target.position.x) < 0.01f &&
            Mathf.Abs(myTransform.position.y - target.position.y) < 0.01f)
        {
            int length = path.Length;
            currentIndex = (currentIndex + 1) % length;
        }
        else
        {
            Vector2 currentPosition = myTransform.position;
            Vector2 targetPosition = path[currentIndex].position;

            Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, platformSpeed * Time.deltaTime);
            increment = newPosition - myTransform.position;
            myTransform.position = newPosition;
        }

        foreach (int key in itemsOnPlatform.Keys)
        {
            Transform t = itemsOnPlatform[key];
            if (myCollider.IsTouching(t.GetComponent<Collider2D>()))
            {
                t.Translate(increment);
            }
            else
            {
                itemsOnPlatform.Remove(t.gameObject.GetInstanceID());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        itemsOnPlatform.Remove(collision.gameObject.GetInstanceID());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        itemsOnPlatform.Add(collision.gameObject.GetInstanceID(), collision.transform);
    }
}

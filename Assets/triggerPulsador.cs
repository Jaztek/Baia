using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerPulsador : MonoBehaviour
{
    public GameObject puerta;
    Rigidbody2D rb2D;

    void Start()
    {
        rb2D = puerta.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        print("ENTER");
        Vector3 vector = new Vector3(rb2D.position.x, 0.5f, 0);
        StartCoroutine(SmoothMovement(vector));
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            print("sqrRemainingDistance= " + sqrRemainingDistance);
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, 1f);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }



}

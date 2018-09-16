using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    Transform myTransform;
    Transform targetTransform;

    void Start()
    {
        myTransform = transform;

        GameObject target = GameObject.FindGameObjectWithTag("Player");
        targetTransform = target.transform;
        myTransform.position = new Vector3(targetTransform.position.x, targetTransform.position.y, myTransform.position.z);
    }

    void FixedUpdate()
    {
        float x = Mathf.SmoothDamp(myTransform.position.x, targetTransform.position.x, ref velocity.x, smoothTime);
        float y = Mathf.SmoothDamp(myTransform.position.y, targetTransform.position.y, ref velocity.y, smoothTime);

        myTransform.position = new Vector3(x, y, myTransform.position.z);
    }
}

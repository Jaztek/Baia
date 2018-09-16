using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{

    public LayerMask layerMask;
    public float maxDrawDistance = 5f;

    Vector2 offset;
    Transform myTransform;
    Transform parent;

    BoxCollider2D parentBoxCollider;
    SpriteRenderer sr;
    SpriteRenderer parentSr;
    float bottomY;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        myTransform = transform;
        parent = transform.parent;
        offset = new Vector2(
            myTransform.position.x - parent.position.x,
            myTransform.position.y - parent.position.y);

        parentSr = parent.GetComponent<SpriteRenderer>();
        parentBoxCollider = parent.GetComponent<BoxCollider2D>();

        sr.flipX = parentSr.flipX;
        bottomY = parentBoxCollider.bounds.extents.y * parent.localScale.y;
    }

    void Update()
    {
        Debug.DrawRay(parent.position, maxDrawDistance * Vector2.down);

        RaycastHit2D raycastHit = Physics2D.Raycast(parent.position, Vector2.down, maxDrawDistance, layerMask);
        if (raycastHit)
        {
            Vector2 collisionPoint = raycastHit.point;
            float distance = raycastHit.distance;

            float angle = Vector2.Angle(raycastHit.normal, Vector2.right);
            if (Vector2.Angle(raycastHit.normal, Vector2.right) != 90)
            {
                transform.rotation = Quaternion.AngleAxis(90 + angle, Vector3.forward);
            } else
            {
                transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            }

            myTransform.localScale = new Vector3(
                // Mathf.Abs(bottomY / distance),
                1f,
                myTransform.localScale.y,
                myTransform.localScale.z);

            myTransform.position = new Vector3(
                parent.position.x,
                collisionPoint.y - 0.1f,
                myTransform.position.z);

            sr.sprite = parentSr.sprite;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Abs(offset.y) / distance);
            sr.enabled = true;
        }
        else
        {
            sr.enabled = false;
        }
    }
}

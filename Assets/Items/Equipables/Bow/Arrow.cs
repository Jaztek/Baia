using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    float damage = 1f;
    Rigidbody2D rb;

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 v = rb.velocity;
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Desactivamos las físicas de momento
            GameController.GetInstance().ApplyDamage(
                GameObject.FindGameObjectWithTag("Player"),
                collision.gameObject,
                damage,
                Vector2.zero);
        }

        Destroy(gameObject);
    }
}

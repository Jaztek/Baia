using UnityEngine;

public class AttackCheck : MonoBehaviour
{
    Player player;
    CharacterController2Dv2 characterController;

    private void Start()
    {
        player = GetComponentInParent<Player>();
        characterController = GetComponentInParent<CharacterController2Dv2>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("BAM!");
            int xDirection = characterController.FacingRight ? 1 : -1;
            GameController.GetInstance().ApplyDamage(transform.parent.gameObject, collider.gameObject, player.damage, new Vector2(xDirection * 0.6f, 0.6f));
        }
    }
}

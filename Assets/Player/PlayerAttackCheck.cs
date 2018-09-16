using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    Player player;
    CharacterController2D characterController;

    private void Start()
    {
        player = GetComponentInParent<Player>();
        characterController = GetComponentInParent<CharacterController2D>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            int xDirection = characterController.IsFacingRight ? 1 : -1;

            GameController.GetInstance().ApplyDamage(
                transform.parent.gameObject, 
                collider.gameObject, 
                player.damage, 
                new Vector2(xDirection * 0.6f, 0.6f));
        }
    }
}

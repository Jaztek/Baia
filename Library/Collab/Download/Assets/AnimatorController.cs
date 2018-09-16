using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAnim { IDLE, MOVE, JUMP }

public class AnimatorController : MonoBehaviour
{

    private CharacterController2D controller2D;
    Animator animator;
    PlayerAnim animCurrentState = PlayerAnim.IDLE;
    PlayerAnim animState = PlayerAnim.IDLE;

    // Use this for initialization
    void Start()
    {
        controller2D = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animState.Equals(animCurrentState))
        {
            //   return;
        }

        switch (animState)
        {
            case PlayerAnim.IDLE:
                animator.SetBool("isJumping", false);
                animator.SetBool("isMoving", false);
                break;

            case PlayerAnim.MOVE:
                animator.SetBool("isJumping", false);
                animator.SetBool("isMoving", true);
                break;

            case PlayerAnim.JUMP:
                animator.SetBool("isJumping", true);
                break;

        }

    }

    public void SetAnimation(PlayerAnim anim)
    {
        if (!controller2D.CanJump() || anim == PlayerAnim.JUMP)
        {
            animState = PlayerAnim.JUMP;
        }
        else if (anim == PlayerAnim.MOVE)
        {
            animState = PlayerAnim.MOVE;
        }
        else if (anim == PlayerAnim.IDLE)
        {
            animState = PlayerAnim.IDLE;
        }

    }
}

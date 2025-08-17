using UnityEngine;

public class EnemyBandit : EnemyBase
{
    [SerializeField] private Transform attackPoint;

    protected override void Start()
    {
        attackRange = 0.5f;

        base.Start();
    }

    protected override void Update()
    {
        //Get the direction the enemy needs to walk to get towards the player
        if (player.position.x < transform.position.x)
            horizontalMove = -moveSpeed;
        else if (player.position.x > transform.position.x)
            horizontalMove = moveSpeed;

        if (PlayerInRange() && IsGrounded() && !hurting && !dead && !attacking)
        {
            attacking = true;
            attackTimer = 0f - attackDelay;
            Invoke("AttackAnim", attackDelay);
        }

        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    //So this is a tad weird, this function gets called at a specific frame in the animation due to horribly mistiming.
    //The damage tick and animation were not alligned at all, therefore we call this funciton from animation and only set
    //the animation trigger in update func, this also helps with playing the sounds at correct sound to match the animation;
    protected override void Attack()
    {
        FindObjectOfType<AudioManager>().Play("EnemySwordSwoosh", 1.4f, 1.6f);

        //Detect player collider
        Collider2D playerHit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerMask);

        if (playerHit != null)
            playerHit.GetComponent<PlayerController>().TakeDamage(attackDamage);

    }

    bool PlayerInRange()
    {
        Collider2D playerHit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerMask);
        return playerHit != null;
    }

    protected override void GetAnimClipTimes(Animator animator)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip animClip in clips)
        {
            switch (animClip.name)
            {
                case "LightBandit_Death":
                    deathDuration = animClip.length;
                    break;
                case "LightBandit_Hurt":
                    hurtDuration = animClip.length;
                    break;
                case "LightBandit_Attack":
                    attackDuration = animClip.length;
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

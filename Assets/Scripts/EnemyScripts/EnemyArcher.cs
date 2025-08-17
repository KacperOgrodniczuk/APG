using UnityEngine;

public class EnemyArcher : EnemyBase
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform arrowSpawn;

    private float stoppingDistance = 4f;

    protected override void Start()
    {
        attackRange = 10f;
        facingRight = true;

        base.Start();
    }

    protected override void Update()
    {
        //We only care about the x and not the y's so set new vec2s
        Vector2 thisX = new Vector2(transform.position.x, 0);
        Vector2 playerX = new Vector2(player.position.x, 0);

        if (Vector2.Distance(thisX, playerX) > stoppingDistance)
        {
            if (player.position.x < transform.position.x)
                horizontalMove = -moveSpeed;
            else if (player.position.x > transform.position.x)
                horizontalMove = moveSpeed;
        }
        else if (Vector2.Distance(thisX, playerX) < stoppingDistance)
        {
            if (thisX.x > playerX.x && facingRight && !attacking) Flip();
            else if (thisX.x < playerX.x && !facingRight && !attacking) Flip();
            horizontalMove = 0;
        }

        if (PlayerInLineOfSight() && IsGrounded() && !hurting && !dead && !attacking)
        {
            attacking = true;
            attackTimer = 0f - attackDelay;
            Invoke("AttackAnim", attackDelay);
        }

        base.Update();

        if (horizontalMove == 0)
            jump = false;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    //needs to be finished off after the movement is sorted out
    protected override void Attack()
    {
        GameObject arro;
        //if the local scale is above 0 (positive 1) shoot right as the archer is looking right
        if (transform.localScale.x > 0)
            arro = GameObject.Instantiate(arrow, arrowSpawn.position, Quaternion.identity);
        else if (transform.localScale.x < 0)
            arro = GameObject.Instantiate(arrow, arrowSpawn.position, Quaternion.Euler(new Vector3(0,0,180)));
        //if the local scale is below 0 (negative 1) shoot left as the archer is looking left

        FindObjectOfType<AudioManager>().Play("BowString", 1.1f, 1.3f);
    }

    bool PlayerInLineOfSight()
    {
        Vector2 castOrigin = new Vector2(transform.position.x, transform.position.y + 1.1f);
        RaycastHit2D playerHit = Physics2D.Raycast(castOrigin, Vector2.right * transform.localScale.x, attackRange, playerMask);
        Debug.DrawRay(castOrigin, Vector2.right * transform.localScale.x, Color.blue);
        return playerHit.collider != null;
    }

    protected override void GetAnimClipTimes(Animator animator)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip animClip in clips)
        {
            switch (animClip.name)
            {
                case "Archer_Death":
                    deathDuration = animClip.length;
                    break;
                case "Archer_Hurt":
                    hurtDuration = animClip.length;
                    break;
                case "Archer_Attack1":
                    attackDuration = animClip.length;
                    break;
            }
        }
    }
}

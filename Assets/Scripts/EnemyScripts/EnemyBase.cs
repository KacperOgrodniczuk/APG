using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] LayerMask groundMask;
    [SerializeField] protected LayerMask playerMask;

    protected Transform player;
    Animator anim;
    Rigidbody2D rb2d;
    BoxCollider2D bc2d;
    EnvironmentGenerator envGen;

    AudioManager audioManager;

    float maxHealth = 100;
    float currentHealth;

    protected float horizontalMove;
    public static float defaultMoveSpeed;
    public static float moveSpeed;
    float moveSmoothing = 0.05f;
    protected bool facingRight = false;

    float jumpForce = 250f;
    protected bool jump = false;

    protected bool attacking = false;
    public static float defaultAttackDamage;
    public static float attackDamage;
    protected float attackTimer = 0f;
    protected float attackDuration;
    public static float defaultAttackDelay;
    public static float attackDelay;
    protected float attackRange;

    float hurtTimer = 0;
    protected float hurtDuration;
    protected bool hurting = false;

    protected float deathDuration;
    protected bool dead = false;

    Vector3 velocity = Vector3.zero;

    public static void StartingValues()     //This function get's called in the start function of enemyspawner to make sure enemy stats are reset upon scene reload when restarting the game after player death
    {
        defaultMoveSpeed = 10f;
        defaultAttackDamage = 10f;
        defaultAttackDelay = 0.5f;
    }

    public static void DefaultValues()
    {
        moveSpeed = defaultMoveSpeed;
        attackDamage = defaultAttackDamage;
        attackDelay = defaultAttackDelay;
    }

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        bc2d = GetComponent<BoxCollider2D>();
        envGen = GameObject.FindWithTag("envGen").GetComponent<EnvironmentGenerator>();

        audioManager = FindObjectOfType<AudioManager>();

        currentHealth = maxHealth;
        StartingValues();
        DefaultValues();
        GetAnimClipTimes(anim);
    }

    protected virtual void Update() 
    {

        //If ground ahead jump up
        if (ObstacleAhead() && IsGrounded() && !dead && !hurting && !attacking)
            jump = true;

        //if enemy dies cancel invoke so no attacks are performed after death
        if (dead)
            CancelInvoke();

        //attack condition and timer
        attackTimer += Time.deltaTime;
        if (attackTimer > attackDuration)
            attacking = false;

        //hurt timer
        hurtTimer += Time.deltaTime;
        if (hurtTimer > hurtDuration)
            hurting = false;

        //Set animation parameters for the jump veloicty and if touching the ground
        anim.SetFloat("AirSpeed", rb2d.velocity.y);
        anim.SetBool("Grounded", IsGrounded());

        //running animation
        if (rb2d.velocity != Vector2.zero)
            anim.SetInteger("AnimState", 2);
        else
            anim.SetInteger("AnimState", 1);
    }

    protected virtual void FixedUpdate() {
        Move(horizontalMove * Time.deltaTime, jump, hurting, dead, attacking);
        jump = false;
    }

    protected void Move(float horMove, bool jump, bool hurting, bool dead, bool attacking) 
    {
        if (!hurting && !dead && !attacking)
        {
            Vector3 targetVelocity = new Vector2(horMove * 10f, rb2d.velocity.y);
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, moveSmoothing);
            if (horMove > 0 && !facingRight)
                Flip();
            else if (horMove < 0 && facingRight)
                Flip();
        }

        if (attacking || hurting || dead)
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);

        if (jump)
            Jump();
    }

    //Funciton for calling/invoking attack animation
    protected void AttackAnim() {
        anim.SetTrigger("Attack");
    }

    //Function for attacking, seperate from animation as this is called form an animation at a specific frame
    protected virtual void Attack() { }

    //take damage
    public void TakeDamage(float damage) 
    { 
        if (!dead)
        {
            horizontalMove = 0;
            hurting = true;
            hurtTimer = 0f;
            currentHealth -= damage;

            //Play hurt animation and play sound
            anim.SetTrigger("Hurt");
            audioManager.Play("TakeDamage", 0.8f, 1f);
            //If die invoke  die funciton
            if (currentHealth <= 0)
                Invoke("Die", hurtDuration);
        }
    }

    protected void Die()
    {
        dead = true;

        //Die animation
        anim.SetBool("IsDead", dead);
        anim.SetTrigger("Death");

        //Destroy enemy
        Destroy(gameObject, deathDuration + hurtDuration);
    }

    protected void OnDestroy()
    {
        EnemySpawner.enemiesKilled++;
        if(EnemySpawner.enemiesKilled == EnemySpawner.enemiesToSpawn)
        {
            EnemySpawner.enemiesKilled = 0;
            envGen.OpenRoom(envGen.ReturnNextOffset());
        }
    }

    //Jump function...
    protected void Jump()
    {
        rb2d.AddForce(new Vector2(0f, jumpForce));
        anim.SetTrigger("Jump");
        audioManager.Play("EnemyJump", 0.9f, 1.1f);
    }

    //Flip the sprite...
    protected void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public static void IncreaseDamage(float value)
    {
        if (attackDamage + value < 2)
        {
            attackDamage = 2;
            defaultAttackDamage = 2;
        }
        else
        {
            attackDamage += value;
            defaultAttackDamage += value;
        }
    }

    public static void IncreaseAttackDelay(float value)
    {
        if (attackDelay + value < 0.2f)
        {
            attackDelay = 0.2f;
            defaultAttackDelay = 0.2f;
        }
        else
        {
            attackDelay += value;
            defaultAttackDelay += value;
        }
    }

    //check if there's some ground in front.
    protected bool ObstacleAhead()
    {
        Vector2 boxCastSize = new Vector2(bc2d.bounds.size.x, bc2d.bounds.size.y * 0.9f);
        RaycastHit2D castHit = DrawAndReturnBoxCast2D(bc2d.bounds.center, boxCastSize, 0f, Vector2.right, 0.5f, groundMask, Color.black);
        return castHit.collider != null;
    }

    //Check if the enemy is touching the ground
    protected bool IsGrounded()
    {
        //Adjusted size for boxcast, since we don't want it to be able to jump off walls.
        Vector2 boxCastSize = new Vector2(bc2d.bounds.size.x * 0.9f, bc2d.bounds.size.y);
        RaycastHit2D castHit = DrawAndReturnBoxCast2D(bc2d.bounds.center, boxCastSize, 0f, Vector2.down, 0.05f, groundMask, Color.red);
        return castHit.collider != null;
    }

    //Makeshift function to easily return a boxcast and draw it as the same time
    protected RaycastHit2D DrawAndReturnBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, Color color)
    {
        //99% sure this is correct, at least seems like it so imma leave it like this for now, prettymuch use the origin of the cast, add the size divided by 2
        //Once the lines for the box are drawn add the direciton times the distance to create the offset;
        Vector2 offset = direction * distance * transform.localScale.x;
        Debug.DrawLine(new Vector2(origin.x + (size.x / 2), origin.y + (size.y / 2)) + offset, new Vector2(origin.x + (size.x / 2), origin.y - (size.y / 2)) + offset, color);
        Debug.DrawLine(new Vector2(origin.x + (size.x / 2), origin.y - (size.y / 2)) + offset, new Vector2(origin.x - (size.x / 2), origin.y - (size.y / 2)) + offset, color);
        Debug.DrawLine(new Vector2(origin.x - (size.x / 2), origin.y - (size.y / 2)) + offset, new Vector2(origin.x - (size.x / 2), origin.y + (size.y / 2)) + offset, color);
        Debug.DrawLine(new Vector2(origin.x - (size.x / 2), origin.y + (size.y / 2)) + offset, new Vector2(origin.x + (size.x / 2), origin.y + (size.y / 2)) + offset, color);

        return Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask);
    }

    //Base virtual function for animation clips.
    protected virtual void GetAnimClipTimes(Animator animator)
    {}

}

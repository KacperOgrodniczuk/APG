using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] HealthBar healthBar;
    [SerializeField] BlockTimer blockBar;
    [SerializeField] PlayerSpeech playerSpeech;

    Animator anim;
    Rigidbody2D rb2d;
    BoxCollider2D bc2d;

    [SerializeField] Transform attackPoint;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask enemyMask;
    [SerializeField] GameObject deathUI;

    float maxHealth = 100;
    float currentHealth;

    public float defaultMoveSpeed = 25f;
    public float moveSpeed = 25f;
    float moveSmoothing = 0.05f;

    float horizontalMove;
    bool facingRight = true;

    bool jump = false;
    bool rolling = false;
    float jumpForce = 333f;

    public float defaultRollDistance = 25f;
    public float rollDistance = 25f;
    float rollTimer = 0f;
    float rollDuration = 0f;

    bool attacking = false;
    float attackTimer = 0f;
    float[] defaultAttackDuration = new float[4];
    float[] attackDuration = new float[4];
    int attackAnim = 0;

    //float defaultAttackRange = 0.5f;
    float attackRange = 0.5f;
    public float defaultAttackDamage = 40f;
    public float attackDamage = 40f;

    bool hurting = false;
    float hurtTimer = 0f;
    float hurtDuration;
    bool dead = false;

    bool blocking = false;
    float defaultBlockDuration = 0f;
    float blockDuration = 0f;
    float blockCooldown = 3f;
    float blockTimer = 0f;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {

        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        bc2d = GetComponent<BoxCollider2D>();

        GetAnimClipTimes(anim);

        for (int i = 0; i < defaultAttackDuration.Length; i++)
        {
            attackDuration[i] = defaultAttackDuration[i];
        }

        blockDuration = defaultBlockDuration;
        blockBar.SetMaxTime(blockCooldown);

        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    //Function to reset the stats back to default
    public void DefaultStats()
    {
        attackDamage = defaultAttackDamage;
        moveSpeed = defaultMoveSpeed;
        rollDistance = defaultRollDistance;
        if (anim.GetFloat("BlockSpeed") != 1f)
        {
            anim.SetFloat("BlockSpeed", 1f);
            blockDuration = defaultBlockDuration;
        }
        if (anim.GetFloat("AttackSpeed") != 1f)
        {
            anim.SetFloat("AttackSpeed", 1f);
            for (int i = 0; i < attackDuration.Length; i++)
            {
                attackDuration[i] = defaultAttackDuration[i];
            }
        }
    }

    public void IncreaseBlockDuration()
    {
        anim.SetFloat("BlockSpeed", 0.75f); // block speed is reduced so the duration is increased;
        blockDuration *= 1.5f;
    }

    public void DecreaseBlockDuration()
    {
        anim.SetFloat("BlockSpeed", 1.5f); // block speed is increased so the duration is reduced
        blockDuration *= 0.75f;
    }

    public void IncreaseAttackSpeed()
    {
        anim.SetFloat("AttackSpeed", 1.5f);
        for (int i = 0; i < attackDuration.Length; i++)      //attack speed is increased so the attack duration is reduced;
        {
            attackDuration[i] *= 0.75f;
        }
    }

    public void DecreaseAttackSpeed()
    {
        anim.SetFloat("AttackSpeed", 0.75f); //attack speed is reduced so the duration is increased
        for (int i = 0; i < attackDuration.Length; i++)
        {
            attackDuration[i] *= 1.5f;
        }
    }

    //I tried my best to keep animations in update and actual movement in fixed update
    private void Update()
    {
        //Handle the control inputs in update and then pass it to be handled properly in fixed update
        //Deal with input for controls
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;

        //jumping
        if (Input.GetButton("Jump") && IsGrounded())
            jump = true;

        //Handle rolling
        if (Input.GetButton("Roll") && !rolling && !attacking && !blocking)
        {
            rolling = true;
            anim.SetTrigger("Roll");
            FindObjectOfType<AudioManager>().Play("Roll", 1.9f, 2.1f);
            rollTimer = 0f;
        }

        rollTimer += Time.deltaTime;
        if (rollTimer > rollDuration)
            rolling = false;

        //Handle Block
        //There is a block timer > cooldown check. This is done as the block cooldown will be higher than the animation length so that players can't just spam it.
        if (Input.GetButton("Block") && !blocking && blockTimer > blockCooldown)
        {
            blocking = true;
            anim.SetTrigger("Block");
            FindObjectOfType<AudioManager>().Play("ShieldUp");
            blockTimer = 0f;
        }

        blockTimer += Time.deltaTime;
        if (blockTimer > blockDuration)
            blocking = false;

        if (blockTimer < blockCooldown)
            blockBar.SetTime(blockTimer);

        //Handle attack
        if (Input.GetButton("Attack") && !attacking && !rolling && !blocking)
        {
            attackAnim++;
            if (attackAnim > 3)
                attackAnim = 1;

            //Reset the attack animation back to the first attack after a few seconds of now attacking;
            if (attackTimer > attackDuration[attackAnim] * 3f)
                attackAnim = 1;

            //call the attack function
            Attack(attackAnim);
        }

        attackTimer += Time.deltaTime;
        if (attackTimer > attackDuration[attackAnim])
            attacking = false;

        hurtTimer += Time.deltaTime;
        if (hurtTimer > hurtDuration)
            hurting = false;

        //Animation variables, is the character touching the ground and what is the y velocity, used for falling.
        anim.SetFloat("AirSpeedY", rb2d.velocity.y);
        anim.SetBool("Grounded", IsGrounded());

        if (horizontalMove != 0)
            anim.SetInteger("AnimState", 1);
        else
            anim.SetInteger("AnimState", 0);

        if (Input.GetKeyDown(KeyCode.F12))
        {
            var numberOfGameModes = System.Enum.GetValues(typeof(GameMode)).Length;
            TwitchChat.gamemode += 1;
            if ((int)TwitchChat.gamemode == numberOfGameModes) TwitchChat.gamemode = 0;
        }
    }

    //Deal with all the movement in fixed update
    private void FixedUpdate()
    {
        Move(horizontalMove * Time.fixedDeltaTime, jump, rolling, attacking, blocking, hurting, dead);
        jump = false;
    }

    //Function that deals with all the movement
    private void Move(float horMove, bool jump, bool rolling, bool attacking, bool blocking, bool hurting, bool dead)
    {

        //If not rolling, attacking or blocking allow movement.
        if (!rolling && !attacking && !blocking && !hurting && !dead)
        {
            Vector3 targetVelocity = new Vector2(horMove * 10f, rb2d.velocity.y);
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, moveSmoothing);

            //If there's input for movement and sprite is facing wrong way flip it
            if (horMove > 0 && !facingRight)
            {
                Flip();
                healthBar.Flip();
                blockBar.Flip();
                playerSpeech.Flip();
            }
            else if (horMove < 0 && facingRight)
            {
                Flip();
                healthBar.Flip();
                blockBar.Flip();
                playerSpeech.Flip();
            }
        }

        //rolling
        if (rolling)
        {
            //Roll base on the direciton the sprite is oriented in. Times by ten because that's what i do with
            //movement speed too, and also timed by time delta time
            Vector3 targetVelocity = new Vector2(transform.localScale.x * rollDistance * Time.deltaTime * 10f, rb2d.velocity.y);
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, moveSmoothing);
        }

        //if blocking or hurting set the velocity to zero
        if (blocking || hurting)
            rb2d.velocity = Vector2.zero;

        //if attacking set the x velocity to 0 and keep y velocity
        if (attacking)
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        //Jumping
        if (jump)
            Jump();
    }

    private void Attack(int attackAnim)
    {
        attacking = true;
        attackTimer = 0f;
        //Play the attack animation and sound
        anim.SetTrigger("Attack" + attackAnim);
        FindObjectOfType<AudioManager>().Play("PlayerSwordSwoosh", 1.4f, 1.6f);

        //Detect all enemies hit
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyMask);

        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<EnemyBase>().TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage, bool ignoreBlock = false)
    {
        if (hurting || dead)    //if already hurting or dead return
            return;

        if (!ignoreBlock && blocking)   //if not ignoring blocking and blocking return
        {
            FindObjectOfType<AudioManager>().Play("ShieldHit", 1.4f, 1.6f);
            return;
        }

        horizontalMove = 0;
        hurting = true;
        hurtTimer = 0f;
        currentHealth -= damage;

        healthBar.SetHealth(currentHealth);

        //Play the hurt animation
        anim.SetTrigger("Hurt");

        FindObjectOfType<AudioManager>().Play("PlayerTakeDamage", 1.4f, 1.6f);

        if (currentHealth <= 0)
            Die();

    }

    public void Heal(float heal)
    {
        if (currentHealth + heal > maxHealth) currentHealth = maxHealth;    //If current health plus heal is equal to more than max health set health to max health
        else currentHealth += heal;                                         //else just add heal to current health
        healthBar.SetHealth(currentHealth);
    }

    public void IncreaseMaxHealth(float value)  //increase max heatlth by value, check if curreny health is more than max health in case the value provided is negative thuus reducing max health
    {
        if (maxHealth + value < 1) maxHealth = 1;   //This is for cases where the player loses max health (passed value is negative)
        else  maxHealth += value;

        if (currentHealth > maxHealth) currentHealth = maxHealth;   // if current health is more than max health set current health to max health

        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    public void IncreaseMoveSpeed(float value)
    {
        if (defaultMoveSpeed + value < 10)
        {
            moveSpeed = 10;
            defaultMoveSpeed = 10;
        }
        else
        {
            moveSpeed += value;
            defaultMoveSpeed += value;
        }
    }

    public void IncreaseDamage(float value)
    {
        if (defaultAttackDamage + value < 10)
        {
            attackDamage = 10;
            defaultAttackDamage = 10;
        }
        else
        {
            attackDamage += value;
            defaultAttackDamage += value;
        }
    }

    private void Die()
    {
        dead = true;

        anim.SetTrigger("Death");
        rb2d.velocity = Vector2.zero;
        deathUI.SetActive(true);
        GetComponent<PlayerController>().enabled = false;
    }

    private void Jump()
    {
        rb2d.AddForce(new Vector2(0f, jumpForce));
        anim.SetTrigger("Jump");
        FindObjectOfType<AudioManager>().Play("PlayerJump", 0.9f, 1.1f);
    }

    //flip the sprite
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 locScale = transform.localScale;
        locScale.x *= -1;
        transform.localScale = locScale;
    }

    //Check if the player is touching the ground
    private bool IsGrounded()
    {
        //Adjusted size for boxcast, since we don't want it to be able to jump off walls.
        Vector2 boxCastSize = new Vector2(bc2d.bounds.size.x * 0.9f, bc2d.bounds.size.y);
        RaycastHit2D castHit = DrawAndReturnBoxCast2D(bc2d.bounds.center, boxCastSize, 0f, Vector2.down, 0.05f, groundMask);
        return castHit.collider != null;
    }

    //Makeshift function to easily return a boxcast and draw it as the same time
    private RaycastHit2D DrawAndReturnBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask)
    {
        //99% sure this is correct, at least seems like it so imma leave it like this for now, prettymuch use the origin of the cast, add the size divided by 2
        //Once the lines for the box are drawn add the direciton times the distance to create the offset;
        Vector2 offset = direction * distance;
        Debug.DrawLine(new Vector2(origin.x + (size.x / 2), origin.y + (size.y / 2)) + offset, new Vector2(origin.x + (size.x / 2), origin.y - (size.y / 2)) + offset, Color.red);
        Debug.DrawLine(new Vector2(origin.x + (size.x / 2), origin.y - (size.y / 2)) + offset, new Vector2(origin.x - (size.x / 2), origin.y - (size.y / 2)) + offset, Color.red);
        Debug.DrawLine(new Vector2(origin.x - (size.x / 2), origin.y - (size.y / 2)) + offset, new Vector2(origin.x - (size.x / 2), origin.y + (size.y / 2)) + offset, Color.red);
        Debug.DrawLine(new Vector2(origin.x - (size.x / 2), origin.y + (size.y / 2)) + offset, new Vector2(origin.x + (size.x / 2), origin.y + (size.y / 2)) + offset, Color.red);

        return Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask);
    }

    //A funciton to obtain the length of desired animations in seconds
    //Will use this for matching animation speed with player statistics (i.e attacks with attackspeed)
    private void GetAnimClipTimes(Animator animator)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip animClip in clips)
        {
            switch (animClip.name)
            {
                case "HeroKnight_Roll":
                    rollDuration = animClip.length;
                    break;
                //Array starting from 1 and not 0 is intentional, because of the naming convention for attack
                //animations and how the triggers are called.
                case "HeroKnight_Attack1":
                    defaultAttackDuration[0] = animClip.length;
                    defaultAttackDuration[1] = animClip.length;
                    break;
                case "HeroKnight_Attack2":
                    defaultAttackDuration[2] = animClip.length;
                    break;
                case "HeroKnight_Attack3":
                    defaultAttackDuration[3] = animClip.length;
                    break;
                case "HeroKnight_Block":
                    defaultBlockDuration = animClip.length;
                    break;
                case "HeroKnight_Hurt":
                    hurtDuration = animClip.length;
                    break;
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

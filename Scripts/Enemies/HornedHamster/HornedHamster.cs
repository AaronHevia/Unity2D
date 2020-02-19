using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HornedHamster : MonoBehaviour
{
    private enum State
    {
        Moving,
        Knockback,
        Dead
    }

    private State currentState;

    private bool groundDetected;
    private bool wallDetected;
    private PlayerMovementControls playerController;
    private Animator aliveAnim;
    private GameObject alive;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatArePlatforms;

    private Rigidbody2D aliveRb2D;
    [SerializeField] private float movementSpeed;
    
    [SerializeField] private float maxHealth;
    private float currentHealth;

    private float knockbackStartTime;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Vector2 knockbackSpeed;
    private int damageDirection;


    private Vector2 movement;

    private int enemyFacingDirection = 1;
    private int playerFacingDirection;

    [SerializeField] private GameObject hitParticle;
    [SerializeField] private GameObject chunkParticle;
    [SerializeField] private GameObject bloodParticle;

    [SerializeField] private Transform touchDamageCheck;    
    [SerializeField] private float touchDamageCooldown;
    [SerializeField] private float touchDamage;
    [SerializeField] private float touchDamageWidth;
    [SerializeField] private float touchDamageHeight;
    [SerializeField] private LayerMask whatIsPlayer;
    private Vector2 touchDamageBotLeft;
    private Vector2 touchDamageTopRight;
    private float lastTouchDamageTime;
    private float[] attackDetails = new float[2];


    private void Awake()
    {
        alive = transform.Find("Alive").gameObject;
        aliveRb2D = alive.GetComponent<Rigidbody2D>();
        aliveAnim = alive.GetComponent<Animator>();
        playerController = GameObject.Find("Player").GetComponent<PlayerMovementControls>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }
    private void Update()
    {
        switch (currentState)
        {
            case State.Moving:
                UpdateMovingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    // ---MOVING STATE---
    private void EnterMovingState()
    {

    }

    private void UpdateMovingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatArePlatforms);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatArePlatforms);

        CheckTouchDamage();

        if (!groundDetected || wallDetected)
        {
            // Flip Enemy
            Flip();
        }
        else
        {
            // Move Enemy
            movement.Set(movementSpeed * enemyFacingDirection, aliveRb2D.velocity.y);
            aliveRb2D.velocity = movement;
        }
    }

    private void ExitMovingState()
    {

    }

    // ---KNOCKBACK STATE---
    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time;
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb2D.velocity = movement;
        aliveAnim.SetBool("knockback", true);
    }

    private void UpdateKnockbackState()
    {
        if (Time.time >= knockbackStartTime + knockbackDuration)
            SwitchState(State.Moving);
    }

    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("knockback", false);
    }

    // ---DEAD STATE---
    private void EnterDeadState()
    {        
        Instantiate(chunkParticle, alive.transform.position, chunkParticle.transform.rotation);
        Instantiate(bloodParticle, alive.transform.position, bloodParticle.transform.rotation);
        Destroy(gameObject);
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // ---OTHER FUNCTIONS---
    private void SwitchState (State state)
    {
        switch (currentState)
        {
            case State.Moving:
                ExitMovingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case State.Moving:
                EnterMovingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
        }

        currentState = state;
    }

    private void Flip()
    {
        enemyFacingDirection *= -1;
        alive.transform.Rotate(0f, 180f, 0f);
    }

    private void Damage(float attackDamage)     // Damage received.
    {
        currentHealth -= attackDamage;
        playerFacingDirection = playerController.GetFacingDirection();

        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if (playerFacingDirection == 1)        
            damageDirection = 1;
            /*playerOnLeft = true;*/        
        else        
            damageDirection = -1;
            /*playerOnLeft = false;*/
                
        if (currentHealth > 0f)
            SwitchState(State.Knockback);
        else if (currentHealth < 0f)
            SwitchState(State.Dead);        
    }

    private void CheckTouchDamage()
    {
        if (Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {
            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);

            if (hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = alive.transform.position.x;
                hit.SendMessage("Damage", attackDetails);

            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));

        Vector2 botLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 botRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

        Gizmos.DrawLine(botLeft, botRight);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(botLeft, topLeft);
        Gizmos.DrawLine(topRight, botRight);
    }
}

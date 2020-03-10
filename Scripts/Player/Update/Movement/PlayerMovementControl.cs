using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovementControl : MonoBehaviour
{    
    private Rigidbody2D rb2d;
    private Animator anim;    
    [SerializeField] private LayerMask whatArePlatforms;
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform groundCheck;    
    [SerializeField] private CapsuleCollider2D standingCollider;
    [SerializeField] private CapsuleCollider2D crouchingCollider;    
    [SerializeField] private float airSpeed = 0.95f;
    [SerializeField] private float runSpeed = 12.0f; 
    [SerializeField] private float rollSpeed = 3.0f;
    [SerializeField] private float rollTime = 0.3f;
    [SerializeField] private float rollCoolDown = 1.0f;
    [SerializeField] private float dashTime = 0.4f;               
    [SerializeField] private float dashSpeed = 30.0f;
    [SerializeField] private float dashCoolDown = 2.0f;
    [SerializeField] private float distanceBetweenImages = 0.05f;
    [SerializeField] private float wallSlideSpeed = 2.0f;
    [SerializeField] private float climbSpeed = 4.0f;
    [SerializeField] private Vector2 wallJumpVector = new Vector2(0.8f, 1.0f);
    [SerializeField] private float wallJumpForce = 20.0f;
    [SerializeField] private float ledgeClimbXOffset1 = 0.25f;
    [SerializeField] private float ledgeClimbYOffset1 = 0.5f;
    [SerializeField] private float ledgeClimbXOffset2 = 0.5f;
    [SerializeField] private float ledgeClimbYOffset2 = 2.0f;
    
    private Vector2 ledgePositionBottom;
    private Vector2 ledgePosition1;
    private Vector2 ledgePosition2;    
               
    private readonly float checkRadius = 0.195f;
    private readonly float wallCheckDistance = 0.24f;
    private readonly float gravityScale = 1.0f;
    private readonly float jumpForce = 20f;                         // *** In order to determine jump force decide on max jump height and how long it takes to reach jump height.  Gravity is -50.  Calculated using maxjumpheight = (gravity * timeToJump^2)/2.  Max jump height is 4 and time to jump is .4s. *** //
    private readonly float variableJumpMultiplier = 2.0f;
    private readonly float fallMultiplier = 2.5f;
    private float rollTimeLeft = 0.0f;
    private float dashTimeLeft = 0.0f;
    private float lastRoll = -100.0f;
    private float lastDash = -100.0f;
    private float lastImageXPosition;    

    private readonly int amountOfJumps = 2;        
    private int facingDirection = 1;
    private int wallJumpDirection;
    private int horizontalInputDirection;
    private int verticalInputDirection;    
    private int amountOfJumpsRemaining;
        
    private bool isTouchingCeiling;
    private bool isTouchingWall;    
    private bool isTouchingGround;
    private bool isFacingRight;
    private bool canMove;
    private bool canFlip;
    private bool canJump;
    private bool canWallJump;    
    private bool ledgeDetected;
    private bool isTouchingLedge;    
    private bool isRunning;    
    private bool isCrouching;    
    private bool isRolling;    
    private bool isDashing;    
    private bool isWallSliding;    
    private bool isWallGrabbing;    
    private bool isWallClimbing;
    private bool isJumping;
    private bool isWallJumping;
    private bool isFirstJump;
    private bool isClimbingLedge;

    bool crouch;
    bool roll;
    bool dash;
    bool wallGrab;
    bool wallClimb;
    bool jump;
    bool wallJump;    
    
    private void Awake()
    {        
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {        
        rb2d.gravityScale = gravityScale;        
        isFacingRight = true;        
        canFlip = true;        
        standingCollider.enabled = true;
        crouchingCollider.enabled = false;
        rollTimeLeft = rollTime;
        isClimbingLedge = false;
        ResetJumpCount();
    }

    private void Update()
    {
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        PlayerInput();
        CheckSurroundings();
        FlipCharacter();
        UpdateAnimations();
        Crouch();
        JumpValidation();
        Teleport();        
    }

    private void FixedUpdate()
    {
        Move();
        Roll();        
        Dash();
        WallSlide();
        WallGrab();
        WallClimb();
        Jump();
        BetterJump();
        WallJump();
    }

    private void PlayerInput()
    {
        horizontalInputDirection = (int)Input.GetAxisRaw("Horizontal");        
        verticalInputDirection = (int)Input.GetAxisRaw("Vertical");
        
        if (Input.GetButton("Horizontal"))
        {
            Move();
        }
        
        if (isTouchingGround && verticalInputDirection == -1)
        {
            crouch = true;            
        }        
        
        if (Input.GetButtonDown("Dash"))
        {
            if (Time.time >= lastDash + dashCoolDown)
            {
                dash = true;
            }
        }

        if (Input.GetButton("WallGrab"))
        {
            if (isTouchingWall)
            {
                wallGrab = true;
            }
        }

        if (Input.GetButton("WallGrab") && Input.GetButton("Vertical"))
        {
            if (isTouchingWall)
            {
                wallClimb = true;
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isTouchingGround && verticalInputDirection == -1)
            {
                if (Time.time >= lastRoll + rollCoolDown)
                    roll = true;
            }
            else if (isTouchingGround || amountOfJumpsRemaining > 0 && !isTouchingWall)
            {
                jump = true;
            }
            else if (isTouchingWall)
            {                
                wallJump = true;                
            }
        }
    }

    private void CheckSurroundings()
    {
        isTouchingCeiling = Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, whatArePlatforms);
        
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatArePlatforms);
        
        isTouchingGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatArePlatforms);       
        
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatArePlatforms);        
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(ceilingCheck.position, checkRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);        
    }

    private void Move()
    {
        if (!isTouchingGround && horizontalInputDirection == 0)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x * airSpeed, rb2d.velocity.y);
        }
        else if (canMove && !isTouchingWall)
        {
            rb2d.velocity = new Vector2(runSpeed * horizontalInputDirection, rb2d.velocity.y);
        }
    }

    private void Crouch()
    {
        if (crouch || (isTouchingCeiling && isTouchingGround))
        {
            rb2d.velocity = Vector2.zero;
            canMove = false;
            canJump = false;
            isCrouching = true;
            crouch = false;
        }
        else
        {
            isCrouching = false;
            canMove = true;
        }
        
        if (!isTouchingGround)
        {
            standingCollider.enabled = true;
            crouchingCollider.enabled = false;
        }
        else
        {
            if (isCrouching)
            {
                standingCollider.enabled = false;
                crouchingCollider.enabled = true;
            }
            else
            {
                standingCollider.enabled = true;
                crouchingCollider.enabled = false;
            }
        }        
    }

    private void Roll()
    {
        if (roll)
        {            
            isRolling = true;          
            rollTimeLeft = rollTime;
            lastRoll = Time.time;
            roll = false;
        }

        if (isRolling)
        {
            if (rollTimeLeft > 0)
            {                
                canFlip = false;
                rb2d.velocity = new Vector2(rollSpeed * facingDirection, 0.0f);
                rollTimeLeft -= Time.deltaTime;
            }

            if (rollTimeLeft <= 0 || isTouchingWall)
            {                
                isRolling = false;                
                canFlip = true;                
            }
        }        
    }

    private void Dash()         // *** Look for different dash animation to make it an attack. *** // *** Can damage. *** //
    {
        if (dash)
        {
            isDashing = true;
            dashTimeLeft = dashTime;
            lastDash = Time.time;
            PlayerAfterImagePool.Instance.GetFromPool();
            lastImageXPosition = transform.position.x;
            dash = false;
        }

        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                rb2d.velocity = new Vector2(dashSpeed * facingDirection, 0.0f);     // If you want character to fall/follow gravity, place rb2d.velocity.y instead of 0.0f.
                dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXPosition) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXPosition = transform.position.x;
                }
            }

            if (dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;                
            }
        }        
    }

    private void WallSlide()
    { 
        if (isTouchingWall && !isTouchingGround && !isClimbingLedge)
        {
            isWallSliding = true;
        }      
        else
        {
            isWallSliding = false;
        }

        if (isWallSliding && !isClimbingLedge)
        {
            if (rb2d.velocity.y < -wallSlideSpeed)
            {
                rb2d.velocity = new Vector2(0, -wallSlideSpeed);
            }            
        }        
    }

    private void WallGrab()
    {
        if (wallGrab)
        {
            isWallGrabbing = true;
            isWallSliding = false;
            wallGrab = false;
        }
        else
        {
            isWallGrabbing = false;
        }

        if (isWallGrabbing)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.gravityScale = 0.0f;
        }
        else
        {
            rb2d.gravityScale = gravityScale;
        }        
    }

    private void WallClimb()
    {
        if (wallClimb)
        {
            isWallClimbing = true;
            isWallSliding = false;
            isWallGrabbing = false;
            wallClimb = false;
        }
        else
        {
            isWallClimbing = false;
        }

        if (isWallClimbing)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, climbSpeed * verticalInputDirection);
        }        
    }

    private void ResetJumpCount()
    {
        amountOfJumpsRemaining = amountOfJumps;
    }

    private void JumpValidation()
    {
        if (isTouchingGround || isTouchingWall)
        {
            ResetJumpCount();
        }

        if (amountOfJumpsRemaining <= 0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }

        if (isTouchingWall)
        {
            canWallJump = true;
            canJump = false;
        }
        else
        {
            canWallJump = false;
        }       

        wallJumpDirection = -facingDirection;
    }

    private void Jump()
    {
        if (jump && canJump)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
            isJumping = true;
            isWallJumping = false;
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            amountOfJumpsRemaining--;
            jump = false;
        }         
    }

    private void WallJump()
    {
        wallJumpDirection = -facingDirection;
        if (wallJump && canWallJump)
        {
            if (horizontalInputDirection != facingDirection)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
                isWallJumping = true;
                Vector2 wallJumpAddForce = new Vector2(wallJumpVector.x * wallJumpForce * wallJumpDirection, wallJumpVector.y * wallJumpForce);
                rb2d.AddForce(wallJumpAddForce, ForceMode2D.Impulse);
                Flip();
                amountOfJumpsRemaining--;
                wallJump = false;
            }             
        }        
    }

    private void BetterJump()
    {
        if (rb2d.velocity.y < 0)        // Faster and sharper decent.
        {
            rb2d.gravityScale = fallMultiplier;
        }
        else if (rb2d.velocity.y > 0 && !Input.GetButton("Jump"))       // Variable Jump.
        {
            rb2d.gravityScale = variableJumpMultiplier;
        }               
    }

    private void Teleport()
    {
        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePositionBottom = wallCheck.position;
        }

        if (ledgeDetected && !isClimbingLedge)
        {
            isClimbingLedge = true;

            if (isFacingRight)
            {
                ledgePosition1 = new Vector2(Mathf.Floor(ledgePositionBottom.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbYOffset1);
                ledgePosition2 = new Vector2(Mathf.Floor(ledgePositionBottom.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePosition1 = new Vector2(Mathf.Ceil(ledgePositionBottom.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbYOffset1);
                ledgePosition2 = new Vector2(Mathf.Ceil(ledgePositionBottom.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePositionBottom.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;            
        }
        
        if (isClimbingLedge)
            transform.position = ledgePosition1;
    }

    #region Animation Handling
    private void FinishLedgeClimb()
    {
        isClimbingLedge = false;
        transform.position = ledgePosition2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("isClimbingLedge", isClimbingLedge);
    }
    
    private void FlipCharacter()
    {
        if (isFacingRight && horizontalInputDirection < 0)
            Flip();
        else if (!isFacingRight && horizontalInputDirection > 0)
            Flip();
    }

    private void Flip()
    {
        if (canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void UpdateAnimations()
    {
        if (Mathf.Abs(rb2d.velocity.x) >= 0.01f && isTouchingGround && !isTouchingWall)
            isRunning = true;
        else
            isRunning = false;
        anim.SetBool("isRunning", isRunning);        

        anim.SetBool("isCrouching", isCrouching);

        anim.SetBool("isRolling", isRolling);

        anim.SetBool("isDashing", isDashing);

        anim.SetBool("isWallSliding", isWallSliding);

        anim.SetBool("isWallGrabbing", isWallGrabbing);

        anim.SetBool("isWallClimbing", isWallClimbing);

        anim.SetFloat("yVelocity", rb2d.velocity.y);
        
        anim.SetBool("isTouchingGround", isTouchingGround);

        anim.SetBool("isJumping", isJumping);

        if (amountOfJumpsRemaining == 1)
        {
            isFirstJump = true;            
        }
        else
        {
            isFirstJump = false;
            isJumping = false;
            isWallJumping = false;
        }
        anim.SetBool("isFirstJump", isFirstJump);

        anim.SetBool("isWallJumping", isWallJumping);

        anim.SetBool("isClimbingLedge", isClimbingLedge);
    }
    #endregion

    #region Returns
    // *** Add public returns for other scripts to access. *** //
    public bool WallGrabReturn()
    {
        return isWallGrabbing;
    }

    public bool CrouchReturn()
    {
        return isCrouching;
    }
    #endregion
}

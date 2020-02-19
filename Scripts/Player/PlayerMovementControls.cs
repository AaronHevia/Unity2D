using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementControls : MonoBehaviour
{
    #region Variables and Constants

    #region Initial References
    
    [SerializeField] Transform ceilingCheck;                            // Stores the position which we will be checking for ceiling.
    [SerializeField] Transform groundCheck;                             // Stores the position which we will be checking for the ground.
    private readonly float CheckRadius = 0.3f;                                   // Stores the radius of the ground and ceiling check objects.
    [SerializeField] Transform wallCheck;                               // Stores the position which we will be checking for walls.
    private readonly float wallCheckDistance = 0.35f;                            // Stores the distance of the player to the wall.
    [SerializeField] Transform ledgeCheck;                              // Stores reference for ledge detection.
    [SerializeField] private float ledgeClimbXOffset1 = 0.0f;           // 
    [SerializeField] private float ledgeClimbYOffset1 = 0.0f;           //
    [SerializeField] private float ledgeClimbXOffset2 = 0.0f;           //
    [SerializeField] private float ledgeClimbYOffset2 = 0.0f;           //
    [SerializeField] LayerMask whatArePlatforms;                        // Layer used to determine primary walls and floors.
    [SerializeField] float wallSlideSpeed = 2.0f;                              // Stores the speed of wall slide.  
    private Rigidbody2D rb2d;                                           // Stores reference for the player's rigid body.
    private Animator anim;                                              // Stores reference for the player's Animator.
    #endregion

    #region Horizontal Movement
    [SerializeField] private float horizontalMoveSpeed = 15.0f;                   // Stores speed at which the character moves left or right.
    [SerializeField] private float airDragMultiplier = 0.95f;           // Stores the force in which to slow down x movement if player gives no more horizontal input.
    private float horizontalInputDirection;                             // Stores what direction the player is trying to move in.
    #endregion

    #region Surroundings
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isTouchingCeiling;
    private bool isGrounded;
    #endregion

    #region Ledges
    private Vector2 ledgePositionBottom;            // Stores references for ledge position tile.
    private Vector2 ledgePosition1;                 // Stores references for ledge position tile.
    private Vector2 ledgePosition2;                 // Stores references for ledge position tile.
    private bool canClimbLedge = false;
    private bool ledgeDetected;
    private bool isTouchingLedge;
    private bool canMove;
    private bool canFlip;
    #endregion

    #region Weapon Switch
    [SerializeField] private GameObject[] meleeweapons;             // Armed and Unarmed stances (determined by the amount of transforms assigned as weapons).
    private int currentWeapon = 0;                                  // Reference for initial stance.
    private int previousWeapon;                                     // Reference for previous weapon used.
    #endregion    
    
    #region Jumping
    [SerializeField] private float jumpForce = 26.0f;           // Stores jump force at which makes the character jump.
    private readonly float variableJumpHeightMultiplier = 0.5f;          // Stores the variable in which the jump force is multiplied by depending on how long jump key is pushed.     
    [SerializeField] float wallJumpForce = 13.0f;               // Stores the jump force in which the character wall jumps with.
    [SerializeField] float jumpTimerSet = 0.15f;
    [SerializeField] float turnTimerSet = 0.2f;
    [SerializeField] float wallJumpTimerSet = 0.5f;
    [SerializeField] Vector2 wallJumpDirection;                                 // Stores the depth in which we jump off of the walls    
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private readonly int amountOfJumps = 2;                                     // Stores the amount of times a player can jump without hitting ground.
    private int amountOfJumpsRemaining;                                         // Stores remaining amount of jumps left before hitting ground.
    private int facingDirection = 1;                                            // Stores the direction in which your character is facing.  Initialized to 1 since character is originally facing right.
    private int lastWallJumpDirection;
    private bool isAttemptingToJump;
    private bool canNormalJump;
    private bool canWallJump;
    private bool checkJumpMultiplier;
    private bool hasWallJumped;
    #endregion

    #region Dash
    [SerializeField] public float dashTime = 0.25f;
    [SerializeField] public float dashSpeed = 30.0f;
    [SerializeField] public float distanceBetweenImages = 0.1f;
    [SerializeField] public float dashCoolDown = 1.5f;
    private float dashTimeLeft;
    private float lastImageXPosition;
    private float lastDash = -100f;
    private bool isDashing;    
    #endregion

    #region Crouch
    [SerializeField] public CapsuleCollider2D standing;
    [SerializeField] public CapsuleCollider2D crouching;
    [SerializeField] private float crawlSpeedMultiplier = 0.5f;
    private bool isCrouching;
    private bool isCrawling;
    #endregion

    #region Floor Slide    
    [SerializeField] public float slideTime = 0.25f;    
    [SerializeField] private float slideSpeed = 13.0f;
    [SerializeField] public float slideCoolDown = 1.5f;
    private float lastSlide = -100f;
    private float slideTimeLeft;
    private bool isSliding = false;
    #endregion

    #region Wall Grab
    private readonly float climbSpeed = 1.0f;
    private float gravityStore;
    private float verticalInputDirection;           // Stores what direction the player is trying to move in.
    private bool wallGrab;    
    private float yAxis;    
    #endregion    

    #region Animation
    private bool isFacingRight = true;          // Stores whether the character is facing right or not.  Initialized to true due to direction character is starting.
    private bool isRunning;
    private bool doubleJump;
    #endregion

    #region Damage Received
    private bool knockback;
    private float knockbackStartTime;    
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Vector2 knockbackSpeed;
    #endregion
    #endregion

    #region Main Methods
    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
        
    void Start()
    {
        amountOfJumpsRemaining = amountOfJumps;         // Initializes game to jump however amount of times set(double jump for our case).        
        wallJumpDirection = new Vector2(1f, 2f);                  // Initializes wall direction to 1.        
        standing.enabled = true;
        crouching.enabled = false;        
        gravityStore = rb2d.gravityScale;        
        SelectWeapon();                                 // Initializes weapon select to unarmed.
    }
        
    void Update()
    {        
        Crouch();
        WallGrab();
        CheckDash();
        CheckJump();
        CheckInput();
        CheckSlide();
        FloorSlide();
        WeaponSwitch();        
        FlipCharacter();
        CheckIfCanJump();
        CheckKnockback();
        CheckLedgeClimb();
        UpdateAnimations();
        CheckIfWallSliding();        
    }
        
    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();        
    }
    #endregion

    #region Interaction
    private void CheckInput()                   // Checks for any kind of input we expect from the player.
    {
        horizontalInputDirection = Input.GetAxisRaw("Horizontal");          // Checks for -1(left) or 1(right) when the assigned keys(found in Project Settings -> Input) are pressed.
        verticalInputDirection = Input.GetAxisRaw("Vertical");
        yAxis = Input.GetAxis("Vertical");


        if (Input.GetButtonDown("Jump"))                                    // Checks if player pushed the jump button.
        {
            if (isGrounded || (amountOfJumpsRemaining > 0 && !isTouchingWall))
                NormalJump();
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)            // Logic to keep player from wall jumping same wall with 1 jump.
        {
            if (!isGrounded && horizontalInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;
                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0)                                                 // Logic to keep player from wall jumping same wall with 1 jump.
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump"))                // Half jump (or whatever the variable is set to).
        {
            checkJumpMultiplier = false;
            rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y * variableJumpHeightMultiplier);
        }

        if (Input.GetButtonDown("Dash"))                                    // Dash.
        {            
            if (Time.time >= (lastDash + dashCoolDown))
                AttemptToDash();                       
        }
    }

    private void CheckSurroundings()            // Checks world space surroundings in order to apply necessary physics.
    {
        isTouchingCeiling = Physics2D.OverlapCircle(ceilingCheck.position, CheckRadius, whatArePlatforms);          // Checks for the ceiling.  Requires a point a radius and a layer mask.

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, CheckRadius, whatArePlatforms);                    // Checks for the ground.  Requires a point a radius and a layer mask.

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatArePlatforms);       // Checks for the walls.

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatArePlatforms);     // Checks for ledges.
        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePositionBottom = wallCheck.position;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && horizontalInputDirection == facingDirection && rb2d.velocity.y < 0 && !canClimbLedge)
            isWallSliding = true;
        else
            isWallSliding = false;
    }

    private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

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

            anim.SetBool("canClimbLedge", canClimbLedge);
        }

        if (canClimbLedge)
            transform.position = ledgePosition1;
    }

    private void ApplyMovement()                // Applies Movement velocity (x axis) to the Rigid Body when left or right is pushed.
    {
        if (!isGrounded && !isWallSliding && horizontalInputDirection == 0 && !knockback)                             // Slows down x velocity if player is no longer giving any more input in air.
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x * airDragMultiplier, rb2d.velocity.y);
        }
        else if (canMove && !knockback)
        {
            rb2d.velocity = new Vector2(horizontalMoveSpeed * horizontalInputDirection, rb2d.velocity.y);         // Applies Horizontal movement to the Rigidbody when moving.
        }

        if (isCrawling && isCrouching && !knockback)        
            rb2d.velocity = new Vector2(crawlSpeedMultiplier * horizontalMoveSpeed * horizontalInputDirection, rb2d.velocity.y);
        
        if (isWallSliding)          // Limits the character's speed while wall sliding.
        {
            if (rb2d.velocity.y < -wallSlideSpeed)
                rb2d.velocity = new Vector2(rb2d.velocity.x, -wallSlideSpeed);
        }
    }
    #endregion

    #region Weapon Switching
    private void WeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            previousWeapon = currentWeapon;

            if (currentWeapon >= meleeweapons.Length - 1)
                currentWeapon = 0;
            else
                currentWeapon++;

            if (previousWeapon != currentWeapon)
                SelectWeapon();

            anim.SetInteger("currentWeapon", currentWeapon);
            anim.SetTrigger("weaponSwitchTrigger");
        }
    }

    private void SelectWeapon()
    {
        int i = 0;
        foreach (GameObject weapon in meleeweapons)
        {
            if (i == currentWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            i++;
        }
    }
    #endregion  
        
    #region Jumping
    private void CheckIfCanJump()           // Checks to see if you can jump.
    {
        if (isGrounded && rb2d.velocity.y <= 0.01f)         // Resets remaining jumps.        
            amountOfJumpsRemaining = amountOfJumps;        

        if (isTouchingWall)                                 // Sets jump logic while touching wall.
        {
            checkJumpMultiplier = false;
            canWallJump = true;
        }

        if (amountOfJumpsRemaining <= 0 || (isCrouching || isCrawling))                    // Sets bool of jumping.        
            canNormalJump = false;        
        else        
            canNormalJump = true;
    }

    private void CheckJump()            // Determines How to jump.
    {
        if (jumpTimer > 0)          // Determines which jump to use.
        {            
            if (!isGrounded && isTouchingWall && horizontalInputDirection != 0 && horizontalInputDirection != facingDirection)            
                WallJump();            
            else if (isGrounded)            
                NormalJump();            
        }
        
        if (isAttemptingToJump)         // Start of timer for variable normal jump.        
            jumpTimer -= Time.deltaTime;        

        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && horizontalInputDirection == -lastWallJumpDirection)            //prevents player from turning back to same wall.
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0.0f);             
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)            
                hasWallJumped = false;            
            else            
                wallJumpTimer -= Time.deltaTime;            
        }
    }
    
    private void NormalJump()           // Applies jump velocity (y axis) to the Rigid Body when jump button is pushed.
    {
        if (canNormalJump)          // Applies function if canNormalJump is true.                                                                                                                          
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
            amountOfJumpsRemaining--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()         // Applies wall jump velocity (y axis) to the Rigid Body when jump button is pushed.
    {
        if (canWallJump)            // Applies function if canWallJump is true.
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsRemaining = amountOfJumps;
            amountOfJumpsRemaining--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * horizontalInputDirection, wallJumpForce * wallJumpDirection.y);
            rb2d.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }
    #endregion

    #region Dash
    private void AttemptToDash()
    {        
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXPosition = transform.position.x;        
    }

    private void CheckDash()            // Responsible for setting the dash velocity and checking if we should be dashing or if we should stop.
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                rb2d.velocity = new Vector2(dashSpeed * facingDirection, 0.0f);      //If you want character to fall while dashing replace the zero with rb2d.velocity.y
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
    #endregion

    #region Crouch
    private void Crouch()
    {
        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || isTouchingCeiling) && isGrounded)        
            isCrouching = true;        
        else        
            isCrouching = false;        
  
        if(!isGrounded)
        {
            standing.enabled = true;
            crouching.enabled = false;            
        }
        else
        {
            if (isCrouching)
            {                
                standing.enabled = false;
                crouching.enabled = true;                
            }
            else
            {                
                standing.enabled = true;
                crouching.enabled = false;                
            }         
        }

        if (isCrouching && Mathf.Abs(rb2d.velocity.x) > 0.01)         // Crouch logic.                
            isCrawling = true;                    
        else
            isCrawling = false;                    
    }
    #endregion

    #region Floor Slide
    private void FloorSlide()
    {
        if (isCrouching && rb2d.velocity.x == 0 && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Keypad0)))
        {
            if (Time.time >= (lastSlide + slideCoolDown))
                AttemptToSlide();
        }        
    }

    private void AttemptToSlide()
    {        
        isSliding = true;        
        slideTimeLeft = slideTime;
        lastSlide = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXPosition = transform.position.x;
    }

    private void CheckSlide()            // Responsible for setting the dash velocity and checking if we should be dashing or if we should stop.
    {
        if (isSliding)
        {
            if (slideTimeLeft > 0)
            {                
                crouching.enabled = false;                
                isCrouching = false;                
                canMove = false;
                canFlip = false;
                rb2d.velocity = new Vector2(slideSpeed * facingDirection, 0.0f);      //If you want character to fall while dashing replace the zero with rb2d.velocity.y
                slideTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXPosition) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXPosition = transform.position.x;
                }
            }

            if (slideTimeLeft <= 0 || isTouchingWall)
            {
                crouching.enabled = true;                
                isCrouching = true;
                isSliding = false;
                canMove = true;
                canFlip = true;
            }
        }
    }
    #endregion

    #region Wall Grab
    
    private void WallGrab()
    {   
            if ((isTouchingWall || isWallSliding) && !isGrounded && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {                
                isWallSliding = false;
                wallGrab = true;                
            }
            else
                wallGrab = false;

            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                wallGrab = false;

            if (wallGrab)
            {
                rb2d.gravityScale = 0.0f;

                rb2d.velocity = new Vector2(rb2d.velocity.x, verticalInputDirection * climbSpeed);
            }
            else
                rb2d.gravityScale = gravityStore;

            anim.SetBool("wallGrab", wallGrab);
            anim.SetFloat("yAxis", yAxis);        
    }
    #endregion

    #region Damage Received Knockback
    public void Knockback(int direction)
    {
        knockback = true;
        knockbackStartTime = Time.time;
        rb2d.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
    }

    private void CheckKnockback()
    {
        if (Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rb2d.velocity = new Vector2(0.0f, rb2d.velocity.y);
        }
    }
    #endregion

    #region Animation Handling        
    private void FlipCharacter()            // Flips character depending on input.
    {
        if (isFacingRight && horizontalInputDirection < 0)
            Flip();
        else if (!isFacingRight && horizontalInputDirection > 0)
            Flip();
    }

    private void Flip()         // How the Flip is performed.
    {
        if (!isWallSliding && canFlip && !knockback)          //if statement keeps the flip from happening while wallsliding.
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void UpdateAnimations()
    {
        if (Mathf.Abs(rb2d.velocity.x) >= 0.01f  && (!isCrouching || !isCrawling))            // Sets animation bool for running.
            isRunning = true;
        else
            isRunning = false;
        anim.SetBool("isRunning", isRunning);           

        if (amountOfJumpsRemaining == 1)            // Sets animation bool for double jump.
            doubleJump = true;
        else
            doubleJump = false;
        anim.SetBool("doubleJump", doubleJump);

        anim.SetBool("isGrounded", isGrounded);       
        anim.SetBool("isWallSliding", isWallSliding); 
        anim.SetFloat("yVelocity", rb2d.velocity.y);         
        anim.SetBool("isCrawling", isCrawling);       
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isSliding", isSliding);
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePosition2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }
    #endregion    

    #region Tools and Utilities    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(ceilingCheck.position, CheckRadius);                                                                             // CeilingCheck sphere.

        Gizmos.DrawWireSphere(groundCheck.position, CheckRadius);                                                                             // GroundCheck sphere.
        
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));     // WallCheck.        
    }

    public int GetFacingDirection()
    {
        return facingDirection;
    }

    public void DisableFlip()
    {
        canFlip = false;
    }

    public void EnableFlip()
    {
        canFlip = true;
    }

    public bool Grounded()
    {
        return isGrounded;
    }

    public int CurrentWeapon()
    {
        return currentWeapon;
    }

    public bool Crouching()
    {
        return isCrouching;
    }

    public bool WallSliding()
    {
        return isWallSliding;
    }

    public void DisableMove()
    {
        canMove = false;
        rb2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    public void EnableMove()
    {
        canMove = true;
        rb2d.constraints = RigidbodyConstraints2D.FreezePositionX - 1 | RigidbodyConstraints2D.FreezeRotation;
    }

    public bool GetDashStatus()
    {
        return isDashing;        
    }

    public bool GetSlideStatus()
    {
        return isSliding;
    }
    #endregion
}
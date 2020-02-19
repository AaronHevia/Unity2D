using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatControls : MonoBehaviour
{
    #region Variables and Constants

    #region Initial Variables
    private PlayerMovementControls pmc;
    private Animator anim;
    private Rigidbody2D rb2d;
    private bool isAttacking;
    #endregion  

    #region Arrows
    [SerializeField] private GameObject[] arrows;
    private int currentArrow = 0;
    private int previousArrow;
    #endregion

    #region Spells
    [SerializeField] private GameObject[] spells;
    private int currentSpell = 0;
    private int previousSpell;
    #endregion

    #region Unarmed Combat
    [SerializeField] public float unarmedInputDelay = 20.0f;
    [SerializeField] private int numberOfUnarmedAttacks = 0;
    private float lastUnarmedAttackTime = Mathf.NegativeInfinity;
    private bool unarmed1;
    private bool unarmed2;
    private bool unarmed3;
    private bool unarmed4;
    private bool airAttack;
    private bool canJumpKick;
    private bool jumpKick;
    #endregion

    #region Armed Combat
    [SerializeField] public float armedComboDelay = 1.5f;
    [SerializeField] private int numberOfArmedAttacks = 0;
    private float lastArmedAttackTime = Mathf.NegativeInfinity;
    private bool armed1 = false;
    private bool armed2 = false;
    private bool armed3 = false;
    [SerializeField] public float powerArmedComboDelay = 1.5f;
    [SerializeField] private bool isPowerArmed;
    [SerializeField] private int numberOfPowerArmedAttacks = 0;
    private float lastPowerArmedAttackTime = Mathf.NegativeInfinity;
    private bool powerArmed1 = false;
    private bool powerArmed2 = false;
    private bool powerArmed3 = false;

    // Set up air attack combo.

    private bool airArmed1 = false;
    private bool airArmed2 = false;
    private bool airArmed3 = false;
    #endregion

    #region Bow Attacks    
    [SerializeField] private float attackCD = .75f;
    private float attackTimer;
    private bool shotArrow = false;
    private bool canShootBow;
    private bool jumpShot;
    #endregion

    #region Spell Attacks
    [SerializeField] private float spellCD = 1.25f;
    private float spellTimer;    
    private bool spellCast = false;
    #endregion

    #region Potion    
    private readonly float potionCD = 0.30f;    
    private float potionTimer;
    private bool potionUsed = false;
    public bool potionDisabled;
    #endregion

    #region Hitbox
    [SerializeField] private Transform attackHitBox;
    [SerializeField] private float attackRadius;
    [SerializeField] private LayerMask whatIsDamageable;
    [SerializeField] private float[] attackDamage = new float[21];
    #endregion

    #region Damage Received
    private PlayerStats playerstats;
    #endregion
    #endregion

    #region Main States
    private void Awake()
    {
        anim = GetComponent<Animator>();
        pmc = GetComponent<PlayerMovementControls>();
        rb2d = GetComponent<Rigidbody2D>();
        playerstats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        unarmed1 = false;
        unarmed2 = false;
        unarmed3 = false;
        unarmed4 = false;
        jumpKick = false;
        armed1 = false;
        armed2 = false;
        armed3 = false;
        powerArmed1 = false;
        powerArmed2 = false;
        powerArmed3 = false;
        airArmed1 = false;
        airArmed2 = false;
        airArmed3 = false;
        SelectArrow();
        SelectSpell();        
    }   

    void Update()
    {
        ArrowSwitch();
        SpellSwitch();
        UpdateAnimations();
        UnarmedAttacks();
        ArmedAttacks();
        PowerArmedAttacks();
        BowAttacks();
        Spells();
        JumpKick();
        JumpShot();
        Potions();
        DamageCalculator();
    }
    #endregion

    #region Arrows
    private void ArrowSwitch()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            previousArrow = currentArrow;

            if (currentArrow >= arrows.Length - 1)
                currentArrow = 0;
            else
                currentArrow++;

            if (previousArrow != currentArrow)
                SelectArrow();
        }
    }

    private void SelectArrow()
    {
        int i = 0;
        foreach (GameObject arrow in arrows)
        {
            if (i == currentArrow)
                arrow.gameObject.SetActive(true);
            else
                arrow.gameObject.SetActive(false);
            i++;
        }
    }
    #endregion

    #region Spells
    private void SpellSwitch()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            previousSpell = currentSpell;

            if (currentSpell >= spells.Length - 1)
                currentSpell = 0;
            else
                currentSpell++;

            if (previousSpell != currentSpell)
                SelectSpell();
        }
    }

    private void SelectSpell()
    {
        int i = 0;
        foreach (GameObject spell in spells)
        {
            if (i == currentSpell)
                spell.gameObject.SetActive(true);
            else
                spell.gameObject.SetActive(false);
            i++;
        }
    }
    #endregion    

    #region Unarmed Combat
    public void UnarmedAttacks()
    {
        if (Time.deltaTime >= lastUnarmedAttackTime + unarmedInputDelay)        
            numberOfUnarmedAttacks = 0;        

        if (!shotArrow && !spellCast && pmc.CurrentWeapon() == 0 && pmc.Grounded() && Input.GetKeyDown(KeyCode.Keypad1))
        {           
            lastUnarmedAttackTime = Time.deltaTime;
            numberOfUnarmedAttacks++;

            if (numberOfUnarmedAttacks == 1)
                unarmed1 = true;

            numberOfUnarmedAttacks = Mathf.Clamp(numberOfUnarmedAttacks, 0, 4);
        }
    }

    private void JumpKick()
    {
        if (pmc.CurrentWeapon() == 0 && !pmc.Grounded() && Input.GetKeyDown(KeyCode.Keypad1))
        {
            canJumpKick = true;
            lastUnarmedAttackTime = Time.deltaTime;
        }

        if (canJumpKick)
        {
            if (!isAttacking)
            {
                jumpKick = true;
                canJumpKick = false;
                isAttacking = true;
                airAttack = true;
                anim.SetBool("isAttacking", isAttacking);
                anim.SetBool("airAttack", airAttack);
                anim.SetBool("jumpKick", jumpKick);
            }
        }

        if (Time.deltaTime >= lastUnarmedAttackTime + unarmedInputDelay)
            canJumpKick = false;
    }
    #endregion

    #region Armed Combat
    public void ArmedAttacks()
    {
        if (!isPowerArmed)
        {
            if (Time.deltaTime >= lastArmedAttackTime + armedComboDelay)
                numberOfArmedAttacks = 0;

            if (!shotArrow && !spellCast && pmc.CurrentWeapon() == 1 && pmc.Grounded() && !pmc.Crouching() && !isPowerArmed && Input.GetKeyDown(KeyCode.Keypad1))
            {
                lastArmedAttackTime = Time.deltaTime;
                numberOfArmedAttacks++;

                if (numberOfArmedAttacks == 1)
                    armed1 = true;

                numberOfArmedAttacks = Mathf.Clamp(numberOfArmedAttacks, 0, 3);
            }
        }
    }

    public void PowerArmedAttacks()
    {
        if (Time.deltaTime >= lastPowerArmedAttackTime + powerArmedComboDelay)        
            numberOfPowerArmedAttacks = 0;        

        if (!shotArrow && !spellCast && pmc.CurrentWeapon() == 1 && pmc.Grounded() && !pmc.Crouching() && isPowerArmed && Input.GetKeyDown(KeyCode.Keypad1))
        {
            lastPowerArmedAttackTime = Time.deltaTime;
            numberOfPowerArmedAttacks++;

            if (numberOfPowerArmedAttacks == 1)
                powerArmed1 = true;

            numberOfPowerArmedAttacks = Mathf.Clamp(numberOfPowerArmedAttacks, 0, 3);
        }
    }
    #endregion

    #region Bow Attacks
    private void BowAttacks()
    {
        if (!unarmed1 && !unarmed2 && !unarmed3 && !unarmed4 && !armed1 && !armed2 && !armed3 && !powerArmed1 && !powerArmed2 && !powerArmed3 && !shotArrow && !spellCast && pmc.Grounded() && Input.GetKeyDown(KeyCode.Keypad2))
        {
            shotArrow = true;
            attackTimer = attackCD;
            // Fire arrow at release point of clip.
        }

        if (shotArrow)
        {
            if (attackTimer > 0)            
                attackTimer -= Time.deltaTime;            
            else            
                shotArrow = false;            
        }
    }

    private void JumpShot()
    {
        if ((pmc.CurrentWeapon() == 0 || pmc.CurrentWeapon() == 1) && !pmc.Grounded() && Input.GetKeyDown(KeyCode.Keypad2))
        {
            canShootBow = true;
            attackTimer = Time.deltaTime;
        }

        if (canShootBow)
        {
            if (!isAttacking)
            {
                jumpShot = true;
                canShootBow = false;
                isAttacking = true;
                airAttack = true;
                anim.SetBool("isAttacking", isAttacking);
                anim.SetBool("airAttack", airAttack);
                anim.SetBool("jumpShot", jumpShot);
            }
        }
        if (Time.deltaTime >= lastUnarmedAttackTime + unarmedInputDelay)
            canJumpKick = false;
    }
    #endregion

    #region Spells
    private void Spells()
    {       
        if (!unarmed1 && !unarmed2 && !unarmed3 && !unarmed4 && !armed1 && !armed2 && !armed3 && !powerArmed1 && !powerArmed2 && !powerArmed3 && !shotArrow && !spellCast && pmc.Grounded() && Input.GetKeyDown(KeyCode.Keypad3))
        {   
            spellCast = true;
            spellTimer = spellCD;
            // Cast spell.
        }

        if (spellCast)
        {
            if (spellTimer > 0)            
                spellTimer -= Time.deltaTime;                
            else
                spellCast = false;
        }        
    }
    #endregion

    #region Potions
    private void Potions()
    {
        if (!potionDisabled)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Keypad7))
            {
                potionDisabled = true;
                potionUsed = true;
                potionTimer = potionCD;
                // Potion
            }

            if (potionUsed)
            {
                StartCoroutine(PotionCooldown());
                if (potionTimer > 0)
                    potionTimer -= Time.deltaTime;
                else
                    potionUsed = false;
            }
        }        
    }
    private void PotionReturn()
    {
        potionUsed = false;
    }
    IEnumerator PotionCooldown()
    {                
        yield return new WaitForSeconds(30);
        potionDisabled = false;                
    }
    #endregion   

    #region Animation Updates
    private void UpdateAnimations()
    {
        anim.SetBool("unarmed4", unarmed4);
        anim.SetBool("unarmed3", unarmed3);
        anim.SetBool("unarmed2", unarmed2);
        anim.SetBool("unarmed1", unarmed1);
        anim.SetBool("armed3", armed3);
        anim.SetBool("armed2", armed2);
        anim.SetBool("armed1", armed1);
        anim.SetBool("powerArmed3", powerArmed3);
        anim.SetBool("powerArmed2", powerArmed2);
        anim.SetBool("powerArmed1", powerArmed1);
        anim.SetBool("isPowerArmed", isPowerArmed);
        anim.SetBool("shotArrow", shotArrow);
        anim.SetBool("spellCast", spellCast);
        anim.SetBool("potionUsed", potionUsed);
    }

    private void FinishAttacks()
    {
        isAttacking = false;
        airAttack = false;
        jumpKick = false;
        jumpShot = false;        
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("airAttack", airAttack);
        anim.SetBool("jumpKick", jumpKick);
        anim.SetBool("jumpShot", jumpShot);        
    }

    #region Unarmed Returns
    public void Return1()
    {
        if (numberOfUnarmedAttacks >= 2)
        {
            unarmed2 = true;
            unarmed1 = false;
        }
        else
        {
            unarmed1 = false;
            armed1 = false;
            armed2 = false;
            armed3 = false;
            numberOfUnarmedAttacks = 0;
        }
    }
    public void Return2()
    {
        if (numberOfUnarmedAttacks >= 3)
        {
            isPowerArmed = true;
            unarmed3 = true;
            unarmed2 = false;
            armed1 = false;
            armed2 = false;
            armed3 = false;
        }
        else
        {
            unarmed2 = false;
            armed1 = false;
            armed2 = false;
            armed3 = false;
            numberOfUnarmedAttacks = 0;
        }
    }

    public void Return3()
    {
        if (numberOfUnarmedAttacks >= 4)
        {            
            unarmed4 = true;
            unarmed3 = false;
            armed1 = false;
            armed2 = false;
            armed3 = false;
        }
        else
        {
            unarmed3 = false;
            armed1 = false;
            armed2 = false;
            armed3 = false;
            numberOfUnarmedAttacks = 0;
        }
    }

    public void Return4()
    {
        unarmed4 = false;
        isPowerArmed = false;
        armed1 = false;
        armed2 = false;
        armed3 = false;
        numberOfUnarmedAttacks = 0;
    }
    #endregion

    #region Armed Returns
    public void ArmedReturn1()
    {
        if (numberOfArmedAttacks >= 2)
        {
            armed2 = true;
            armed1 = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
        }
        else
        {
            armed1 = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
            numberOfArmedAttacks = 0;
        }
    }
    public void ArmedReturn2()
    {
        if (numberOfArmedAttacks >= 3)
        {
            armed3 = true;
            armed2 = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
        }
        else
        {
            armed2 = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
            numberOfArmedAttacks = 0;
        }
    }

    public void ArmedReturn3()
    {
        armed3 = false;
        unarmed1 = false;
        unarmed2 = false;
        unarmed3 = false;
        unarmed4 = false;
        numberOfArmedAttacks = 0;
    }
    #endregion

    #region Power Armed Returns
    public void PowerArmedReturn1()
    {
        if (numberOfPowerArmedAttacks >= 2)
        {
            powerArmed2 = true;
            powerArmed1 = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
        }
        else
        {
            powerArmed1 = false;
            isPowerArmed = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
            numberOfPowerArmedAttacks = 0;
        }
    }

    public void PowerArmedReturn2()
    {
        if (numberOfPowerArmedAttacks >= 3)
        {
            powerArmed3 = true;
            powerArmed2 = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
        }
        else
        {
            powerArmed2 = false;
            isPowerArmed = false;
            unarmed1 = false;
            unarmed2 = false;
            unarmed3 = false;
            unarmed4 = false;
            numberOfPowerArmedAttacks = 0;
        }
    }

    public void PowerArmedReturn3()
    {
        powerArmed3 = false;
        isPowerArmed = false;
        unarmed1 = false;
        unarmed2 = false;
        unarmed3 = false;
        unarmed4 = false;
        numberOfPowerArmedAttacks = 0;
    }
    #endregion

    public void WeaponSwitchReturn()
    {
        unarmed1 = false;
        unarmed2 = false;
        unarmed3 = false;
        unarmed4 = false;
        armed1 = false;
        armed2 = false;
        armed3 = false;
        powerArmed1 = false;
        powerArmed2 = false;
        powerArmed3 = false;
    }
    #endregion

    #region Hitbox
    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitBox.position, attackRadius, whatIsDamageable);

        foreach (Collider2D collider in detectedObjects)
        {
            if (unarmed1)            
                collider.transform.parent.SendMessage("Damage", attackDamage[0]);            
            else if (unarmed2)
                collider.transform.parent.SendMessage("Damage", attackDamage[1]);
            else if (unarmed3)
                collider.transform.parent.SendMessage("Damage", attackDamage[2]);
            else if (unarmed4)
                collider.transform.parent.SendMessage("Damage", attackDamage[3]);
            else if (armed1)
                collider.transform.parent.SendMessage("Damage", attackDamage[4]);
            else if (armed2)
                collider.transform.parent.SendMessage("Damage", attackDamage[5]);
            else if (armed3)
                collider.transform.parent.SendMessage("Damage", attackDamage[6]);
            else if (powerArmed1)
                collider.transform.parent.SendMessage("Damage", attackDamage[7]);
            else if (powerArmed2)
                collider.transform.parent.SendMessage("Damage", attackDamage[8]);
            else if (powerArmed3)
                collider.transform.parent.SendMessage("Damage", attackDamage[9]);
            else if (jumpKick)
                collider.transform.parent.SendMessage("Damage", attackDamage[10]);
            else if (airArmed1)
                collider.transform.parent.SendMessage("Damage", attackDamage[11]);
            else if (airArmed2)
                collider.transform.parent.SendMessage("Damage", attackDamage[12]);
            else if (airArmed3)
                collider.transform.parent.SendMessage("Damage", attackDamage[13]);
            else if (arrows[0] && shotArrow)
                collider.transform.parent.SendMessage("Damage", attackDamage[14]);
            else if (arrows[1] && shotArrow)
                collider.transform.parent.SendMessage("Damage", attackDamage[15]);
            else if (arrows[2] && shotArrow)
                collider.transform.parent.SendMessage("Damage", attackDamage[16]);
            else if (spells[0] && spellCast)
                collider.transform.parent.SendMessage("Damage", attackDamage[17]);
            else if (spells[1] && spellCast)
                collider.transform.parent.SendMessage("Damage", attackDamage[18]);
            else if (spells[2] && spellCast)
                collider.transform.parent.SendMessage("Damage", attackDamage[19]);
            else if (spells[3] && spellCast)
                collider.transform.parent.SendMessage("Damage", attackDamage[20]);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackHitBox.position, attackRadius);        
    }

    // Damage Numbers
    void DamageCalculator()
    {        
        System.Random damage = new System.Random();

        attackDamage[0] = damage.Next(1, 11);       // Punch1
        attackDamage[1] = damage.Next(1, 11);       // Punch2
        attackDamage[2] = damage.Next(11, 21);      // Punch3 (stuns)
        attackDamage[3] = damage.Next(21, 36);      // Kick1/2 (knockback)        
        attackDamage[4] = damage.Next(21, 41);      // Armed1
        attackDamage[5] = damage.Next(51, 71);      // Armed2 (knockup)
        attackDamage[6] = damage.Next(81, 101);     // Armed3 (pushback)
        attackDamage[7] = damage.Next(41, 91);      // PowerArmed1
        attackDamage[8] = damage.Next(101, 121);    // PowerArmed2 (knockup)
        attackDamage[9] = damage.Next(131, 151);    // PowerArmed3 (increased pushback)
        attackDamage[10] = damage.Next(1, 8);       // JumpKick
        attackDamage[11] = damage.Next(31, 71);     // Air1 (knockup + jump)
        attackDamage[12] = damage.Next(81, 101);    // Air2 (knockup + jump)
        attackDamage[13] = damage.Next(151, 201);   // Air3 (knockdown + stun)        
        attackDamage[14] = damage.Next(11, 26);     // BluntTip 
        attackDamage[15] = damage.Next(31, 51);     // SteelTip
        attackDamage[16] = damage.Next(41, 61);     // VenomShot (damage over time)
        attackDamage[17] = damage.Next(1, 11);      // FireSpell (linear ground aoe)
        attackDamage[18] = damage.Next(1, 11);      // FrostSpell (slows)
        attackDamage[19] = damage.Next(1, 11);      // ShadowSpell (circular aoe)
        attackDamage[20] = damage.Next(1, 11);      // LightningSpell (screen aoe)        
        
    }
    #endregion

    #region Damage Received
    private void Damage(float[] attackDetails)
    {
        if (!pmc.GetDashStatus() || !pmc.GetSlideStatus())
        {
            int direction;

            playerstats.DecreaseHealth(attackDetails[0]);

            if (attackDetails[1] < transform.position.x)
                direction = 1;
            else
                direction = -1;

            pmc.Knockback(direction);
        }
    }
    #endregion
}

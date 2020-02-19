using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    [SerializeField] private GameObject hitParticle;
    
    [SerializeField] private bool applyKnockback;
    
    [SerializeField] private float maxHealth;
    [SerializeField] private float knockbackSpeedX;
    [SerializeField] private float knockbackSpeedY;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private float knockbackDeathSpeedX;
    [SerializeField] private float knockbackDeathSpeedY;
    [SerializeField] private float deathTorque;

    private PlayerMovementControls playerController;
    
    private GameObject aliveGameObject;
    private GameObject brokenTopGameObject;
    private GameObject brokenBottomGameObject;

    private Rigidbody2D rbAlive;
    private Rigidbody2D rbTop;
    private Rigidbody2D rbBottom;

    private Animator aliveAnimator;

    private bool playerOnLeft;
    private bool knockback;

    private float currentHealth;
    private float knockbackStart;

    private int playerFacingDirection;

    private void Start()
    {
        currentHealth = maxHealth;

        playerController = GameObject.Find("Player").GetComponent<PlayerMovementControls>();

        aliveGameObject = transform.Find("Alive").gameObject;
        brokenTopGameObject = transform.Find("Top").gameObject;
        brokenBottomGameObject = transform.Find("Bottom").gameObject;

        aliveAnimator = aliveGameObject.GetComponent<Animator>();
        rbAlive = aliveGameObject.GetComponent<Rigidbody2D>();
        rbTop = brokenTopGameObject.GetComponent<Rigidbody2D>();
        rbBottom = brokenBottomGameObject.GetComponent<Rigidbody2D>();

        aliveGameObject.SetActive(true);
        brokenTopGameObject.SetActive(false);
        brokenBottomGameObject.SetActive(false);
    }

    private void Update()
    {
        CheckKnockback();
    }

    private void Damage(float attackDamage)
    {
        currentHealth -= attackDamage;
        playerFacingDirection = playerController.GetFacingDirection();
                
        Instantiate(hitParticle, aliveGameObject.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
        
        if (playerFacingDirection == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }

        aliveAnimator.SetBool("playerOnLeft", playerOnLeft);
        aliveAnimator.SetTrigger("damage");

        if (applyKnockback && currentHealth > 0.0f)
        {
            Knockback();
        }

        if (currentHealth < 0.0f)
        {
            Die();
        }
    }

    private void Knockback()
    {
        knockback = true;
        knockbackStart = Time.time;
        rbAlive.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
    }

    private void CheckKnockback()
    {
        knockback = false;
        rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y);
    }

    private void Die()
    {
        aliveGameObject.SetActive(false);
        brokenTopGameObject.SetActive(true);
        brokenBottomGameObject.SetActive(true);

        brokenTopGameObject.transform.position = aliveGameObject.transform.position;
        brokenBottomGameObject.transform.position = aliveGameObject.transform.position;

        rbBottom.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
        rbTop.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbTop.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse);

    }
}

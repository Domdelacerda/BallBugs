using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeStinger : Projectile
{
    // The amount of extra damage dealt after the bullet bounces
    public int damagePerBounce = 5;
    // Private damage field for calculations
    private int damage = 0;
    // Float variable used to determine the time before the stinger has collision with bee
    public float colliderDelay = 0.25f;
    // Collider used to check if the stinger is overlapping 
    private Collider2D opponent;
    // The size of the stinger used for overlapping detection
    private const float stingerSize = 0.005f;

    public int poisonDamage = 0;
    public float poisonInterval = 0;
    public int numPoisonIntervals = 0;
    public bool toxic = false;
    public bool shieldPiercing = false;
    public bool tacticalReload = false;
    public bool superTacticalReload = false;

    // Start is called before the first frame update
    void Start()
    {
        // Ignore collisions between the bug that fired the projectile and the projectile itself
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>());
        FriendlyFireOff();
        // Slinghsot mode uses data from 3 frames before the joystick recentered itself
        if (owner.GetComponent<Bug>().slingshotMode == true)
        {
            rb.velocity = transform.up * speed * owner.GetComponent<Bug>().currentCharge * owner.GetComponent<Bug>().joystickDrawSaveStates[2].magnitude;
        }
        // Manual mode uses current joystick data
        else
        {
            rb.velocity = transform.up * speed * owner.GetComponent<Bug>().currentCharge * owner.GetComponent<Bug>().joystickDraw.magnitude;
        }

        // Check to see if the stinger is overlapping a player or enemy on the first frame
        Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        opponent = Physics2D.OverlapCircle(pos, stingerSize, LayerMask.GetMask("Player"));
        if (opponent == null)
        {
            opponent = Physics2D.OverlapCircle(pos, stingerSize, LayerMask.GetMask("Enemy"));
        }
        if (opponent != null)
        {
            // Ensure that the stinger can't penetrate shields
            if (!Physics2D.OverlapCircle(pos, stingerSize, LayerMask.GetMask("Shield")))
            {
                damage = Mathf.RoundToInt(maxDamage * owner.GetComponent<Bug>().currentCharge);
                if (damage < minDamage)
                {
                    damage = minDamage;
                }
                opponent.gameObject.GetComponent<Bug>().Damage(damage);
                opponent.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
                Destroy(gameObject);
            }
            // If the stinger spawns inside a shielded enemy: don't deal any damage, give the player
            // their stinger back, and destroy the stinger
            else
            {
                owner.GetComponent<Bug>().StopRecharge();
                owner.GetComponent<Bug>().recharged = true;
                if (owner.GetComponent<Bug>().bugAnimator != null)
                {
                    owner.GetComponent<Bug>().bugAnimator.SetBool("IsRecharged", true);
                }
                Destroy(gameObject);
            }
        }
        // Reset bug's charge to zero
        owner.GetComponent<Bug>().currentCharge -= 1f / numProjectiles;
    }

    void Update()
    {
        // Tactical reload
        if (tacticalReload == true)
        {
            if (Physics2D.OverlapCircle(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), 
                stingerSize, owner.GetComponent<Bug>().defaultLayer))
            {
                // If the bug is currently reloading, stop it from doing so
                owner.GetComponent<Bug>().StopRecharge();
                owner.GetComponent<Bug>().recharged = true;
                if (superTacticalReload == true)
                {
                    owner.GetComponent<Bug>().currentCharge += charge;
                }
                if (owner.GetComponent<Bug>().bugAnimator != null)
                {
                    owner.GetComponent<Bug>().bugAnimator.SetBool("IsRecharged", true);
                }
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with terrain
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            // Deal 5 extra damage per bounce
            damage += damagePerBounce;
        }
        // On collision with a shield
        else if (collision.collider.gameObject.layer == shieldLayer)
        {
            // Shield script here
            ShieldDeflect(collision.collider.transform, collision.relativeVelocity.magnitude);
            // Reactivate collision between the bug that initially fired the projectile and the projectile itself
            Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>(), false);
            owner = collision.gameObject;
            // Damage is dealt based on how fast the stinger is moving
            int tempDamage = Mathf.RoundToInt(maxDamage * (collision.relativeVelocity.magnitude / speed));
            // Make sure that damage doesn't exceed the maximum
            if (tempDamage > maxDamage)
            {
                tempDamage = maxDamage;
            }
            // Make sure that the damage isn't less than the minimum
            else if (tempDamage < minDamage)
            {
                tempDamage = minDamage;
            }
            if (shieldPiercing == true)
            {
                bool poisoned = collision.gameObject.GetComponent<Bug>().poisoned;
                collision.gameObject.GetComponent<Bug>().Poison(damage + tempDamage, 0, 1);
                if (poisoned == false)
                {
                    collision.gameObject.GetComponent<Bug>().poisoned = false;
                }
                if (poisonDamage != 0)
                {
                    collision.gameObject.GetComponent<Bug>().Poison(poisonDamage, poisonInterval, numPoisonIntervals);
                }
            }
            else
            {
                owner.GetComponent<Bug>().Shield(damage + tempDamage);
            }
            owner.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
        // On collision with a player
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            // Damage is dealt based on how fast the stinger is moving
            int tempDamage = Mathf.RoundToInt(maxDamage * (collision.relativeVelocity.magnitude / speed));
            // Make sure that damage doesn't exceed the maximum
            if (tempDamage > maxDamage)
            {
                tempDamage = maxDamage;
            }
            // Make sure that the damage isn't less than the minimum
            else if (tempDamage < minDamage)
            {
                tempDamage = minDamage;
            }
            damage += tempDamage;
            if (toxic == true)
            {
                bool poisoned = collision.gameObject.GetComponent<Bug>().poisoned;
                collision.gameObject.GetComponent<Bug>().Poison(damage, 0, 1);
                if (poisoned == false)
                {
                    collision.gameObject.GetComponent<Bug>().poisoned = false;
                }
            }
            else
            {
                collision.gameObject.GetComponent<Bug>().Damage(damage);
            }
            if (poisonDamage != 0)
            {
                collision.gameObject.GetComponent<Bug>().Poison(poisonDamage, poisonInterval, numPoisonIntervals);
            }
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            Destroy(gameObject);
        }
    }
}
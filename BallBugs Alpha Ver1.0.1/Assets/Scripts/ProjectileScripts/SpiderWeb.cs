using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpiderWeb : Projectile
{
    // The LineRenderer that represents the strand of web spider swings from
    public LineRenderer strand;
    // The animator controller for when bugs get wrapped in web
    public RuntimeAnimatorController wrappedBug;
    // The amount of time an opponent remains wrapped
    public float wrappedTime = 1f;
    // The force with which the enemy is reeled in towards the spider
    public float reelPower = 1f;
    // Save state for the charge of the web when fired
    private float charge;

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
        // Reset bug's charge to zero
        charge = owner.GetComponent<Bug>().currentCharge;
        owner.GetComponent<Bug>().currentCharge -= 1f / numProjectiles;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with terrain
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            if (bounces < 0)
            {
                owner.GetComponent<Bug>().Ungrapple();
                owner.GetComponent<Bug>().currentWeb = gameObject;
                rb.simulated = false;
                gameObject.transform.SetParent(collision.gameObject.transform);
                owner.GetComponent<Bug>().Grapple(gameObject.transform.position);
            }
        }
        // On collision with a shield
        else if (collision.collider.gameObject.layer == shieldLayer)
        {
            // Shield script here
            ShieldDeflect(collision.collider.transform, collision.relativeVelocity.magnitude);
            // Reactivate collision between the bug that initially fired the projectile and the projectile itself
            Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>(), false);
            owner = collision.gameObject;
            int damage = maxDamage + comboDamage * owner.GetComponent<Bug>().comboCounter;
            owner.GetComponent<Bug>().Shield(damage);
        }
        // On collision with a player
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            int damage = maxDamage + comboDamage * owner.GetComponent<Bug>().comboCounter;
            collision.gameObject.GetComponent<Bug>().Damage(damage);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            collision.gameObject.GetComponent<Bug>().Wrap(wrappedBug, wrappedTime * charge);
            collision.gameObject.GetComponent<Bug>().SetVelocity(ReelIn(collision.gameObject.transform.position));
            Destroy(gameObject);
        }
    }

    public Vector2 ReelIn(Vector2 enemyPos)
    {
        // Get the direction the player is in relative to the enemy
        Vector2 pos = new Vector2(owner.GetComponent<Transform>().position.x, owner.GetComponent<Transform>().position.y);
        Vector2 distanceVector = new Vector2(pos.x - enemyPos.x, pos.y - enemyPos.y);
        return distanceVector.normalized * charge * reelPower;
    }

    void FixedUpdate()
    {
        owner.GetComponent<Bug>().UpdateGrapple(gameObject.transform.position, gameObject);
        // The projectile will rotate in the direction that it is facing
        rb.rotation = Mathf.Atan2(rb.velocity.x, rb.velocity.y) * Mathf.Rad2Deg * -1f;
        // Velocity is clamped so that the projectile never moves too fast
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, speed);
        // Update the first position of the line renderer to the web's position
        strand.SetPosition(0, gameObject.transform.position);
        // Update the second position of the line renderer to the spider's position
        strand.SetPosition(1, owner.GetComponent<Transform>().position);
    }
}

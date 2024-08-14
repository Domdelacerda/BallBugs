using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MosquitoProboscis : Projectile
{
    public float maxLifesteal = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        // Ignore collisions between the bug that fired the projectile and the projectile itself
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>());
        FriendlyFireOff();
        // Slinghsot mode uses data from 3 frames before the joystick recentered itself
        if (owner.GetComponent<Bug>().slingshotMode == true)
        {
            rb.velocity = transform.up * speed * charge * owner.GetComponent<Bug>().joystickDrawSaveStates[2].magnitude;
        }
        // Manual mode uses current joystick data
        else
        {
            rb.velocity = transform.up * speed * charge * owner.GetComponent<Bug>().joystickDraw.magnitude;
        }

        // Reset bug's charge to zero
        owner.GetComponent<Bug>().currentCharge -= 1f / numProjectiles;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with terrain
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
        }
        // On collision with a shield
        else if (collision.collider.gameObject.layer == shieldLayer)
        {
            // Shield script here
            ShieldDeflect(collision.collider.transform, collision.relativeVelocity.magnitude);
            // Reactivate collision between the bug that initially fired the projectile and the projectile itself
            Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>(), false);
            int damage = collision.gameObject.GetComponent<Bug>().Shield(maxDamage);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            int lifesteal = Mathf.RoundToInt(damage * maxLifesteal * charge);
            //owner.GetComponent<Bug>().Heal(lifesteal);
            owner = collision.gameObject;
        }
        // On collision with a player
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            int damage = collision.gameObject.GetComponent<Bug>().Damage(maxDamage);
            int lifesteal = Mathf.RoundToInt(damage * maxLifesteal * charge);
            owner.GetComponent<Bug>().Heal(lifesteal);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            Destroy(gameObject);
        }
    }
}

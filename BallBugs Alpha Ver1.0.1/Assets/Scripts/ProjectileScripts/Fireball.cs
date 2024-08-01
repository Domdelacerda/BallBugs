using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Projectile
{
    // Private damage field for calculations
    private int damage = 0;

    // When the fireball hits terrain, 1 is subtracted from its bounce counter
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            if (bounces < 0)
            {
                if (generateSecondaryEffect == true)
                {
                    Explode();
                }
                Destroy(gameObject);
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
            damage = Mathf.RoundToInt((gameObject.transform.localScale.x / 2f) * maxDamage);
            // Make sure that damage doesn't exceed the maximum
            if (damage > maxDamage)
            {
                damage = maxDamage;
            }
            // Make sure that the damage isn't less than the minimum
            else if (damage < minDamage)
            {
                damage = minDamage;
            }
            owner.GetComponent<Bug>().Shield(damage);
            owner.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            damage = Mathf.RoundToInt((gameObject.transform.localScale.x / 2f) * maxDamage);
            // Make sure that damage doesn't exceed the maximum
            if (damage > maxDamage)
            {
                damage = maxDamage;
            }
            // Make sure that the damage isn't less than the minimum
            else if (damage < minDamage)
            {
                damage = minDamage;
            }
            collision.gameObject.GetComponent<Bug>().Damage(damage);
            if (generateSecondaryEffect == true)
            {
                Explode();
            }
            else
            {
                collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            }
            Destroy(gameObject);
        }
    }

    void Explode()
    {
        GameObject secondaryEffect = Instantiate(secondaryEffectPrefab, gameObject.transform.position, gameObject.transform.rotation);
        secondaryEffect.GetComponent<Explosion>().owner = owner;
        secondaryEffect.transform.localScale = gameObject.transform.localScale * secondaryEffectSize;
    }
}
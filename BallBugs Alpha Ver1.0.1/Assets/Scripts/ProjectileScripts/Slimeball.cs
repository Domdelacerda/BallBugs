using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slimeball : Projectile
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var contact = collision.GetContact(0);
        var point = contact.point;
        var rotation = Quaternion.LookRotation(Vector3.forward, contact.normal);

        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            if (generateSecondaryEffect == true)
            {
                GameObject puddle = Instantiate(secondaryEffectPrefab, point, rotation);
                puddle.transform.localScale = gameObject.transform.localScale * secondaryEffectSize;
                puddle.GetComponent<SlimePuddle>().collisionTransform = collision.gameObject.transform;
                puddle.GetComponent<SlimePuddle>().owner = owner;
            }
            if (bounces < 0)
            {
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
            owner.GetComponent<Bug>().Shield(maxDamage);
            owner.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            collision.gameObject.GetComponent<Bug>().Damage(maxDamage);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            Destroy(gameObject);
        }
    }
}
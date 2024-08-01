using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrasshopperKick : Projectile
{
    public float jumpForce = 15f;
    public float charge = 0f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with terrain
        if (collision.gameObject.CompareTag("Terrain"))
        {
            // Fling the grasshopper backwards upon kicking the ground
            owner.GetComponent<Grasshopper>().Launch(jumpForce * charge);
            if (generateSecondaryEffect == true)
            {
                var contact = collision.GetContact(0);
                var point = contact.point;
                GameObject leftShock = Instantiate(secondaryEffectPrefab, point, Quaternion.LookRotation(Vector3.forward, 
                    RotateVector(contact.normal, Mathf.PI / 2))); 
                leftShock.transform.localScale *= charge + 1;
                int damage = leftShock.GetComponent<Shockwave>().damage;
                leftShock.GetComponent<Shockwave>().damage = Mathf.RoundToInt(damage * charge);
                leftShock.GetComponent<Shockwave>().owner = owner;
                GameObject rightShock = Instantiate(secondaryEffectPrefab, point, Quaternion.LookRotation(Vector3.forward,
                    RotateVector(contact.normal, Mathf.PI / -2)));
                rightShock.GetComponent<Shockwave>().damage = Mathf.RoundToInt(damage * charge);
                rightShock.transform.localScale *= charge + 1;
                rightShock.GetComponent<Shockwave>().owner = owner;
            }
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            owner.GetComponent<Bug>().Shield(0);
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Projectile>().owner.GetComponent<CircleCollider2D>(),
                collision.gameObject.GetComponent<Collider2D>(), false);
            collision.gameObject.GetComponent<Projectile>().owner = owner;
        }
        // On collision with a shield
        else if (collision.collider.gameObject.layer == shieldLayer)
        {
            collision.gameObject.GetComponent<Bug>().Shield(maxDamage);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
        // On collision with a player
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            int damage = Mathf.RoundToInt(maxDamage * charge);
            damage = Mathf.Max(damage, minDamage);
            collision.gameObject.GetComponent<Bug>().Damage(damage);
            if (invincibility == true)
            {
                collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            }
        }
        Destroy(gameObject);
    }

    // Rotate a supplied vector by a given angle
    Vector3 RotateVector(Vector3 vector, float angle)
    {
        float x = vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle);
        float y = vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle);
        return new Vector3(x, y, 0);
    }

    public void OnDestroy()
    {
        owner.GetComponent<Bug>().currentCharge = 0;
    }
}

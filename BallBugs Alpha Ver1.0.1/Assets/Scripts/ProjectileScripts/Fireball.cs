//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a fireball class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;

public class Fireball : Projectile
{
    /// <summary>--------------------------------------------------------------
    /// Fireball is a projectile that is fired by the Firefly character. The
    /// fireball deals damage based on how large it is.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Bouncing I, II: Increases the number of times the fireball can bounce
    /// off terrain before disappearing by 1 per upgrade level.
    /// 
    /// </summary>-------------------------------------------------------------

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            if (bounces < 0)
            {
                if (secondaryEffectPrefab != null && secondaryEffectSize != 0)
                {
                    Explode();
                }
                Destroy(gameObject);
            }
        }
        else if (collision.collider.gameObject.layer == SHIELD_LAYER)
        {
            ShieldDeflect(collision.collider.transform, 
                collision.relativeVelocity.magnitude);
            Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), 
                gameObject.GetComponent<Collider2D>(), false);
            owner = collision.gameObject;
            tempDamage = CalculateDamage();
            Bug bug = owner.GetComponent<Bug>();
            bug.Shield(tempDamage);
            bug.InvincibilityFrames(invincibilityTime);
        }
        else if (collision.gameObject.layer == PLAYER_LAYER 
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            tempDamage = CalculateDamage();
            Bug bug = collision.gameObject.GetComponent<Bug>();
            bug.Damage(tempDamage);
            if (secondaryEffectPrefab != null && secondaryEffectSize != 0)
            {
                Explode();
            }
            else
            {
                bug.InvincibilityFrames(invincibilityTime);
            }
            Destroy(gameObject);
        }
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Calculates damage based on size, and truncates the damage if it is too
    /// high or low.
    /// </summary>
    /// <returns>the amount of damage to deal.</returns>
    /// -----------------------------------------------------------------------
    private int CalculateDamage()
    {
        int tempDamage = Mathf.RoundToInt(charge * maxDamage);
        tempDamage = Mathf.Min(tempDamage, maxDamage);
        tempDamage = Mathf.Max(tempDamage, minDamage);
        return tempDamage;
    }

    /// <summary>--------------------------------------------------------------
    /// Generates an explosion with size proportional to the size of the
    /// initial fireball. Sets the explosion's owner to the fireball's owner.
    /// </summary>-------------------------------------------------------------
    private void Explode()
    {
        GameObject explosion = Instantiate(secondaryEffectPrefab,
            gameObject.transform.position, gameObject.transform.rotation);
        Explosion script = explosion.GetComponent<Explosion>();
        script.owner = owner;
        script.charge = charge;
        explosion.transform.localScale = gameObject.transform.localScale 
            * secondaryEffectSize;
    }
}
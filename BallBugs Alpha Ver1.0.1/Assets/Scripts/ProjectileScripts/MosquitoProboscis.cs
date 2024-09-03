//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a mosquito proboscis class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;

public class MosquitoProboscis : Projectile
{
    /// <summary>--------------------------------------------------------------
    /// Mosquito Proboscis is a projectile that is fired by the Mosquito 
    /// character. The proboscis deals constant damage, but steals health from
    /// the opponent based on the charge amount.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Lifesteal I, II III: Increases the maximum amount of lifesteal per
    /// proboscis hit by 10% per upgrade level.
    /// 
    /// </summary>-------------------------------------------------------------

    public float maxLifesteal = 0.5f;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), 
            gameObject.GetComponent<Collider2D>());
        FriendlyFireOff();
        Bug bug = owner.GetComponent<Bug>();
        if (bug.slingshotMode == true)
        {
            rb.velocity = transform.up * speed * charge 
                * bug.joystickDrawSaveStates[2].magnitude;
        }
        else
        {
            rb.velocity = transform.up * speed * charge 
                * bug.joystickDraw.magnitude;
        }
        bug.currentCharge -= 1f / numProjectiles;
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
        }
        else if (collision.collider.gameObject.layer == SHIELD_LAYER)
        {
            ShieldDeflect(collision.collider.transform, 
                collision.relativeVelocity.magnitude);
            Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), 
                gameObject.GetComponent<Collider2D>(), false);
            Bug bug = collision.gameObject.GetComponent<Bug>();
            bug.Shield(maxDamage);
            bug.InvincibilityFrames(invincibilityTime);
            owner = collision.gameObject;
        }
        else if (collision.gameObject.layer == PLAYER_LAYER 
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            Bug bug = collision.gameObject.GetComponent<Bug>();
            int damage = bug.Damage(maxDamage);
            int lifesteal = Mathf.RoundToInt(damage * maxLifesteal * charge);
            owner.GetComponent<Bug>().Heal(lifesteal);
            bug.InvincibilityFrames(invincibilityTime);
            Destroy(gameObject);
        }
    }
}

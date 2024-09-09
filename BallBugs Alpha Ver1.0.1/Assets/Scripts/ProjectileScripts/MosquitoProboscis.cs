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
    /// Piercing I, II: Increases the number of times the proboscis can 
    /// pierce through enemies by 1 per upgrade level. Proboscises no longer 
    /// knock back players or enemies with this upgrade. 
    /// Piercesteal: Each pierce through an enemy increases the lifesteal of
    /// the proboscis by 25%.
    /// 
    /// </summary>-------------------------------------------------------------

    public float maxLifesteal = 0.5f;
    public float lifestealPerPierce = 0.25f;
    private float lifesteal;

    public Collider2D solidCollider;
    public Collider2D trigger;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        lifesteal = maxLifesteal * charge;
        if (pierces > 0)
        {
            solidCollider.excludeLayers = LayerMask.GetMask("Player", "Enemy");
            trigger.includeLayers = LayerMask.GetMask("Player", "Enemy");
        }
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), 
            solidCollider);
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(),
            trigger);
        Bug bug = owner.GetComponent<Bug>();
        if (bug.defaultLayer == ENEMY_LAYER)
        {
            FriendlyFireOff();
        }
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
        else if (collision.gameObject.layer == SHIELD_LAYER)
        {
            ShieldCollision(collision.transform, collision.gameObject);
        }
        else if (collision.gameObject.layer == PLAYER_LAYER
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            PlayerCollision(collision.gameObject.GetComponent<Bug>());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == SHIELD_LAYER)
        {
            ShieldCollision(collision.transform, collision.gameObject);
        }
        else if (collision.gameObject.layer == PLAYER_LAYER
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            PlayerCollision(collision.gameObject.GetComponent<Bug>());
        }
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Handles a collision event with a shield, which reflects the projectile,
    /// deals shielded damage, and grants invincibility frames. Shield
    /// collisions block secondary effects, so no lifesteal is granted.
    /// </summary>
    /// <param name="oldOwner">the position of the previous owner.</param>
    /// <param name="newOwner">the new owner of the projectile to be assigned.
    /// </param>
    /// -----------------------------------------------------------------------
    private void ShieldCollision(Transform oldOwner, GameObject newOwner)
    {
        ShieldDeflect(oldOwner, rb.velocity.magnitude);
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(),
            solidCollider, false);
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(),
            trigger, false);
        owner = newOwner;
        Bug bug = owner.GetComponent<Bug>();
        bug.Shield(maxDamage);
        bug.InvincibilityFrames(invincibilityTime);
    }

    /// <summary>--------------------------------------------------------------
    /// Handles a collision event with another bug, which deals damage to the
    /// bug, grants invincibility frames, and heals the owner of the proboscis.
    /// If the proboscis' pierce count is zero, it is deleted.
    /// </summary>
    /// <param name="bug">the bug hit by the proboscis.</param>
    /// -----------------------------------------------------------------------
    private void PlayerCollision(Bug bug)
    {
        int damage = bug.Damage(maxDamage);
        int heal = Mathf.RoundToInt(damage * lifesteal);
        owner.GetComponent<Bug>().Heal(heal);
        if (pierces <= 0)
        {
            Destroy(gameObject);
        }
        if (bug.invincible == false)
        {
            pierces--;
            lifesteal += lifestealPerPierce;
        }
        bug.InvincibilityFrames(invincibilityTime);
    }
}
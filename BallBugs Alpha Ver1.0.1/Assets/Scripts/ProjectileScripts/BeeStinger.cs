//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a bee stinger class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class BeeStinger : Projectile
{
    /// <summary>--------------------------------------------------------------
    /// Bee Stinger is a projectile that is fired by the Bee character. The
    /// stinger deals damage based on how fast it is moving when it collides
    /// with a player.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Tactical Reload: If the owner of the stinger touches it after firing,
    /// the stinger will be destroyed and the owner will instantly reload.
    /// Super Tactical Reload: Upon tactical reload, the owner will also regain
    /// however much charge the projectile was initially fired with (Requires
    /// Tactical Reload).
    /// 
    /// Bouncing I, II: Increases the number of times the stinger can bounce
    /// off terrain before disappearing by 1 per upgrade level.
    /// Bouncing Combo: Each bounce increases the stinger's base damage by a
    /// small amount (Requires Bouncing I).
    /// 
    /// Piercing I, II, III: Increases the number of times the stinger can 
    /// pierce through enemies by 1 per upgrade level. Stingers no longer knock
    /// back players or enemies with this upgrade.
    /// Piercing Combo: Each pierce increases the stinger's base damage by a
    /// moderate amount (Requires Piercing I).
    /// 
    /// Poison I, II: Enemies hit by the stinger will be poisoned for a small
    /// amount of damage over time, increasing with each upgrade level.
    /// 
    /// Toxic: Stingers now deal poison damage in place of regular damage, 
    /// which ignores the armor stat of armored bugs.
    /// 
    /// </summary>-------------------------------------------------------------

    private int damage = 0;
    public float colliderDelay = 0.25f;

    public int poisonDamage = 0;
    public float poisonInterval = 0;
    public int numPoisonIntervals = 0;
    public bool toxic = false;
    public bool tacticalReload = false;
    public bool superTacticalReload = false;

    public Collider2D solidCollider;
    public Collider2D trigger;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        if (pierces > 0)
        {
            solidCollider.excludeLayers = LayerMask.GetMask("Player", "Enemy");
            trigger.includeLayers = LayerMask.GetMask("Player", "Enemy");
        }
        if (tacticalReload == false)
        {
            FriendlyFireOff();
        }
        ActivateTacticalReload();
        Bug bug = owner.GetComponent<Bug>();
        if (owner.GetComponent<Bug>().slingshotMode == true)
        {
            rb.velocity = transform.up * speed * bug.currentCharge 
                * bug.joystickDrawSaveStates[2].magnitude;
        }
        else
        {
            rb.velocity = transform.up * speed * bug.currentCharge 
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
            damage += bounceDamage;
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
            if (collision.gameObject == owner)
            {
                TacticalReload();
            }
            else
            {
                PlayerCollision(collision.gameObject.GetComponent<Bug>());
            }
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Delays the activation of tactical reload to ensure that the owner of
    /// the projectile doesn't reload on the first frame the stinger exists.
    /// </summary>
    /// <param name="delayTime">the amount of time it takes for tactical reload
    /// to activate.</param>
    /// <returns>the coroutine that re-enables tactical reloading.</returns>
    /// -----------------------------------------------------------------------
    private IEnumerator TacticalReloadDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        tacticalReload = true;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Calculates damage based on speed, and truncates the damage if it is too
    /// high or low.
    /// </summary>
    /// <returns>the amount of damage to deal.</returns>
    /// -----------------------------------------------------------------------
    private int CalculateDamage()
    {
        int tempDamage = Mathf.RoundToInt(maxDamage * (rb.velocity.magnitude 
            / speed));
        tempDamage = Mathf.Min(tempDamage, maxDamage);
        tempDamage = Mathf.Max(tempDamage, minDamage);
        return tempDamage;
    }

    /// <summary>--------------------------------------------------------------
    /// Handles a collision event with a shield, which reflects the projectile,
    /// deals shielded damage, and grants invincibility frames. Shield
    /// collisions block secondary effects, so the target is not poisoned.
    /// </summary>
    /// <param name="oldOwner">the position of the previous owner.</param>
    /// <param name="newOwner">the new owner of the projectile to be assigned.
    /// </param>
    /// -----------------------------------------------------------------------
    private void ShieldCollision(Transform oldOwner, GameObject newOwner)
    {
        ShieldDeflect(oldOwner, rb.velocity.magnitude);
        ActivateTacticalReload();
        owner = newOwner;
        tempDamage = CalculateDamage();
        Bug bug = owner.GetComponent<Bug>();
        bug.Shield(damage + tempDamage);
        bug.InvincibilityFrames(invincibilityTime);
    }

    /// <summary>--------------------------------------------------------------
    /// Handles a collision event with another bug, which deals damage to the
    /// bug, grants invincibility frames, and poisons the bug if poison is
    /// enabled. If the stinger's pierce count is zero, it is deleted.
    /// </summary>
    /// <param name="bug">the bug hit by the stinger.</param>
    /// -----------------------------------------------------------------------
    private void PlayerCollision(Bug bug)
    {
        tempDamage = CalculateDamage();
        if (toxic == true)
        {
            bool poisoned = bug.poisoned;
            bug.Poison(damage + tempDamage, 0, 1);
            if (poisoned == false)
            {
                bug.poisoned = false;
            }
        }
        else
        {
            bug.Damage(damage + tempDamage);
        }
        if (poisonDamage != 0 && bug.invincible == false)
        {
            bug.Poison(poisonDamage, poisonInterval, numPoisonIntervals);
        }
        if (pierces <= 0)
        {
            Destroy(gameObject);
        }
        if (bug.invincible == false)
        {
            pierces--;
            damage += pierceDamage;
        }
        bug.InvincibilityFrames(invincibilityTime);
    }

    /// <summary>--------------------------------------------------------------
    /// If tactical reload is enabled, start the delay so that the collision
    /// doesn't register until the stinger is fired.
    /// </summary>-------------------------------------------------------------
    private void ActivateTacticalReload()
    {
        if (tacticalReload == true)
        {
            tacticalReload = false;
            StartCoroutine(TacticalReloadDelay(colliderDelay));
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Reloads the owner of the stinger on contact, and if super tactical 
    /// reload is enabled, gives them charge equal to the amount of charge the
    /// stinger was initially fired with.
    /// </summary>-------------------------------------------------------------
    private void TacticalReload()
    {
        if (tacticalReload == true)
        {
            Bug bug = owner.GetComponent<Bug>();
            bug.StopRecharge();
            bug.recharged = true;
            if (superTacticalReload == true)
            {
                bug.currentCharge += charge;
                bug.currentCharge = Mathf.Min(bug.currentCharge, 1);
            }
            if (bug.bugAnimator != null)
            {
                bug.bugAnimator.SetBool("IsRecharged", true);
            }
            Destroy(gameObject);
        }
    }
}
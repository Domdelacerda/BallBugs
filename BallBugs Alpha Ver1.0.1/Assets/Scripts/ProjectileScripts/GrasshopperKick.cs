//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a grasshopper kick class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;

public class GrasshopperKick : Projectile
{
    /// <summary>--------------------------------------------------------------
    /// Grasshopper Kick is a projectile that is fired by the Grasshopper
    /// character. The kick deals damage and knockback based on the amount of
    /// charge when fired.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Jumping I, II, III, IV: Kicking terrain causes Grasshopper to super 
    /// jump, with launch power determined by charge amount. Each subsequent 
    /// upgrade level increases base jump power by 20%.
    /// 
    /// Shockwave: Kicking terrain creates 2 shockwaves that travel
    /// in opposite directions perpedicular to the terrain. The shockwaves
    /// increase in size and damage proportional to charge.
    /// 
    /// Deflection: Kicking projectiles deflects them, preventing all damage to
    /// grasshopper and launching the projectile in the direction of the kick.
    /// 
    /// </summary>-------------------------------------------------------------

    public Collider2D solidCollider;

    public float jumpForce = 15f;
    public bool deflection = true;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    private void Start()
    {
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(),
            gameObject.GetComponent<Collider2D>());
        Bug bug = owner.GetComponent<Bug>();
        if (bug.defaultLayer == ENEMY_LAYER)
        {
            FriendlyFireOff();
        }
        if (limitedLife == true)
        {
            Destroy(gameObject, lifetime);
        }
        if (fixedTrajectory == true)
        {
            rb.velocity = transform.up * speed;
        }
        else if (bug.slingshotMode == true)
        {
            rb.velocity = transform.up * speed
                * bug.joystickDrawSaveStates[2].magnitude;
        }
        else
        {
            rb.velocity = transform.up * speed
                * bug.joystickDrawSaveStates[0].magnitude;
        }
        if (numProjectiles > 1)
        {
            bug.currentCharge -= 1f / (numProjectiles - 1);
        }
        else if (numProjectiles == 1)
        {
            bug.currentCharge = 0;
        }
        if (deflection == true)
        {
            solidCollider.includeLayers = LayerMask.GetMask("Projectile");
        }
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            if (jumpForce != 0)
            {
                owner.GetComponent<Grasshopper>().Launch(jumpForce * charge);
            }
            if (secondaryEffectPrefab != null && secondaryEffectSize != 0)
            {
                ContactPoint2D contact = collision.GetContact(0);
                Vector2 point = contact.point;
                Shockwave(point, contact.normal, 1);
                Shockwave(point, contact.normal, -1);
            }
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            if (deflection == true)
            {
                owner.GetComponent<Bug>().Shield(0);
                Projectile projectile =
                    collision.gameObject.GetComponent<Projectile>();
                Physics2D.IgnoreCollision
                    (projectile.owner.GetComponent<CircleCollider2D>(),
                    collision.gameObject.GetComponent<Collider2D>(), false);
                projectile.owner = owner;
            }
        }
        else if (collision.collider.gameObject.layer == SHIELD_LAYER)
        {
            int damage = Mathf.RoundToInt(maxDamage * charge);
            damage = Mathf.Max(damage, minDamage);
            Bug bug = collision.gameObject.GetComponent<Bug>();
            bug.Shield(damage);
            bug.InvincibilityFrames(invincibilityTime);
        }
        else if (collision.gameObject.layer == PLAYER_LAYER 
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            int damage = Mathf.RoundToInt(maxDamage * charge);
            damage = Mathf.Max(damage, minDamage);
            Bug bug = collision.gameObject.GetComponent<Bug>();
            bug.Damage(damage);
            bug.InvincibilityFrames(invincibilityTime);
        }
        Destroy(gameObject);
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Rotates a vector by a given angle while preserving its magnitude.
    /// </summary>
    /// <param name="vector">the vector to be rotated.</param>
    /// <param name="angle">the angle to rotate the vector by in radians.
    /// </param>
    /// <returns>the original vector rotated by the angle.</returns>
    /// -----------------------------------------------------------------------
    private Vector3 RotateVector(Vector3 vector, float angle)
    {
        float x = vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle);
        float y = vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle);
        return new Vector3(x, y, 0);
    }

    /// <summary>--------------------------------------------------------------
    /// Generates a shockwave at a point, perpendicular to the given normal
    /// line, and in the provided direction.
    /// </summary>
    /// <param name="point">the position to instantiate the shockwave.</param>
    /// <param name="normal">the normal line that determines the rotation of
    /// the shockwave.</param>
    /// <param name="direction">the direction the shockwave will move, with 1
    /// representing right and -1 representing left.</param>
    /// -----------------------------------------------------------------------
    private void Shockwave(Vector2 point, Vector2 normal, int direction)
    {
        GameObject shockwave = Instantiate(secondaryEffectPrefab, point, 
            Quaternion.LookRotation(Vector3.forward, RotateVector(normal,
            Mathf.PI / (2 * direction))));
        shockwave.transform.localScale *= charge + 1;
        Shockwave script = shockwave.GetComponent<Shockwave>();
        script.damage = Mathf.RoundToInt(script.damage * charge);
        script.owner = owner;
    }
}

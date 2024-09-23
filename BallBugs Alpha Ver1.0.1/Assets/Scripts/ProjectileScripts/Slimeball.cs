//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a slimeball class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;

public class Slimeball : Projectile
{
    /// <summary>--------------------------------------------------------------
    /// Slimeball is a projectile that is fired by the Snail character. The
    /// slimeball deals constant damage.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Bouncing I, II: Increases the number of times the slimeball can bounce
    /// off terrain before disappearing by 1 per upgrade level.
    /// 
    /// </summary>-------------------------------------------------------------

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var contact = collision.GetContact(0);
        var point = contact.point;
        var rotation = Quaternion.LookRotation(Vector3.forward, 
            contact.normal);

        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            if (secondaryEffectPrefab != null && secondaryEffectSize != 0)
            {
                GameObject puddle = Instantiate(secondaryEffectPrefab, point, 
                    rotation);
                SlimePuddle script = puddle.GetComponent<SlimePuddle>();
                puddle.transform.localScale = gameObject.transform.localScale 
                    * secondaryEffectSize;
                script.collisionTransform = collision.gameObject.transform;
                script.owner = owner;
            }
            if (bounces < 0)
            {
                DetachBubbles();
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
            Bug bug = owner.GetComponent<Bug>();
            bug.Shield(maxDamage);
            bug.InvincibilityFrames(invincibilityTime);
        }
        else if (collision.gameObject.layer == PLAYER_LAYER 
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            Bug bug = collision.gameObject.GetComponent<Bug>();
            bug.Damage(maxDamage);
            bug.InvincibilityFrames(invincibilityTime);
            DetachBubbles();
            Destroy(gameObject);
        }
    }
}
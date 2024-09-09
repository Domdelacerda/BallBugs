//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a spider web class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;

public class SpiderWeb : Projectile
{
    /// <summary>--------------------------------------------------------------
    /// Spider Web is a projectile that is fired by the Spider character. The
    /// spider web deals very little damage but reels the target in towards the
    /// spider's mandibles. Upon contact with terrain, the web allows the
    /// spider that fired it to swing from it.
    /// </summary>-------------------------------------------------------------

    public LineRenderer strand;
    public RuntimeAnimatorController wrappedBug;
    
    public float wrappedTime = 1f;
    public float reelPower = 1f;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), 
            gameObject.GetComponent<Collider2D>());
        Bug bug = owner.GetComponent<Bug>();
        if (bug.defaultLayer == ENEMY_LAYER)
        {
            FriendlyFireOff();
        }
        if (bug.slingshotMode == true)
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

    void FixedUpdate()
    {
        owner.GetComponent<Bug>().UpdateGrapple(gameObject.transform.position,
            gameObject);
        rb.rotation = Mathf.Atan2(rb.velocity.x, rb.velocity.y)
            * Mathf.Rad2Deg * -1f;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, speed);
        strand.SetPosition(0, gameObject.transform.position);
        strand.SetPosition(1, owner.GetComponent<Transform>().position);
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            if (bounces < 0)
            {
                Bug bug = owner.GetComponent<Bug>();
                bug.Ungrapple();
                bug.currentWeb = gameObject;
                rb.simulated = false;
                gameObject.transform.SetParent(collision.gameObject.transform);
                bug.Grapple(gameObject.transform.position);
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
            int damage = maxDamage + comboDamage * bug.comboCounter;
            bug.Shield(damage);
            bug.InvincibilityFrames(invincibilityTime);
        }
        else if (collision.gameObject.layer == PLAYER_LAYER 
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            int damage = maxDamage + comboDamage 
                * owner.GetComponent<Bug>().comboCounter;
            Bug bug = collision.gameObject.GetComponent<Bug>();
            bug.Damage(damage);
            bug.InvincibilityFrames(invincibilityTime);
            bug.Wrap(wrappedBug, wrappedTime * charge);
            bug.SetVelocity(ReelIn(collision.gameObject.transform.position));
            Destroy(gameObject);
        }
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Determines the direction to pull the opponent toward the spider.
    /// </summary>
    /// <param name="enemyPos">the enemy's position.</param>
    /// <returns>the direction from the enemy to the spider.</returns>
    /// -----------------------------------------------------------------------
    public Vector2 ReelIn(Vector2 enemyPos)
    {
        Vector2 pos = new Vector2(owner.GetComponent<Transform>().position.x,
            owner.GetComponent<Transform>().position.y);
        Vector2 distanceVector = new Vector2(pos.x - enemyPos.x, 
            pos.y - enemyPos.y);
        return distanceVector.normalized * charge * reelPower;
    }
}

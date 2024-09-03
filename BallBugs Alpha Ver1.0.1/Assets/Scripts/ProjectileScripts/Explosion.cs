//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have an explosion class that functions as intended
//-----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Explosion is a secondary effect that is triggered when a Fireball
    /// is destroyed. The explosion lingers for a short amount of time, and
    /// deals damage equal to the original fireball's damage.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Knockback: The explosion now knocks back enemies it comes in
    /// contact with.
    /// Bomb Jump: The explosion now knocks back the player that fired it and
    /// its allies, allowing them to gain momentum from the blast without
    /// taking any damage.
    /// Blast Block: The explosion now knocks back enemy projectiles, granting
    /// ownership to the explosion's owner and potentially redirecting the
    /// projectile back at the enemy and dealing damage.
    /// Force I, II, III, IV: Knockback force for all knockback related 
    /// upgrades is increased by 20% per upgrade level.
    /// 
    /// </summary>-------------------------------------------------------------

    public bool colliderImmediate = false;
    public bool colliderDeactivate = true;

    private const float ANIMATION_DELAY = 0.15f;
    private const float ANIMATION_REFRAIN = 0.6f;
    public float lifetime = 1f;

    public bool knockback = true;
    public bool bombJump = true;
    public bool blastBlock = true;
    public float explosionForce = 50f;

    private int damage = 0;
    public int maxDamage = 30;
    public float charge = 0f;
    public float invincibilityTime = 0.5f;

    private const int PLAYER_LAYER = 9;
    private const int ENEMY_LAYER = 10;
    private const int SHIELD_LAYER = 11;

    public GameObject owner;
    public CircleCollider2D explosionCollider;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        Destroy(gameObject, lifetime);
        if (colliderImmediate == false)
        {
            explosionCollider.enabled = false;
            StartCoroutine(ColliderActivate());
        }
        else
        {
            explosionCollider.enabled = true;
        }
        if (colliderDeactivate == true)
        {
            StartCoroutine(ColliderDeactivate());
        }
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
        {
            if (other.gameObject.CompareTag("Projectile"))
            {
                if (blastBlock == true)
                {
                    owner.GetComponent<Bug>().Shield(0);
                    Projectile projectile = 
                        other.gameObject.GetComponent<Projectile>();
                    Physics2D.IgnoreCollision
                    (projectile.owner.GetComponent<CircleCollider2D>(),
                    other, false);
                    projectile.owner = owner;
                    Knockback(other);
                }
            }
            else if ((other.gameObject.layer == PLAYER_LAYER 
                || other.gameObject.layer == ENEMY_LAYER))
            {
                Bug bug = other.gameObject.GetComponent<Bug>();
                if (bug.defaultLayer != owner.GetComponent<Bug>().defaultLayer)
                {
                    if (knockback == true)
                    {
                        Knockback(other);
                    }
                    damage = Mathf.RoundToInt(charge * maxDamage);
                    bug.Damage(damage);
                    bug.InvincibilityFrames(invincibilityTime);
                }
                else
                {
                    if (bombJump == true)
                    {
                        Knockback(other);
                    }
                }
            }
            else if (other.gameObject.layer == SHIELD_LAYER)
            {
                Bug bug = other.gameObject.GetComponent<Bug>();
                if (bug.defaultLayer != owner.GetComponent<Bug>().defaultLayer)
                {
                    damage = Mathf.RoundToInt(charge * maxDamage);
                    bug.Shield(damage);
                    bug.InvincibilityFrames(invincibilityTime);
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Delays the activation of the explosion's collider to account for the
    /// time the animation takes to play out.
    /// </summary>
    /// <returns>the coroutine that re-enables the collider.</returns>
    /// -----------------------------------------------------------------------
    IEnumerator ColliderActivate()
    {
        yield return new WaitForSeconds(ANIMATION_DELAY);
        explosionCollider.enabled = true;
    }

    /// <summary>--------------------------------------------------------------
    /// Delays the activation of the explosion's collider to account for the
    /// time the animation takes to play out.
    /// </summary>
    /// <returns>the coroutine that re-enables the collider.</returns>
    /// -----------------------------------------------------------------------
    IEnumerator ColliderDeactivate()
    {
        yield return new WaitForSeconds(ANIMATION_REFRAIN);
        explosionCollider.enabled = false;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Launches a given collider's attached rigidbody away from the explosion
    /// based on the distance vector from the explosion's center to the other
    /// collider.
    /// </summary>
    /// <param name="other">the collider to be knocked back.</param>
    /// -----------------------------------------------------------------------
    private void Knockback(Collider2D other)
    {
        Vector2 thisPosition = gameObject.transform.localPosition;
        Vector2 otherPosition =
            other.gameObject.transform.localPosition;
        Vector2 distanceVector = new Vector2(otherPosition.x
            - thisPosition.x, otherPosition.y - thisPosition.y);
        other.attachedRigidbody.velocity = new Vector2(0, 0);
        other.attachedRigidbody.AddForce(distanceVector.normalized
            * explosionForce * gameObject.transform.localScale.x);
    }
}
//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a projectile class that other projectiles inherit from
//-----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Projectile contains attributes and methods common among all 
    /// projectiles: The projectile's speed when fired, the amount of bounces
    /// it has before it disappears, the minimum and maximum damage, and the
    /// amount of invincibility frames an enemy gets when they are hit with the
    /// projectile. Projectile exists so that the more specific projectile 
    /// classes don't have to repeat the same code and can instead use 
    /// inheritance.
    /// </summary>-------------------------------------------------------------

    public float speed = 10f;
    public float charge = 0f;

    protected int tempDamage = 0;
    public int maxDamage = 25;
    public int minDamage = 1;
    public int comboDamage = 0;
    public int bounceDamage = 0;
    public int pierceDamage = 0;

    public int bounces = 0;
    public int pierces = 0;
    public int numProjectiles = 1;

    public float invincibilityTime = 0f;
    public float lifetime = 1f;

    public Rigidbody2D rb;
    public GameObject owner;

    public GameObject secondaryEffectPrefab;
    public float secondaryEffectSize = 1f;
    public bool limitedLife = false;
    public bool fixedTrajectory = false;

    private const float SHIELD_DEFLECT_POWER = 1.5f;
    private const float BUBBLE_LIFESPAN = 5f;

    protected const int PLAYER_LAYER = 9;
    protected const int ENEMY_LAYER = 10;
    protected const int SHIELD_LAYER = 11;

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
        if (limitedLife == true)
        {
            StartCoroutine(DetachDelay());
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
    }

    void FixedUpdate()
    {
        if (fixedTrajectory == false)
        {
            rb.rotation = Mathf.Atan2(rb.velocity.x, rb.velocity.y) 
                * Mathf.Rad2Deg * -1f;
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, speed);
        }
        if (secondaryEffectSize == 0 || secondaryEffectPrefab == null)
        {
            if (bounces < 0)
            {
                DetachBubbles();
                Destroy(gameObject);
            }
        }
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            bounces--;
            tempDamage += bounceDamage;
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
            if (invincibilityTime != 0)
            {
                bug.InvincibilityFrames(invincibilityTime);
            }
        }
        else if (collision.gameObject.layer == PLAYER_LAYER 
            || collision.gameObject.layer == ENEMY_LAYER)
        {
            Bug ownerBug = owner.GetComponent<Bug>();
            Bug enemyBug = collision.gameObject.GetComponent<Bug>();
            int damage = maxDamage + tempDamage + comboDamage 
                * ownerBug.comboCounter;
            enemyBug.Damage(damage);
            if (invincibilityTime != 0)
            {
                enemyBug.InvincibilityFrames(invincibilityTime);
            }
            ownerBug.comboCounter++;
            if (pierces <= 0)
            {
                DetachBubbles();
                Destroy(gameObject);
            }
            else
            {
                tempDamage += pierceDamage;
                pierces--;
            }
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Detach the bubble particles from the projectile the frame before it
    /// is destroyed.
    /// </summary>
    /// <returns>coroutine that executes bubble detachment event.</returns>
    /// -----------------------------------------------------------------------
    public IEnumerator DetachDelay()
    {
        yield return new WaitForSeconds(lifetime - Time.fixedDeltaTime);
        DetachBubbles();
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Redirects a projectile's velocity towards its original owner after
    /// being deflected.
    /// </summary>
    /// <param name="other">the positition of the projectile's previous owner.
    /// </param>
    /// <param name="speed">the speed of the projectile that determines how
    /// fast it will be reflected.</param>
    /// -----------------------------------------------------------------------
    public void ShieldDeflect(Transform other, float speed)
    {
        Vector3 direction = new Vector3(owner.transform.position.x 
            - other.position.x,
            owner.transform.position.y - other.position.y, 
            owner.transform.position.z - other.position.z);
        rb.velocity = direction.normalized * speed * SHIELD_DEFLECT_POWER;
    }

    /// <summary>--------------------------------------------------------------
    /// Detach the bubble particles from the projectile so that the bubbles
    /// don't immediately disappear when the projectile is destroyed.
    /// </summary>-------------------------------------------------------------
    public void DetachBubbles()
    {
        ParticleSystem bubbles =
            gameObject.GetComponentInChildren<ParticleSystem>();
        if (bubbles != null)
        {
            bubbles.transform.SetParent(null, false);
            bubbles.transform.position = gameObject.transform.position;
            bubbles.Stop();
            Destroy(bubbles.gameObject, BUBBLE_LIFESPAN);
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Turns off collision between the projectile and all players on the
    /// owner's collision layer
    /// </summary>-------------------------------------------------------------
    public void FriendlyFireOff()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Bug");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].layer == owner.GetComponent<Bug>().defaultLayer)
            {
                Collider2D[] colliders = 
                    gameObject.GetComponents<Collider2D>();
                for (int j = 0; j < colliders.Length; j++)
                {
                    Physics2D.IgnoreCollision
                    (players[i].GetComponent<CircleCollider2D>(), 
                    colliders[j]);
                }
            }
        }
    }
}
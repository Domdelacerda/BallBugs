using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // The colliderImmediate bool determines whether the collider is active when the
    // Explosion is instantiated or not; set to false to account for the animation delay
    public bool colliderImmediate = false;

    // The colliderDeactivate bool determines whether the collider is deactivated after
    // The explosion animation starts to recede; set to true to account for this
    public bool colliderDeactivate = true;

    // The knockback bool determines whether or not the explosion will knock other
    // Rigidbodies away
    public bool knockback = true;

    // The animationDelay float determines how much time is elapsed waiting for the
    // Explosion animation to play out before activating the explosion collider
    private const float animationDelay = 0.15f;

    // The animationRefrain float determines how much time is elapsed waiting for the
    // Explosion animation to play out before deactivating the explosion collider
    private const float animationRefrain = 0.6f;

    // The lifetime float determines how long the explosion lasts before being destroyed
    public float lifetime = 1f;

    // The explosionForce float determines how much force is applied to rigidbodies that
    // Enter the explosion's radius
    public float explosionForce = 50f;

    // The maximum damage dealt by the explosion
    public int maxDamage = 30;

    // The amount of invincibility time in seconds that this projectile gives
    public float invincibilityTime = 0.5f;

    // The playerLayer int represents the layer that all player characters exist on
    private const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;
    // The shield layer int represents the layer that all shields exist on
    protected const int shieldLayer = 11;

    // The firefly GameObject is the firefly that caused this explosion. Previously used
    // GameObject.FindWithTag, but it wouldn't have worked with multiple fireflies
    public GameObject owner;

    // The explosionCollider CircleCollider2D is the trigger collider of the explosion
    public CircleCollider2D explosionCollider;

    // Private damage field for calculations
    public int damage = 0;

    // Start is called before the first frame update
    void Start()
    {
        FriendlyFireOff();
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
        {
            if (knockback == true)
            {
                Vector2 thisPosition = new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y);
                Vector2 otherPosition = new Vector2(other.gameObject.transform.localPosition.x, other.gameObject.transform.localPosition.y);
                Vector2 distanceVector = new Vector2(otherPosition.x - thisPosition.x, otherPosition.y - thisPosition.y);

                other.attachedRigidbody.velocity = new Vector2(0, 0);
                other.attachedRigidbody.AddForce(distanceVector.normalized * explosionForce * gameObject.transform.localScale.x);
            }
            // If the game object that entered the trigger is the firefly that created the explosion, they won't take damage
            if ((other.gameObject.layer == playerLayer || other.gameObject.layer == enemyLayer) 
                && other.gameObject.GetInstanceID() != owner.gameObject.GetInstanceID())
            {
                // Magic number 3.3 which is the maximum size an explosion can be. CHANGE THIS!
                damage = Mathf.RoundToInt((gameObject.transform.localScale.x / 3.3f) * maxDamage);
                other.gameObject.GetComponent<Bug>().Damage(damage);
                other.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            }
            else if (other.gameObject.layer == shieldLayer && other.gameObject.GetInstanceID() != owner.gameObject.GetInstanceID())
            {
                // Magic number 3.3 which is the maximum size an explosion can be. CHANGE THIS!
                damage = Mathf.RoundToInt((gameObject.transform.localScale.x / 3.3f) * maxDamage);
                other.gameObject.GetComponent<Bug>().Shield(damage);
                other.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            }
        }
    }

    public void FriendlyFireOff()
    {
        if (owner.layer == enemyLayer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Bug");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].layer == enemyLayer)
                {
                    Physics2D.IgnoreCollision(players[i].GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>());
                }
            }
        }
    }

    IEnumerator ColliderActivate()
    {
        yield return new WaitForSeconds(animationDelay);
        explosionCollider.enabled = true;
    }

    IEnumerator ColliderDeactivate()
    {
        yield return new WaitForSeconds(animationRefrain);
        explosionCollider.enabled = false;
    }
}

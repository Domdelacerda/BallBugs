using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public GameObject bubblesPrefab;
    public GameObject splashPrefab;

    // The playerInWaterGravityScale float represents the factor by which the player's gravity
    // Scale is multiplied by upon entering the water / divided by when exiting the water
    public float playerInWaterGravityScale = 0.05f;
    // The playerInWaterDragScale float represents the factor by which the player's linear
    // Drag is multiplied by upon entering the water / divided by when exiting the water
    public float playerInWaterDragScale = 0.25f;

    // The projectileInWaterGravityScale float represents the factor by which a projectile's
    // Gravity scale is multiplied by upon entering the water / divided by when exiting the water
    public float projectileInWaterGravityScale = 0.5f;
    // The projectileInWaterDragScale float represents the factor by which a projectile's
    // Linear drag is multiplied by upon entering the water / divided by when exiting the water
    public float projectileInWaterDragScale = 4f;

    // The playerLayer int represents the layer that all player characters exist on
    private const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;
    // The shield layer int represents the layer that all shields exist on
    protected const int shieldLayer = 11;

    // Upon entering the water:
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Projectile")
        {
            other.attachedRigidbody.gravityScale = other.attachedRigidbody.gravityScale * projectileInWaterGravityScale;
            other.attachedRigidbody.drag = other.attachedRigidbody.drag * projectileInWaterDragScale;
            GameObject bubbles = Instantiate(bubblesPrefab, other.transform);
            bubbles.GetComponent<ParticleSystem>().trigger.AddCollider(gameObject.GetComponent<Collider2D>());
        }
        else if (other.gameObject.layer == playerLayer || other.gameObject.layer == enemyLayer || other.gameObject.layer == shieldLayer)
        {
            other.attachedRigidbody.gravityScale = other.attachedRigidbody.gravityScale * playerInWaterGravityScale;
            other.attachedRigidbody.drag = other.attachedRigidbody.drag * playerInWaterDragScale;
        }
        GameObject splash = Instantiate(splashPrefab, other.transform.localPosition, Quaternion.identity);
        splash.GetComponent<ParticleSystem>().trigger.AddCollider(gameObject.GetComponent<Collider2D>());
    }

    // Upon exiting the water:
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Projectile")
        {
            other.attachedRigidbody.gravityScale = other.attachedRigidbody.gravityScale / projectileInWaterGravityScale;
            other.attachedRigidbody.drag = other.attachedRigidbody.drag / projectileInWaterDragScale;
            Destroy(other.GetComponentInChildren<ParticleSystem>().gameObject);
        }
        else if (other.gameObject.layer == playerLayer || other.gameObject.layer == enemyLayer || other.gameObject.layer == shieldLayer)
        {
            other.attachedRigidbody.gravityScale = other.attachedRigidbody.gravityScale / playerInWaterGravityScale;
            other.attachedRigidbody.drag = other.attachedRigidbody.drag / playerInWaterDragScale;
        }
        GameObject splash = Instantiate(splashPrefab, other.transform.localPosition, Quaternion.identity);
        ParticleSystem particles = splash.GetComponent<ParticleSystem>();
        particles.trigger.AddCollider(gameObject.GetComponent<Collider2D>());
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    // The speed of the projectile the moment it is fired
    public float speed = 10f;

    // The maximum amount of damage that this projectile can deal
    public int damage = 20;
    // The damage dealt per consecutive hit
    public int comboDamage = 0;
    // The combo counter used for tracking consecutive hits
    private int comboCounter = 0;

    // The amount of invincibility time in seconds that this projectile gives
    public float invincibilityTime = 0f;

    // The rigidbody component of this projectile
    public Rigidbody2D rb;

    // The bug that fired this projectile A.K.A the bug who won't be damaged by it
    public GameObject owner;
    // The point that checks whether terrain has run out or not
    public Transform checkPoint;

    // The player layer int represents the layer that all player characters exist on
    protected const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;
    // The shield layer int represents the layer that all shields exist on
    protected const int shieldLayer = 11;

    private const float searchRadius = 0.1f;

    void Start()
    {
        // Ignore collisions between the bug that fired the projectile and the projectile itself
        Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>());
        FriendlyFireOff();
        Vector2 point = new Vector2(checkPoint.position.x, transform.position.y);
        if (!Physics2D.OverlapCircle(point, searchRadius, LayerMask.GetMask("Terrain")))
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.GetComponent<Collider2D>().enabled = true;
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            rb.velocity = transform.up * speed;
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == shieldLayer)
        {
            rb.velocity *= -1;
            Physics2D.IgnoreCollision(owner.GetComponent<CircleCollider2D>(), gameObject.GetComponent<Collider2D>(), false);
            owner = collision.gameObject;
            int totalDamage = damage + comboDamage * comboCounter;
            owner.GetComponent<Bug>().Shield(totalDamage);
            owner.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            comboCounter++;
        }
        else if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer)
        {
            int totalDamage = damage + comboDamage * comboCounter;
            collision.gameObject.GetComponent<Bug>().Damage(totalDamage);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
            comboCounter++;
        }
    }

    public void FixedUpdate()
    {
        Vector2 point = new Vector2(checkPoint.position.x, transform.position.y);
        if (!Physics2D.OverlapCircle(point, searchRadius, LayerMask.GetMask("Terrain")))
        {
            Destroy(gameObject);
        }
    }
}

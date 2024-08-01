using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    // The amount of damage spikes deal when touching them
    public int damage = 20;
    // The amount of invincibility time in seconds touching spikes gives
    public float invincibilityTime = 1.0f;

    public int poisonDamage = 3;
    public float poisonInterval = 1;
    public int numPoisonIntervals = 5;

    // The player layer int represents the layer that all player characters exist on
    protected const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;

    void Start()
    {
        FriendlyFireOff();
    }

    public void FriendlyFireOff()
    {
        if (gameObject.transform.parent != null)
        {
            if (gameObject.transform.parent.gameObject.layer == enemyLayer)
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
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == playerLayer || (collision.gameObject.layer == enemyLayer))
        {
            collision.gameObject.GetComponent<Bug>().Damage(damage);
            if (poisonDamage != 0)
            {
                collision.gameObject.GetComponent<Bug>().Poison(poisonDamage, poisonInterval, numPoisonIntervals);
            }
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer || (collision.gameObject.layer == enemyLayer))
        {
            collision.gameObject.GetComponent<Bug>().Damage(damage);
            if (poisonDamage != 0)
            {
                collision.gameObject.GetComponent<Bug>().Poison(poisonDamage, poisonInterval, numPoisonIntervals);
            }
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
    }
}

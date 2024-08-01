using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    // The amount of damage spikes deal when touching them
    public int damage = 20;
    // The amount of invincibility time in seconds touching spikes gives
    public float invincibilityTime = 1.0f;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bug")
        {
            collision.gameObject.GetComponent<Bug>().Damage(damage);
            collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
        else
        {
            Destroy(collision.gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Honeydrop : MonoBehaviour
{
    // The point in-game that moves around and assigns its coordinates to the collectible
    public Transform randomPoint;

    // The playerLayer int represents the layer that all player characters exist on
    private const int playerLayer = 9;
    private const int enemyLayer = 10;
    private const int shieldLayer = 11;

    private void Pickup()
    {
        // The random point game object runs a script that places it in a new random position
        randomPoint.GetComponent<PositionGenerator>().GeneratePosition();

        // Move the collectible to its new random position
        gameObject.transform.position = new Vector3(randomPoint.position.x, randomPoint.position.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer || collision.gameObject.layer == shieldLayer)
        {
            if (collision.gameObject.GetComponent<Bug>() != null)
            {
                // When a player picks up the collectible, increment their score and heal them for 5 HP
                collision.gameObject.GetComponent<Bug>().Heal(5);
                collision.gameObject.GetComponent<Bug>().score++;
                collision.gameObject.GetComponent<Bug>().scoreText.text = collision.gameObject.GetComponent<Bug>().score.ToString();
            }
            else
            {
                // When a player picks up the collectible, increment their score and heal them for 5 HP
                collision.gameObject.GetComponentInParent<Bug>().Heal(5);
                collision.gameObject.GetComponentInParent<Bug>().score++;
                collision.gameObject.GetComponentInParent<Bug>().scoreText.text = collision.gameObject.GetComponent<Bug>().score.ToString();
            }

            // Perform pickup function to create a new collectible and delete the old one
            Pickup();
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            // When a projectile picks up the collectible, find the owner of the projectile and
            // increment their score
            collision.gameObject.GetComponent<Projectile>().owner.GetComponent<Bug>().score++;
            collision.gameObject.GetComponent<Projectile>().owner.GetComponent<Bug>().scoreText.text = 
                collision.gameObject.GetComponent<Projectile>().owner.GetComponent<Bug>().score.ToString();

            // Perform pickup function to create a new collectible and delete the old one
            Pickup();
        }
    }
}
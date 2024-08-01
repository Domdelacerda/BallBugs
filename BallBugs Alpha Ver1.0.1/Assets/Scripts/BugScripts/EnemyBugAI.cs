using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnemyBugAI : MonoBehaviour
{
    // Rigidbody for the enemy
    public Rigidbody2D rb;

    // The distance from which the enemy is able to detect targets
    public int detectionRadius = 10;
    // The speed at which the enemy rotates toward the player
    public float rotationSpeed = 0.01f;

    // The image of the health bar foreground
    public Image healthBarForeground;
    // The image of the health bar foreground
    public Image healthBarBackground;

    // The collider of the player currently being targeted by the enemy
    private Collider2D target;

    // Distance vector used for determining which way the enemy should face
    private Vector3 distanceVector;

    // The playerLayer int represents the layer that all player characters exist on
    protected const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;

    // Start is called on the first frame
    void Start()
    {
        // All that is needed to turn a prefab from player to enemy is switching between
        // the player and enemy layers in the editor. This script handles the rest
        if (gameObject.layer == enemyLayer)
        {
            gameObject.GetComponent<PlayerInput>().enabled = false;
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            healthBarForeground.color = new Color(0.7f, 0, 0, 1);
            healthBarBackground.color = new Color(0.2f, 0, 0, 1);
        }
        else
        {
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        // If the enemy does not already have a target
        if (target == null)
        {
            // Search for a new target within the detection radius
            target = Physics2D.OverlapCircle(pos, detectionRadius, LayerMask.GetMask("Player"));
        }
        // If the enemy detects a player in its detection radius
        else if (target != null && gameObject.GetComponent<Bug>().wrapped == false)
        {
            // Get the direction the player is in relative to the enemy
            Vector2 targetPos = new Vector2(target.gameObject.transform.localPosition.x, target.gameObject.transform.localPosition.y);
            distanceVector = new Vector2(targetPos.x - pos.x, targetPos.y - pos.y);
            distanceVector = distanceVector.normalized;
            // Move the enemy in the direction of the player
            rb.AddForce(distanceVector * gameObject.GetComponent<PlayerMovement>().moveSpeed);
            // Rotate the enemy towards the player
            gameObject.GetComponent<Bug>().joystickDraw = -distanceVector;
        }
        else if (gameObject.GetComponent<Bug>().wrapped == true)
        {
            gameObject.GetComponent<Bug>().shoot = false;
            gameObject.GetComponent<Bug>().primed = false;
            gameObject.GetComponent<Bug>().joystickDraw = Vector2.zero;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Rotation method using quaternions and lerp for slow rotation; don't use :(
            /*
            distanceVector = target.transform.position - gameObject.transform.position;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, -distanceVector);
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, rotation, Time.time * rotationSpeed);
            */
            if (gameObject.GetComponent<Bug>().recharged == true && gameObject.GetComponent<Bug>().currentWeb != null)
            {
                gameObject.GetComponent<Bug>().Ungrapple();
            }
            if (gameObject.GetComponent<Bug>().currentCharge == 1f)
            {
                gameObject.GetComponent<Bug>().shoot = true;
            }
        }
    }
}

//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have enemy AI that is challenging for players to battle against
//-----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnemyBugAI : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// The enemy bug AI causes any bug tagged as an "enemy" to exhibit
    /// aggressive behavior toward player characters. Enemies will relentlessly
    /// chase down their target as soon as they enter theur detection radius.
    /// The enemy will also periodically charge up attacks and unleash them on
    /// their target once at max charge.
    /// </summary>-------------------------------------------------------------

    public Rigidbody2D rb;

    public int detectionRadius = 10;
    public float rotationSpeed = 0.01f;

    public Image healthBarForeground;
    public Image healthBarBackground;

    private Collider2D target;
    private Vector3 distanceVector;

    protected const int playerLayer = 9;
    protected const int enemyLayer = 10;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        if (gameObject.layer == enemyLayer)
        {
            gameObject.GetComponent<PlayerInput>().enabled = false;
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            healthBarForeground.color = new Color(0.7f, 0, 0, 1);
            healthBarBackground.color = new Color(0.2f, 0, 0, 1);
        }
        else
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (target != null)
        {
            if (gameObject.GetComponent<Bug>().recharged == true 
                && gameObject.GetComponent<Bug>().currentWeb != null)
            {
                gameObject.GetComponent<Bug>().Ungrapple();
            }
            if (gameObject.GetComponent<Bug>().currentCharge == 1f)
            {
                gameObject.GetComponent<Bug>().shoot = true;
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 pos = new Vector2(gameObject.transform.position.x, 
            gameObject.transform.position.y);
        if (target == null)
        {
            target = Physics2D.OverlapCircle(pos, detectionRadius, 
                LayerMask.GetMask("Player"));
        }
        else if (target != null && gameObject.GetComponent<Bug>().wrapped
            == false)
        {
            Vector2 targetPos = new Vector2
                (target.gameObject.transform.localPosition.x, 
                target.gameObject.transform.localPosition.y);
            distanceVector = new Vector2(targetPos.x - pos.x, targetPos.y 
                - pos.y);
            distanceVector = distanceVector.normalized;
            rb.AddForce(distanceVector 
                * gameObject.GetComponent<PlayerMovement>().moveSpeed);
            gameObject.GetComponent<Bug>().joystickDraw = -distanceVector;
        }
        else if (gameObject.GetComponent<Bug>().wrapped == true)
        {
            gameObject.GetComponent<Bug>().shoot = false;
            gameObject.GetComponent<Bug>().primed = false;
            gameObject.GetComponent<Bug>().joystickDraw = Vector2.zero;
        }
    }
}

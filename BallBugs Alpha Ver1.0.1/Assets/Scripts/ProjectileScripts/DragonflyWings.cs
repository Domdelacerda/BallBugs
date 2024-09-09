//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a dragonfly winga class that functions as intended
//-----------------------------------------------------------------------------

using UnityEngine;

public class DragonflyWings : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Dragonfly wings deal damage to other bugs while the dragonfly that owns
    /// them is flying. Damage is dealt based on dragonfly's current charge.
    /// </summary>-------------------------------------------------------------
    
    public GameObject owner;
    public int maxDamage = 10;
    public float invincibilityTime = 0.5f;

    private const int PLAYER_LAYER = 9;
    private const int ENEMY_LAYER = 10;
    private const int SHIELD_LAYER = 11;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    private void Start()
    {
        Physics2D.IgnoreCollision(owner.GetComponent<Collider2D>(), 
            gameObject.GetComponent<Collider2D>());
        Bug bug = owner.GetComponent<Bug>();
        if (bug.defaultLayer == ENEMY_LAYER)
        {
            FriendlyFireOff(bug);
        }
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bug")
        {
            Bug bug = collision.gameObject.GetComponent<Bug>();
            if (bug.defaultLayer == SHIELD_LAYER)
            {
                bug.Shield(maxDamage);
                bug.InvincibilityFrames(invincibilityTime);
            }
            else if (bug.defaultLayer == PLAYER_LAYER
                || bug.defaultLayer == ENEMY_LAYER)
            {
                bug.Damage(maxDamage);
                bug.InvincibilityFrames(invincibilityTime);
            }
        }
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Turns off collision between the projectile and all players on the
    /// owner's collision layer
    /// </summary>-------------------------------------------------------------
    public void FriendlyFireOff(Bug bug)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Bug");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].layer == bug.defaultLayer)
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
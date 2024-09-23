//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a honeydrop that players can pick up
//-----------------------------------------------------------------------------

using UnityEngine;

public class Honeydrop : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Honeydrop is a collectible item in the game that players and their
    /// projectiles are able to interact with. If a player touches the honey
    /// drop, they will regain a small amount of health and gain a point.
    /// They can also collect the drop with a projectile they fire, and while
    /// they will still gain a point, they won't regain helth.
    /// </summary>-------------------------------------------------------------

    public Transform randomPoint;
    public int health = 5;

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject collector = collision.gameObject;
        if (collector.CompareTag("Bug"))
        {
            Bug bug = collector.GetComponent<Bug>();
            if (bug == null)
            {
                bug = collector.GetComponentInParent<Bug>();
            }
            Pickup(bug, true);
        }
        else if (collector.CompareTag("Projectile"))
        {
            Projectile projectile = collector.GetComponent<Projectile>();
            Bug bug = projectile.owner.GetComponent<Bug>();
            Pickup(bug, false);
        }
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Picks up this honey drop and gives the benefits to the player that
    /// picked it up. The random point variable has a new random position
    /// generated and then sets the honey drop's position to it.
    /// </summary>
    /// <param name="bug">the bug that will recieve the benefits.</param>
    /// <param name="heal">whether the bug gets healed or not.</param>
    /// -----------------------------------------------------------------------
    private void Pickup(Bug bug, bool heal)
    {
        if (heal == true)
        {
            bug.Heal(health);
        }
        bug.score++;
        bug.scoreText.text = bug.score.ToString();
        randomPoint.GetComponent<PositionGenerator>().GeneratePosition();
        gameObject.transform.position = randomPoint.position;
    }
}
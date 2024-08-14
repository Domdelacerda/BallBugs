//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a grasshopper class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;

public class Grasshopper : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Grasshopper is one of the player characters in the game. Grasshopper is
    /// a melee class that attacks with powerful kicks that knock enemies
    /// backward. The charge meter determines the damage the kicks deal as well
    /// as the amount of knockback applied to the target. Similarly, when
    /// Grasshopper kicks terrain, it will launch itself off the ground with
    /// force proportional to current charge.
    /// </summary>-------------------------------------------------------------

    public Transform firePoint;
    public GameObject kickPrefab;
    public GameObject visualizer;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Update()
    {
        if (joystickDraw.magnitude == 0f && recharged == true && primed == true
            && slingshotControls == true && wrapped == false)
        {
            slingshotMode = true;
            Sling();
            Release();
        }
        else if (joystickDraw.magnitude != 0f && recharged == true && 
            wrapped == false)
        {
            ChargingUp(true);
            SetVisualizerActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
            if (shoot == true)
            {
                slingshotMode = false;
                Sling();
                Release();
            }
        }
        else
        {
            SetVisualizerActive(false);
            currentCharge = 0f;
        }
        shoot = false;
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new instance of the kick prefab, increases its mass depending
    /// the current charge, and fires it.
    /// </summary>-------------------------------------------------------------
    public void Sling()
    {
        GameObject kick = Instantiate(kickPrefab, firePoint.position, 
            firePoint.rotation);
        kick.GetComponent<Rigidbody2D>().mass *= currentCharge;
        kick.GetComponent<Projectile>().owner = gameObject;
        kick.GetComponent<Projectile>().charge = currentCharge;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Launches grasshopper in the direction opposite from the direction it
    /// fired with power based on the current charge.
    /// </summary>
    /// <param name="charge">whether the visualizer is being enabled or 
    /// disabled.</param>
    /// -----------------------------------------------------------------------
    public void Launch(float charge)
    {
        Vector2 direction = new Vector2(gameObject.transform.position.x 
            - firePoint.position.x,
            gameObject.transform.position.y - firePoint.position.y);
        rb.velocity = direction.normalized * charge;
    }

    /// <summary>--------------------------------------------------------------
    /// Enables or disables the trajectory visualizer to see where a shot is 
    /// going to be fired.
    /// </summary>
    /// <param name="active">whether the visualizer is being enabled or 
    /// disabled.</param>
    /// -----------------------------------------------------------------------
    void SetVisualizerActive(bool active)
    {
        visualizer.GetComponent<Renderer>().enabled = active;
    }
}

//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a bee class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;

public class Bee : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Bee is one of the player characters in the game. Bee is a sniper class
    /// capable of firing a single stinger projectile. The charge meter
    /// determines the speed at which the stinger is fired.
    /// </summary>-------------------------------------------------------------

    public LineRenderer trajectory;
    public int segmentCount;

    public Transform firePoint;
    public GameObject stingerPrefab;

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
            CalculateTrajectory(currentCharge);
            SetTrajectoryActive(true);
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
            SetTrajectoryActive(false);
            if (currentCharge < 0)
            {
                currentCharge = 0;
            }
        }
        shoot = false;
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new instance of the stinger prefab and fires it.
    /// </summary>-------------------------------------------------------------
    public void Sling()
    {
        GameObject stinger = Instantiate(stingerPrefab, firePoint.position, 
            firePoint.rotation);
        stinger.GetComponent<Projectile>().owner = gameObject;
        stinger.GetComponent<Projectile>().charge = currentCharge;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Enables or disables the trajectory visualizer to see where a shot is 
    /// going to be fired.
    /// </summary>
    /// <param name="active">whether the visualizer is being enabled or 
    /// disabled.</param>
    /// -----------------------------------------------------------------------
    void SetTrajectoryActive(bool active)
    {
        trajectory.enabled = active;
    }

    /// <summary>--------------------------------------------------------------
    /// Calculates the new trajectory based on the position, rotation, and 
    /// current charge of bee. 
    /// Credit for CalculateTrajectory goes to NightShade on youtube: 
    /// https://youtu.be/kRgFiCjdLpY
    /// </summary>
    /// <param name="charge">the current charge of bee.</param>
    /// -----------------------------------------------------------------------
    void CalculateTrajectory(float charge)
    {
        Vector2[] segments = new Vector2[segmentCount];
        segments[0] = firePoint.position;
        float drag = (1.0f - Mathf.Pow(
            stingerPrefab.GetComponent<Rigidbody2D>().drag, 1f/3f)
            * Time.fixedDeltaTime);
        Vector2 segVelocity = new Vector2(joystickDraw.x, joystickDraw.y)
            * charge * -stingerPrefab.GetComponent<Projectile>().speed;
        for (int i = 1; i < segmentCount; i++)
        {
            float timeCurve = (i * Time.fixedDeltaTime * 5.0f);
            segVelocity *= drag;
            segments[i] = segments[0] + segVelocity * timeCurve + 0.5f
                * Physics2D.gravity * Mathf.Pow(timeCurve, 2);
        }
        trajectory.positionCount = segmentCount;
        for (int j = 0; j < segmentCount; j++)
        {
            trajectory.SetPosition(j, segments[j]);
        }
    }
}
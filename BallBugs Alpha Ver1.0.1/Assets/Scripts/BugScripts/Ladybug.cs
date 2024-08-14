//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a ladybug class that is fun to play
//-----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class Ladybug : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Ladybug is one of the player characters in the game. Ladybug is an
    /// all-rounder class that fires multiple spot projectiles in quick
    /// succession. The charge meter determines the number of projectiles
    /// fired per burst, 1 at a minumum and 5 at a maximum.
    /// </summary>-------------------------------------------------------------

    public LineRenderer Trajectory;
    public int segmentCount;

    public Transform firePoint;
    public GameObject spotPrefab;

    public float shotDelay = 0.1f;

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
            StartCoroutine(Delay());
            Release();
        }
        else if (joystickDraw.magnitude != 0f && recharged == true && 
            wrapped == false)
        {
            ChargingUp(true);
            CalculateTrajectory();
            SetTrajectoryActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
            if (recharged == true && shoot == true)
            {
                slingshotMode = false;
                joystickDrawSaveStates[0] = joystickDraw;
                Sling();
                StartCoroutine(Delay());
                Release();
            }
        }
        else
        {
            SetTrajectoryActive(false);
            if (recharged == true)
            {
                currentCharge = 0f;
            }
        }
        shoot = false;
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new instance of the spot prefab and fires it.
    /// </summary>-------------------------------------------------------------
    public void Sling()
    {
        GameObject spot = Instantiate(spotPrefab, firePoint.position,
            firePoint.rotation);
        spot.GetComponent<Projectile>().owner = gameObject;
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Perform a short time dellay between shots, then if enough charge is
    /// left, fire another shot and start the delay again.
    /// </summary>
    /// <returns>coroutine that executes shot delay event.</returns>
    /// -----------------------------------------------------------------------
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(shotDelay);
        if (Mathf.Round(currentCharge * 10.0f) * 0.1f >= 0f)
        {
            Sling();
            StartCoroutine(Delay());
        }
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
        Trajectory.enabled = active;
    }

    /// <summary>--------------------------------------------------------------
    /// Calculates the new trajectory based on the position and rotation of
    /// ladybug. 
    /// Credit for CalculateTrajectory goes to NightShade on youtube: 
    /// https://youtu.be/kRgFiCjdLpY
    /// </summary>-------------------------------------------------------------
    void CalculateTrajectory()
    {
        Vector2[] segments = new Vector2[segmentCount];
        segments[0] = firePoint.position;
        float drag = (1.0f - 0.75f * Time.fixedDeltaTime);
        Vector2 segVelocity = new Vector2(joystickDraw.x, joystickDraw.y) 
            * -spotPrefab.GetComponent<Projectile>().speed;
        for (int i = 1; i < segmentCount; i++)
        {
            float timeCurve = (i * Time.fixedDeltaTime * 5.0f);
            segVelocity *= drag;
            segments[i] = segments[0] + segVelocity * timeCurve + 0.5f 
                * Physics2D.gravity * Mathf.Pow(timeCurve, 2);
        }
        Trajectory.positionCount = segmentCount;
        for (int j = 0; j < segmentCount; j++)
        {
            Trajectory.SetPosition(j, segments[j]);
        }
    }
}
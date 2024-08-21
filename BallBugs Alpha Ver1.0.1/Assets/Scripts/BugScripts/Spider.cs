//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a spider class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;

public class Spider : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Spider is one of the player characters in the game. Spider is a spacer
    /// class that attacks by shooting strands of web that reel opponents in
    /// towards its mandibles. The webs deal little to no damage, but they draw
    /// opponents closer to Spider and immobilize targets for an amount of time
    /// proportional to the charge amount when fired. The charge meter not only
    /// determines stun time, but range as well. If a strand hits terrain, the
    /// spider will grapple onto it and can swing from the web, pull itself
    /// towards the web, and detach when necessary.
    /// </summary>-------------------------------------------------------------

    public LineRenderer trajectory;
    public int segmentCount;

    public Transform firePoint;
    public GameObject webPrefab;

    public float zipSpeed = 0.005f;
    public bool autoZip = false;
    private const float MIN_GRAPPLE_DISTANCE = 0.35f;

    private Vector2 movement;

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
            currentCharge = 0f;
        }
        shoot = false;
        movement = gameObject.GetComponent<PlayerMovement>().movement;
        distance = new Vector2(grapplePos.x - gameObject.transform.position.x,
            grapplePos.y - gameObject.transform.position.y);
        if (((grapple.enabled == true && Vector2.Angle(movement, distance) 
            < 22.5f && movement.magnitude != 0) 
            || autoZip == true) && grapple.distance > MIN_GRAPPLE_DISTANCE)
        {
            grapple.distance -= zipSpeed * Time.deltaTime;
        }
        if (currentWeb == null)
        {
            Ungrapple();
        }
    }

    public void OnJointBreak2D()
    {
        Ungrapple();
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new instance of the web prefab and fires it.
    /// </summary>-------------------------------------------------------------
    public void Sling()
    {
        GameObject web = Instantiate(webPrefab, firePoint.position, 
            firePoint.rotation);
        web.GetComponent<Projectile>().owner = gameObject;
        web.GetComponent<Projectile>().charge = currentCharge;
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
    /// current charge of spider. 
    /// Credit for CalculateTrajectory goes to NightShade on youtube: 
    /// https://youtu.be/kRgFiCjdLpY
    /// </summary>
    /// <param name="charge">the current charge of spider.</param>
    /// -----------------------------------------------------------------------
    void CalculateTrajectory(float charge)
    {
        Vector2[] segments = new Vector2[segmentCount];
        segments[0] = firePoint.position;
        float drag = (1.0f 
            - Mathf.Pow(webPrefab.GetComponent<Rigidbody2D>().drag, 1f / 3f)
            * Time.fixedDeltaTime);
        Vector2 segVelocity = new Vector2(joystickDraw.x, joystickDraw.y) 
            * charge * -webPrefab.GetComponent<Projectile>().speed;
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
//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a bombardier beetle class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;

public class BombardierBeetle : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Bombardier Beetle is one of the player characters in the game.
    /// Bombardier Beetle is a powerhouse class with short range but very high
    /// damage potential. It attacks with a burst of acid similar to a shotgun
    /// blast. The charge meter deterines how narrow the shot spread will be.
    /// </summary>-------------------------------------------------------------

    public float maxSpread = 45f;
    public float minSpread = 22.5f;
    public float knockbackForce = 100f;

    public Transform firePoint;
    public GameObject acidPrefab;

    public LineRenderer trajectoryTop;
    public LineRenderer trajectoryBottom;

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
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates multiple acid spray projectiles at the fire point and evenly 
    /// spaces them out based on the charge amount when fired. Knocks back
    /// Bombardier Beetle with force proportional to charge amount.
    /// </summary>-------------------------------------------------------------
    public void Sling()
    {
        int numProjectiles = 
            acidPrefab.GetComponent<Projectile>().numProjectiles;
        float angle = CalculateAngle(currentCharge) * numProjectiles;
        if (knockbackForce != 0)
        {
            Vector2 distanceVector = new Vector2(rb.position.x - 
                firePoint.position.x, rb.position.y - firePoint.position.y);
            rb.AddForce(distanceVector.normalized * knockbackForce * 
                currentCharge);
        }
        for (int i = 0; i < numProjectiles; i++)
        {
            GameObject acid = Instantiate(acidPrefab, firePoint.position, 
                firePoint.rotation);
            acid.transform.Rotate(0f, 0f, ((numProjectiles / 2) - i) * angle,
                Space.Self);
            acid.GetComponent<Projectile>().owner = gameObject;
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
        trajectoryTop.enabled = active;
        trajectoryBottom.enabled = active;
    }

    /// <summary>--------------------------------------------------------------
    /// Updates the trajectory displays to accurately represent the spread of 
    /// shots to be fired.
    /// </summary>
    /// <param name="charge">Bombardier Beetle's current charge.</param>
    /// -----------------------------------------------------------------------
    void CalculateTrajectory(float charge)
    {
        Vector2 startPoint = firePoint.position;
        Vector2 segVelocity = joystickDraw.normalized
            * -acidPrefab.GetComponent<Projectile>().speed;
        float timeCurve = acidPrefab.GetComponent<Projectile>().lifetime;
        Vector2 endPointTop = startPoint + RotateVector(segVelocity, 
            CalculateAngle(charge)) * timeCurve;
        Vector2 endPointBottom = startPoint + RotateVector(segVelocity, 
            -CalculateAngle(charge)) * timeCurve;
        trajectoryTop.positionCount = 2;
        trajectoryBottom.positionCount = 2;
        trajectoryTop.SetPosition(0, startPoint);
        trajectoryBottom.SetPosition(0, startPoint);
        trajectoryTop.SetPosition(1, endPointTop);
        trajectoryBottom.SetPosition(1, endPointBottom);
    }

    /// <summary>--------------------------------------------------------------
    /// Calculates the minumum/maximum angle from the normal line to fire 
    /// projectiles, A.K.A the shot spread, based on current charge and how far
    /// the joystick is drawn back.
    /// </summary>
    /// <param name="charge">Bombardier Beetle's current charge.</param>
    /// <returns>the deviation angle from normal in radians.</returns>
    /// -----------------------------------------------------------------------
    float CalculateAngle(float charge)
    {
        return ((1 - (charge * joystickDrawSaveStates[1].magnitude)) 
            * maxSpread + minSpread) * (Mathf.PI / 180f);
    }

    /// <summary>--------------------------------------------------------------
    /// Rotates a vector by a given angle while preserving its magnitude.
    /// </summary>
    /// <param name="vector">the vector to be rotated.</param>
    /// <param name="angle">the angle to rotate the vector by in radians.
    /// </param>
    /// <returns>the original vector rotated by the angle.</returns>
    /// -----------------------------------------------------------------------
    Vector2 RotateVector(Vector2 vector, float angle)
    {
        float x = vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle);
        float y = vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle);
        return new Vector2(x, y);
    }
}
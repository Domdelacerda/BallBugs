using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladybug : Bug, ISlingshot
{
    // The segment count for the line renderer component
    public int segmentCount;

    // The point at which the projectile is fired
    public Transform firePoint;
    // The prefab for the projectile to be instantiated
    public GameObject spotPrefab;

    // The delay between firing multiple shots
    public float shotDelay = 0.1f;

    // The trajectory LineRenderer that is used for aiming
    public LineRenderer Trajectory;

    // Update executes every frame
    void Update()
    {
        // Slingshot shooting controls (on joystick release)
        if (joystickDraw.magnitude == 0f && recharged == true && primed == true && slingshotControls == true && wrapped == false)
        {
            slingshotMode = true;
            Sling();
            StartCoroutine(Delay());
            Release();
        }
        // If the joystick is not centered (if it is being pulled back)
        else if (joystickDraw.magnitude != 0f && recharged == true && wrapped == false)
        {
            ChargingUp(true);
            CalculateTrajectory(currentCharge);
            SetTrajectoryActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
            // Manual shooting controls (on button press)
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

    // Delays shots by a specified number of seconds
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(shotDelay);
        if (Mathf.Round(currentCharge * 10.0f) * 0.1f >= 0f)
        {
            Sling();
            StartCoroutine(Delay());
        }
    }

    // Implementation for the sling interface
    public void Sling()
    {
        // New instance of spot prefab is created and fired
        GameObject spot = Instantiate(spotPrefab, firePoint.position, firePoint.rotation);
        spot.GetComponent<Projectile>().owner = gameObject;
    }

    // Credit for SetVisualizerActive goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void SetTrajectoryActive(bool active)
    {
        Trajectory.enabled = active;
    }

    // Credit for ShowTrajectory goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void CalculateTrajectory(float charge)
    {
        Vector2[] segments = new Vector2[segmentCount];
        segments[0] = firePoint.position;
        // For some reason, the drag value in the drag equation is the cube root of the bullet's actual drag
        // 0.421875^(1/3) = 0.75
        // NOTE: Tested this with other values; does not work. Just enter random shit until the projectile follows the trajectory
        float drag = (1.0f - 0.75f * Time.fixedDeltaTime);
        Vector2 segVelocity = new Vector2(joystickDraw.x, joystickDraw.y) * -spotPrefab.GetComponent<Projectile>().speed;
        for (int i = 1; i < segmentCount; i++)
        {
            float timeCurve = (i * Time.fixedDeltaTime * 5.0f);
            segVelocity *= drag;
            segments[i] = segments[0] + segVelocity * timeCurve + 0.5f * Physics2D.gravity * Mathf.Pow(timeCurve, 2);
        }
        Trajectory.positionCount = segmentCount;
        for (int j = 0; j < segmentCount; j++)
        {
            Trajectory.SetPosition(j, segments[j]);
        }
    }
}
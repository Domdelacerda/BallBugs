using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombardierBeetle : Bug, ISlingshot
{
    // The maximum shot spread
    public float maxSpread = 45f;
    // The minimum shot spread
    public float minSpread = 22.5f;

    // The point at which the projectile is fired
    public Transform firePoint;
    // The prefab for the projectile to be instantiated
    public GameObject acidPrefab;

    // The trajectory LineRenderer that is used for aiming
    public LineRenderer trajectoryTop;
    public LineRenderer trajectoryBottom;

    public float knockbackForce = 100f;

    // Update is called once per frame
    void Update()
    {
        // Slingshot shooting controls (on joystick release)
        if (joystickDraw.magnitude == 0f && recharged == true && primed == true && slingshotControls == true && wrapped == false)
        {
            slingshotMode = true;
            Sling();
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

    // Implementation for the sling interface
    public void Sling()
    {
        int numProjectiles = acidPrefab.GetComponent<Projectile>().numProjectiles;
        float angle = CalculateAngle(currentCharge) * numProjectiles;
        if (knockbackForce != 0)
        {
            Vector2 distanceVector = new Vector2(rb.position.x - firePoint.position.x, rb.position.y - firePoint.position.y);
            rb.AddForce(distanceVector.normalized * knockbackForce * currentCharge);
        }
        for (int i = 0; i < numProjectiles; i++)
        {
            GameObject acid = Instantiate(acidPrefab, firePoint.position, firePoint.rotation);
            acid.transform.Rotate(0f, 0f, ((numProjectiles / 2) - i) * angle, Space.Self);
            acid.GetComponent<Projectile>().owner = gameObject;
        }
    }

    // Credit for SetVisualizerActive goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void SetTrajectoryActive(bool active)
    {
        trajectoryTop.enabled = active;
        trajectoryBottom.enabled = active;
    }

    // Get the current rotation range from the current charge
    float CalculateAngle(float charge)
    {
        return ((1 - (charge * joystickDrawSaveStates[1].magnitude)) * maxSpread + minSpread) * (Mathf.PI / 180f);
    }

    // Rotate a supplied vector by a given angle
    Vector2 RotateVector(Vector2 vector, float angle)
    {
        float x = vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle);
        float y = vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle);
        return new Vector2(x, y);
    }

    // Credit for ShowTrajectory goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void CalculateTrajectory(float charge)
    {
        Vector2 startPoint = firePoint.position;
        Vector2 segVelocity = joystickDraw.normalized * -acidPrefab.GetComponent<Projectile>().speed;
        float timeCurve = acidPrefab.GetComponent<Projectile>().lifetime;
        Vector2 endPointTop = startPoint + RotateVector(segVelocity, CalculateAngle(charge)) * timeCurve;
        Vector2 endPointBottom = startPoint + RotateVector(segVelocity, -CalculateAngle(charge)) * timeCurve;
        trajectoryTop.positionCount = 2;
        trajectoryBottom.positionCount = 2;
        trajectoryTop.SetPosition(0, startPoint);
        trajectoryBottom.SetPosition(0, startPoint);
        trajectoryTop.SetPosition(1, endPointTop);
        trajectoryBottom.SetPosition(1, endPointBottom);
    }
}

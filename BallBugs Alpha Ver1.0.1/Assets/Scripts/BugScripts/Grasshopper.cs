using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grasshopper : Bug, ISlingshot
{
    // The point at which the projectile is fired
    public Transform firePoint;
    // The prefab for the projectile to be instantiated
    public GameObject kickPrefab;
    // The projectile visualizer used for showing the attack's range
    public GameObject visualizer;

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
            SetVisualizerActive(true);
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
            SetVisualizerActive(false);
            currentCharge = 0f;
        }
        shoot = false;
    }

    // Implementation for the sling interface
    public void Sling()
    {
        GameObject kick = Instantiate(kickPrefab, firePoint.position, firePoint.rotation);
        kick.GetComponent<Rigidbody2D>().mass *= currentCharge;
        kick.GetComponent<Projectile>().owner = gameObject;
        kick.GetComponent<GrasshopperKick>().charge = currentCharge;
    }

    public void Launch(float power)
    {
        Vector2 direction = new Vector2(gameObject.transform.position.x - firePoint.position.x,
            gameObject.transform.position.y - firePoint.position.y);
        rb.velocity = direction.normalized * power;
    }

    // Credit for SetVisualizerActive goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void SetVisualizerActive(bool active)
    {
        visualizer.GetComponent<Renderer>().enabled = active;
    }
}

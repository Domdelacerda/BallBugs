using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snail : Bug, ISlingshot
{
    // The point at which the projectile is fired
    public Transform firePoint;
    // The prefab for the projectile to be instantiated
    public GameObject slimeballPrefab;

    // The visualizer game object that is used to display projectile size
    public GameObject Visualizer;

    public bool withdrawal = false;

    // Update executes every frame
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
            CalculateVisualizer(currentCharge);
            SetVisualizerActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
            // Manual shooting controls (on button press)
            if (recharged == true && shoot == true)
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
        // New instance of slimeball prefab is created and fired
        GameObject slimeball = Instantiate(slimeballPrefab, firePoint.position, firePoint.rotation);
        slimeball.transform.localScale = slimeball.transform.localScale * (1f + currentCharge);
        slimeball.GetComponent<Projectile>().owner = gameObject;
        if (withdrawal == true)
        {
            gameObject.layer = SHIELD_LAYER;
        }
    }

    public override IEnumerator Recharge()
    {
        yield return new WaitForSeconds(cooldownTime);
        recharged = true;
        if (bugAnimator != null)
        {
            bugAnimator.SetBool("IsRecharged", true);
        }
        comboCounter = 0;
        if (withdrawal == true)
        {
            gameObject.layer = defaultLayer;
        }
    }

    // Credit for SetVisualizerActive goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void SetVisualizerActive(bool active)
    {
        Visualizer.GetComponent<Renderer>().enabled = active;
    }

    // Credit for CalculateVisualizer goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void CalculateVisualizer(float charge)
    {
        Visualizer.transform.localScale = Visualizer.transform.localScale.normalized * (1f + currentCharge) * 4.5f;
    }
}
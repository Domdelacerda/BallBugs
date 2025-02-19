//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a firefly class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

public class Firefly : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Firefly is one of the player characters in the game. Firefly is a
    /// powerhouse class that shoots high damage, explosive fireballs. The
    /// charge meter determines the size of the fireballs and their subsequent
    /// explosions.
    /// </summary>-------------------------------------------------------------

    public Transform firePoint;
    public GameObject fireballPrefab;
    public GameObject visualizer;

    private const float VISUALIZER_SIZE = 4.5f;

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
            CalculateVisualizer(currentCharge * joystickDraw.magnitude);
            SetVisualizerActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
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

    //-------------------------------------------------------------------------
    // INPUT ACTIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Allow the player to crouch when the crouch button is pressed as long as
    /// they aren't currently immobilized.
    /// </summary>
    /// <param name="ctx">the action input that determines whether the player
    /// used the crouch input or not.</param>
    /// -----------------------------------------------------------------------
    public override void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && wrapped == false)
        {
            Ungrapple();
            Detonate();
        }
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new instance of the fireball prefab, scales it depending
    /// the current charge, and fires it.
    /// </summary>-------------------------------------------------------------
    public virtual void Sling()
    {
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, 
            firePoint.rotation);
        fireball.transform.localScale *= 1f + currentCharge 
            * joystickDrawSaveStates[2].magnitude;
        Projectile script = fireball.GetComponent<Projectile>();
        script.owner = gameObject;
        script.charge = currentCharge * joystickDrawSaveStates[2].magnitude;
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
    void SetVisualizerActive(bool active)
    {
        visualizer.GetComponent<Renderer>().enabled = active;
    }

    /// <summary>--------------------------------------------------------------
    /// Calculates the new visualizer size based on the current charge of 
    /// firefly. 
    /// </summary>
    /// <param name="charge">the current charge of firefly.</param>
    /// -----------------------------------------------------------------------
    void CalculateVisualizer(float charge)
    {
        visualizer.transform.localScale = 
            visualizer.transform.localScale.normalized 
            * (1f + charge) * VISUALIZER_SIZE;
    }

    /// <summary>--------------------------------------------------------------
    /// Detonate all fireballs that are currently fired by this firefly.
    /// </summary>-------------------------------------------------------------
    void Detonate()
    {
        Fireball[] fireballs = FindObjectsOfType<Fireball>();
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (fireballs[i].owner == gameObject)
            {
                fireballs[i].Explode();
                Destroy(fireballs[i].gameObject);
            }
        }
    }
}
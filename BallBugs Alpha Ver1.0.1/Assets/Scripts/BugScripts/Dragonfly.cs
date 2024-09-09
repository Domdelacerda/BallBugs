//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a dragonfly class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Dragonfly : Bug
{
    /// <summary>--------------------------------------------------------------
    /// Dragonfly is one of the player characters in the game. Dragonfly is a 
    /// melee class that attacks by rotating its wings like a saw and slicing
    /// opponents that get within range. This technique also allows Dragonfly
    /// to fly for a brief period of time while it still has charge. Dragonfly
    /// reloads with full charge which depletes as it uses its flight ability.
    /// The charge meter determines how fast the wings spin, dealing more
    /// damage and flying faster while fully charged.
    /// </summary>-------------------------------------------------------------

    private bool flying = false;
    public float flightSpeed = 2f;
    public float rotationSpeed = 30f;
    private float moveSpeed = 0f;
    public float gravityReductionScale = 0.5f;

    private float initialDrag;
    public float crouchDrag = 1f;

    public Collider2D wings;
    public PlayerMovement movement;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        defaultLayer = gameObject.layer;
        DisplayHealthBar(maxHealth, health);
        rechargeRoutine = StartCoroutine(Recharge());
        moveSpeed = movement.moveSpeed;
        initialDrag = rb.drag;
    }

    private void Update()
    {
        if (joystickDraw.magnitude != 0f && recharged == true
            && currentCharge > 0 && wrapped == false)
        {
            Fly();
        }
        else if (currentCharge <= 0 && flying == true)
        {
            Fall();
            Release();
        }
        else if (joystickDraw.magnitude == 0f && flying == true)
        {
            Fall();
        }
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
            if (flying == true)
            {
                rb.drag = crouchDrag;
            }
        }
        else if (!ctx.performed || flying == false)
        {
            rb.drag = initialDrag;
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Recharge the bug's attack, allowing them to attack again after a
    /// specified cooldown time has passed.
    /// </summary>
    /// <returns>coroutine that executes recharge event.</returns>
    /// -----------------------------------------------------------------------
    public override IEnumerator Recharge()
    {
        yield return new WaitForSeconds(cooldownTime);
        recharged = true;
        if (bugAnimator != null)
        {
            bugAnimator.SetBool("IsRecharged", true);
        }
        currentCharge = 1f;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Enables flight if dragonfly is not already flying, moving them in the
    /// direction of the shooting joystick.
    /// </summary>-------------------------------------------------------------
    public void Fly()
    {
        if (flying == false)
        {
            wings.enabled = true;
            rb.gravityScale *= gravityReductionScale;
            movement.moveSpeed = 0;
            flying = true;
        }
        rb.AddForce(Vector2.ClampMagnitude(joystickDraw, 1f) * flightSpeed);
        currentCharge -= chargeRate * Time.fixedDeltaTime;
        wings.transform.Rotate(new Vector3(0, 0, currentCharge 
            * rotationSpeed));
    }

    /// <summary>--------------------------------------------------------------
    /// Disables flight if dragonfly is flying, resetting their gravity and
    /// regular movement speed back to normal.
    /// </summary>-------------------------------------------------------------
    public void Fall()
    {
        wings.enabled = false;
        rb.gravityScale /= gravityReductionScale;
        flying = false;
        movement.moveSpeed = moveSpeed;
    }
}
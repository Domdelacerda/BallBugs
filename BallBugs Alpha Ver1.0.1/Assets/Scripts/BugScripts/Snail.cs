//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a snail class that is fun to play
//-----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class Snail : Firefly
{
    /// <summary>--------------------------------------------------------------
    /// Snail is one of the player characters in the game. Snail is a tank 
    /// class that attacks by firing slimeballs at opponents. The slimeballs
    /// turn into sticky puddles on contact with terrain. The charge meter 
    /// determines the size of the slimeballs and their subsequent slime 
    /// puddles.
    /// </summary>-------------------------------------------------------------

    public bool withdrawal = false;

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new instance of the slimeball prefab, scales it depending
    /// the current charge, and fires it.
    /// </summary>-------------------------------------------------------------
    public override void Sling()
    {
        GameObject slimeball = Instantiate(fireballPrefab, firePoint.position,
            firePoint.rotation);
        slimeball.transform.localScale *= 1f + currentCharge 
            * joystickDrawSaveStates[2].magnitude;
        slimeball.GetComponent<Projectile>().owner = gameObject;
        if (withdrawal == true)
        {
            gameObject.layer = SHIELD_LAYER;
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Recharge snail's attack, allowing them to attack again after a
    /// specified cooldown time has passed. If the withdrawal attribute is
    /// enabled, disable their shield after they have finished reloading.
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
        comboCounter = 0;
        if (withdrawal == true)
        {
            gameObject.layer = defaultLayer;
        }
    }
}
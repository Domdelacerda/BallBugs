//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a mosquito class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;

public class Mosquito : Bee, IHealable
{
    /// <summary>--------------------------------------------------------------
    /// Mosquito is one of the player characters in the game. Mosquito is a
    /// sniper class that fires proboscis darts that heal Mosquito based on how
    /// much damage they deal. The charge meter determines the dart's velocity 
    /// and the percentage of health regained from damage dealt. If Mosquito
    /// gains health while at full health, their max health will increase.
    /// </summary>-------------------------------------------------------------

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Adds health to this mosquito based on the specified heal value. A heal
    /// display is created to let the player know how much health was regained.
    /// If the mosquito gains more health than its max health, increase its max
    /// health by the overflow amount to account for it.
    /// </summary>
    /// <param name="healAmount">the amount of healing applied to this 
    /// mosquito.</param>
    /// <returns>the amount of actual health regained after the calculation
    /// is made.</returns>
    /// -----------------------------------------------------------------------
    public override int Heal(int healAmount)
    {
        if (healAmount + health > maxHealth)
        {
            float scaleMultiplier = healAmount + health;
            scaleMultiplier = Mathf.Pow(scaleMultiplier / maxHealth, 0.5f);
            gameObject.transform.localScale *= scaleMultiplier;
            rb.mass *= scaleMultiplier;
            gameObject.GetComponent<PlayerMovement>().moveSpeed *= scaleMultiplier;
            maxHealth = healAmount + health;
        }
        health += healAmount;
        if (healAmount != 0)
        {
            DisplayHealthBar(maxHealth, health);
            CreateDisplay(healDisplay, healAmount);
        }
        return healAmount;
    }
}
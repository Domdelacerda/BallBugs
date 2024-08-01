using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mosquito : Bee, IHealable
{
    // Implmentation for heal interface
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
            // Update the health bar when healed
            DisplayHealthBar(maxHealth, health);
            // Create the heal display when hit
            CreateDisplay(healDisplay, healAmount);
        }
        return healAmount;
    }
}

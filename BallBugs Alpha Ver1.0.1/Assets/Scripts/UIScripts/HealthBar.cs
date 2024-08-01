using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // The image that will be expanded or contracted to display user's health
    public Image healthSprite;
    // The image that will be expanded or contracted to display user's health
    public Image chargeSprite;

    // The rotation on start
    private Quaternion startRotation;

    public void Start()
    {
        startRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void UpdateHealthBar(int maxHealth, int currentHealth)
    {
        healthSprite.fillAmount = Mathf.Round(currentHealth) / Mathf.Round(maxHealth);
    }

    public void UpdateChargeBar(float currentCharge)
    {
        chargeSprite.fillAmount = currentCharge;
    }

    public void Update()
    {
        transform.rotation = startRotation;
    }
}

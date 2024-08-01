using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DamageDisplay : MonoBehaviour
{
    // The text used to display the amount of damage dealt
    public TextMeshPro damageText;
    // The amount of the damage to be displayed
    public int damage = 0;
    // The time that the damage display appears on screen before disappearing
    protected const float damageDisplayTime = 0.75f;
    // The bug who owns the damage display A.K.A the bug taking damage
    public GameObject owner;
    // The collider of the damage display
    public BoxCollider2D damageCollider;
    // The damage display prefab used when combining two damage displays
    public GameObject damageDisplay;
    // A string variable that is used to determine the type of the display
    public string type;

    // The uiLayer int represents the layer that all damage displays exist on
    private const int uiLayer = 5;
    // The layer mask to check for collisions
    private LayerMask uiMask;

    void Start()
    {
        damageText.text = damage.ToString();
        uiMask = LayerMask.GetMask("UI");
        // Delete the damage display after the specified time
        Destroy(gameObject, damageDisplayTime);
    }

    void Update()
    {
        OverlapCheck();
    }

    public void OverlapCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(damageCollider.bounds.center, damageCollider.bounds.size, 0f, uiMask);
        if (colliders.Length > 1)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                // Make sure the collider isn't null
                if (colliders[i] != null)
                {
                    // Ignore this damage display in future comparisons
                    if (colliders[i].GetInstanceID() != damageCollider.GetInstanceID())
                    {
                        // Damage displays will only combine if they belong to the same bug
                        if (owner.gameObject.GetInstanceID() == colliders[i].gameObject.GetComponent<DamageDisplay>().owner.gameObject.GetInstanceID())
                        {
                            // Destroy the shield display if it overlaps with a damage display
                            if (type == "shield" && colliders[i].gameObject.GetComponent<DamageDisplay>().type == "damage")
                            {
                                Destroy(gameObject);
                            }
                            else if (type == "damage" && colliders[i].gameObject.GetComponent<DamageDisplay>().type == "shield")
                            {
                                Destroy(colliders[i].gameObject);
                            }
                            // Only combine displays if they are the same type (so damage and heal won't combine)
                            else if (type == colliders[i].gameObject.GetComponent<DamageDisplay>().type)
                            {
                                // Create a new damage display that combines the values of the first two
                                GameObject combinedDisplay = Instantiate(damageDisplay, gameObject.transform.position, Quaternion.identity);
                                combinedDisplay.GetComponent<DamageDisplay>().damage = damage + colliders[i].gameObject.GetComponent<DamageDisplay>().damage;
                                combinedDisplay.GetComponent<DamageDisplay>().owner = owner;
                                // Change the color of the sprite based on the combo level
                                Color oldColor = gameObject.GetComponent<Renderer>().material.color;
                                if (oldColor.r <= 0.75f)
                                {
                                    combinedDisplay.GetComponent<Renderer>().material.SetColor("_Color", oldColor);
                                }
                                else
                                {
                                    Color newColor = new Color(oldColor.r - 0.075f, oldColor.g - 0.075f, oldColor.b - 0.075f, 1f);
                                    combinedDisplay.GetComponent<Renderer>().material.SetColor("_Color", newColor);
                                    combinedDisplay.GetComponent<DamageDisplay>().type = type;
                                }
                                // Destroy both of the original damage displays
                                Destroy(gameObject);
                                Destroy(colliders[i].gameObject);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public PhysicsMaterial2D defaultMaterial;
    public PhysicsMaterial2D stickyMaterial;
    public float moveSpeedReduction = 0.5f;
    public float gravityScaleReduction = 0.5f;

    public void OnTriggerEnter2D(Collider2D other)
    {
        other.sharedMaterial = stickyMaterial;
        if (other.gameObject.tag == "Bug")
        {
            other.gameObject.GetComponent<PlayerMovement>().moveSpeed *= moveSpeedReduction;
            other.gameObject.GetComponent<Rigidbody2D>().gravityScale *= gravityScaleReduction;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        other.sharedMaterial = defaultMaterial;
        if (other.gameObject.tag == "Bug")
        {
            other.gameObject.GetComponent<PlayerMovement>().moveSpeed /= moveSpeedReduction;
            other.gameObject.GetComponent<Rigidbody2D>().gravityScale /= gravityScaleReduction;
        }
    }
}

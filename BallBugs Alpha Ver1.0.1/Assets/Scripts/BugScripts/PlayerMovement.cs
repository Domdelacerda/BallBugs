using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 1f;

    public Rigidbody2D rb;

    public Vector2 movement;

    // Import Game Object that contains game over functionality
    public GameObject gameEnder;

    void FixedUpdate()
    {
        rb.AddForce(Vector2.ClampMagnitude(movement, 1f) * moveSpeed);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (gameObject.GetComponent<Bug>().wrapped == false)
        {
            movement = ctx.ReadValue<Vector2>();
        }
    }

    void OnDestroy()
    {
        if (gameEnder != null)
        {
            gameEnder.GetComponent<GameOver>().GameOverSequence();
        }
    }
}

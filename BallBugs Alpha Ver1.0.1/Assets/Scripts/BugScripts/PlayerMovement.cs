//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have player movement that is fluid
//-----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Player movement is applied to all player characters in the game, and
    /// gives them the ability to move in every direction.
    /// </summary>-------------------------------------------------------------
    
    public Vector2 movement;
    public float moveSpeed = 1f;

    public Rigidbody2D rb;

    public GameObject gameEnder;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void FixedUpdate()
    {
        rb.AddForce(Vector2.ClampMagnitude(movement, 1f) * moveSpeed);
    }

    void OnDestroy()
    {
        if (gameEnder != null)
        {
            gameEnder.GetComponent<GameOver>().GameOverSequence();
        }
    }

    //-------------------------------------------------------------------------
    // INPUT ACTIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Set the player's movement direction to the same direction that the
    /// movement joystick is drawn back every frame, unless the player is 
    /// currently immobilized.
    /// </summary>
    /// <param name="ctx">the action input that determines the direction the
    /// joystick is pulled in.</param>
    /// -----------------------------------------------------------------------
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (gameObject.GetComponent<Bug>().wrapped == false)
        {
            movement = ctx.ReadValue<Vector2>();
        }
    }
}
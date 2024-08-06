//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a consistent physics system applied to the game scene
//-----------------------------------------------------------------------------

using UnityEngine;

public class Physics : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Physics determines which layers are able to collide with each other in
    /// the games scenes it is placed in. It also controls the flow of time,
    /// allowing the game's update rate to be slowed down, stopped completely,
    /// or resumed at any time.
    /// </summary>-------------------------------------------------------------
    
    private const int PROJECTILE_LAYER = 7;
    private const int HAZARD_LAYER = 8;
    private const float DEFAULT_DELTA_TIME = 0.02f;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = DEFAULT_DELTA_TIME;
        Physics2D.IgnoreLayerCollision(PROJECTILE_LAYER, PROJECTILE_LAYER);
        Physics2D.IgnoreLayerCollision(PROJECTILE_LAYER, HAZARD_LAYER);
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Pauses all physics and gameplay input so that the player can stop
    /// playing the game if need be
    /// </summary>-------------------------------------------------------------
    public void Pause()
    {
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
    }

    /// <summary>--------------------------------------------------------------
    /// Resumes all physics and gameplay input so that the player can continue
    /// playing the game after pausing
    /// </summary>-------------------------------------------------------------
    public void Play()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = DEFAULT_DELTA_TIME;
    }
}

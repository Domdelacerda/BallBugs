using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physics : MonoBehaviour
{
    // This is the distance that the camera should move to switch between levels
    private const int DISTANCE_BETWEEN_LEVELS = 24;
    // This is the distance that the camera starts at by default
    private const int STARTING_POINT = 12;
    // The number of levels currently in the game
    private const int NUM_LEVELS = 4;

    // The default value for delta time - should be 0.02
    // Messing around with this makes the framerate smoother but also ruins a
    // lot of physics calculations - Use with caution
    private const float defaultDeltaTime = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        // Reset the time and delta time
        Time.timeScale = 1;
        Time.fixedDeltaTime = defaultDeltaTime;
        // The line below disables collision between projectiles and other projectiles
        Physics2D.IgnoreLayerCollision(7, 7);
        // The line below disables collision between projectiles and hazards
        Physics2D.IgnoreLayerCollision(7, 8);
        // The line below disables collision between projectiles and players
        // Physics2D.IgnoreLayerCollision(7, 9);
    }

    // Pause time
    public void Pause()
    {
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
    }

    // Resume time
    public void Play()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = defaultDeltaTime;
    }
}

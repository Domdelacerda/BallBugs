//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have the game end when needed
//-----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Game Over disables gameplay components and enables the game over
    /// displays to clearly indicate that the player should leave the game.
    /// </summary>-------------------------------------------------------------

    public GameObject pauseButton;
    public GameObject mainMenuButton;

    public GameObject shootJoystick;
    public GameObject moveJoystick;

    public Image gameOverDisplay;
    public Image gameOverOverlay;

    private const float END_TIME_SCALE = 0.25f;
    private const float END_DELTA_TIME_SCALE = 0.005f;

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Deactivates movement and shooting joysticks while activating the main
    /// menu button and game over display and overlay.
    /// </summary>-------------------------------------------------------------
    public void GameOverSequence()
    {
        pauseButton.SetActive(false);
        shootJoystick.SetActive(false);
        moveJoystick.SetActive(false);
        gameOverDisplay.enabled = true;
        gameOverOverlay.enabled = true;
        mainMenuButton.GetComponent<Image>().enabled = true;
        mainMenuButton.GetComponent<Button>().enabled = true;
        Time.timeScale = END_TIME_SCALE;
        Time.fixedDeltaTime = END_DELTA_TIME_SCALE;
    }
}
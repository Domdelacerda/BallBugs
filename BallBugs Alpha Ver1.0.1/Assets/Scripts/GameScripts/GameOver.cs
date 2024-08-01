using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    // The pause button which will be disabled when the timer runs out
    public GameObject pauseButton;

    // The main menu button which will be enabled when the timer runs out
    public GameObject mainMenuButton;

    // The joysticks which will be disabled when the timer runs out
    public GameObject shootJoystick;
    public GameObject moveJoystick;

    // The Game Object that the game over sprite will be displayed on
    public GameObject gameOverDisplay;

    // The background overlay for when the game ends
    public GameObject gameOverOverlay;

    public void GameOverSequence()
    {
        pauseButton.SetActive(false);
        shootJoystick.SetActive(false);
        moveJoystick.SetActive(false);
        gameOverDisplay.GetComponent<Image>().enabled = true;
        mainMenuButton.GetComponent<Image>().enabled = true;
        mainMenuButton.GetComponent<Button>().enabled = true;
        gameOverOverlay.GetComponent<Image>().enabled = true;
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = 0.005f;
    }
}

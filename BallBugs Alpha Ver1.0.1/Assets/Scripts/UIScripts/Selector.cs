using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class Selector : MonoBehaviour
{
    // The sprite renderer for the character selector
    public Image characterSelectDisplay;
    // The game object for the ready status bar
    public GameObject readyBar;
    public GameObject CPUText;
    // The game objects for the selector arrows
    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject upButton;
    // The sprite for when the play is readied up
    public Sprite readySprite;
    // The sprite for when the play is not ready yet
    public Sprite notReadySprite;
    // A 2D array of character codes used for switching between characters in an
    // ordered sequence
    public string[,] characterCodes;
    // An int for storing the horizontal position of the character selction in the
    // characterCodes array, which corresponds to the type of bug selected
    public int bugSelection = 0;
    // An int for storing the vertical position of the character selction in the
    // characterCodes array, which correspond to the skin selected
    public int skinSelection = 0;
    // A vector for reading in values from the menu control scheme
    public Vector2 navigation;
    // A bool used for determining if the menu navigation cooldown is on or not
    public bool navCooldown = true;
    // The time it takes for navigation between characters to take place
    public float navCooldownTime = 0.2f;
    // Whether the player selector can be visible at this time or not
    public bool visible = false;
    // Whether the player is ready or not
    public bool ready = false;

    // The player input manager to pull data from
    public PlayerInputManager inputManager;
    // The transform component of the player selector object
    public RectTransform playerTransform;
    // The UI input module to be assigned at runtime
    public InputSystemUIInputModule inputModule;
    // The player input component of the player selector
    public PlayerInput playerInput;

    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        navigation = ctx.ReadValue<Vector2>();
    }

    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameObject[] backButtons = GameObject.FindGameObjectsWithTag("Back");
            for (int i = 0; i < backButtons.Length; i++)
            {
                if (backButtons[i].GetComponent<Button>().enabled == true)
                {
                    backButtons[i].GetComponent<BackButton>().InvokeButton();
                    break;
                }
            }
            ready = false;
        }
    }

    public void OnSelect(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && navCooldown == false)
        {
            SetReady();
        }
    }

    public string GetCurrentSelection()
    {
        return characterCodes[bugSelection, skinSelection];
    }

    public IEnumerator CharacterSwapCooldown()
    {
        yield return new WaitForSeconds(navCooldownTime);
        navCooldown = false;
    }

    public void SetVisible()
    {
        characterSelectDisplay.enabled = visible;
        readyBar.GetComponent<Image>().enabled = visible;
        readyBar.GetComponent<Button>().enabled = visible;
        leftButton.GetComponent<Image>().enabled = visible;
        leftButton.GetComponent<Button>().enabled = visible;
        rightButton.GetComponent<Image>().enabled = visible;
        rightButton.GetComponent<Button>().enabled = visible;
        upButton.GetComponent<Image>().enabled = visible;
        upButton.GetComponent<Button>().enabled = visible;
        if (CPUText != null)
        {
            CPUText.GetComponent<Image>().enabled = visible;
        }
    }

    public void SetReady()
    {
        if (ready == false)
        {
            ready = true;
            readyBar.GetComponent<Image>().sprite = readySprite;
        }
        else
        {
            ready = false;
            readyBar.GetComponent<Image>().sprite = notReadySprite;
        }
    }

    public int Navigate(int direction, int selection, int max)
    {
        if (navCooldown == false && ready == false)
        {
            selection += direction;
            if (selection >= max)
            {
                selection = 0;
            }
            else if (selection < 0)
            {
                selection = max - 1;
            }
        }
        return selection;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerSelector : Selector
{
    // The amount of characters currently in the game
    private const int NUMBER_OF_BUGS = 10;
    // The amount of skins for each character
    private const int NUMBER_OF_SKINS = 3;
    // The file location of the folder to search for character profile sprites
    private const string CHAR_PROFILES_LOCATION = "Sprites/ButtonSprites/";
    private const string ASSET_NAME = "Profile";

    // Start is called before the first frame update
    void Start()
    {
        SharedData.maxPlayers += 1;
        characterCodes = new string[NUMBER_OF_BUGS,NUMBER_OF_SKINS];
        // Assign all existing character codes
        // Bee codes
        characterCodes[0,0] = "bee";
        characterCodes[0,1] = "Locked";
        characterCodes[0,2] = "Locked";
        // Ladybug codes
        characterCodes[1,0] = "ladybug";
        characterCodes[1,1] = "ladybeetle";
        characterCodes[1,2] = "ladybird";
        // Firefly codes
        characterCodes[2,0] = "firefly";
        characterCodes[2,1] = "lightningbug";
        characterCodes[2,2] = "glowworm";
        // Snail codes
        characterCodes[3,0] = "greensnail";
        characterCodes[3,1] = "Locked";
        characterCodes[3,2] = "milksnail";
        // Rolypoly codes
        characterCodes[4,0] = "rolypoly";
        characterCodes[4,1] = "Locked";
        characterCodes[4,2] = "isopod";
        // Spider codes
        characterCodes[5, 0] = "spider";
        characterCodes[5, 1] = "Locked";
        characterCodes[5, 2] = "orbweaver";
        // Bombardier codes
        characterCodes[6, 0] = "bombardierbeetle";
        characterCodes[6, 1] = "Locked";
        characterCodes[6, 2] = "Locked";
        // Mosquito codes
        characterCodes[7, 0] = "mosquito";
        characterCodes[7, 1] = "Locked";
        characterCodes[7, 2] = "Locked";
        // Grasshopper codes
        characterCodes[8, 0] = "grasshopper";
        characterCodes[8, 1] = "cricket";
        characterCodes[8, 2] = "locust";
        // Dragonfly codes
        characterCodes[9, 0] = "dragonfly";
        characterCodes[9, 1] = "Locked";
        characterCodes[9, 2] = "Locked";

        // Locate player input manager in scene
        inputManager = (PlayerInputManager)FindObjectOfType(typeof(PlayerInputManager));
        // Change position on start according to number of players
        playerTransform.anchoredPosition = new Vector2((220 * (SharedData.maxPlayers + SharedData.maxBots)) - 550, playerTransform.anchoredPosition.y);
        playerTransform.SetParent(FindObjectOfType<Canvas>().transform);
        //playerTransform.localScale = FindObjectOfType<CanvasScaler>().
        StartCoroutine(CharacterSwapCooldown());
        SetVisible();
    }

    public void NavigateBug(int direction)
    {
        if (navCooldown == false && ready == false)
        {
            bugSelection += direction;
            if (bugSelection >= NUMBER_OF_BUGS)
            {
                bugSelection = 0;
            }
            else if (bugSelection < 0)
            {
                bugSelection = NUMBER_OF_BUGS - 1;
            }
        }
    }

    public void NavigateSkin(int direction)
    {
        if (navCooldown == false && ready == false)
        {
            skinSelection += direction;
            if (skinSelection >= NUMBER_OF_SKINS)
            {
                skinSelection = 0;
            }
            else if (skinSelection < 0)
            {
                skinSelection = NUMBER_OF_SKINS - 1;
            }
        }
    }

    void Update()
    {
        // If the menu is not on cooldown
        if (navigation.magnitude != 0 && navCooldown == false && ready == false)
        {
            bugSelection = Navigate((int)navigation.x, bugSelection, NUMBER_OF_BUGS);
            skinSelection = Navigate((int)navigation.y, skinSelection, NUMBER_OF_SKINS);
            navCooldown = true;
            StartCoroutine(CharacterSwapCooldown());
        }
        // Locate and display the profile sprite that corresponds to the bug selected in the array
        characterSelectDisplay.sprite = Resources.Load<Sprite>(CHAR_PROFILES_LOCATION + GetCurrentSelection() + ASSET_NAME);
    }
}
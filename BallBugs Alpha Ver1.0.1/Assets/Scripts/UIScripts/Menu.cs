using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using Cinemachine;

public class Menu : MonoBehaviour
{
    // The list of player object currently in the game
    private GameObject[] players = new GameObject[0];
    // A bool that determines when players should be visible or not
    private bool playersVisible = false;

    // Change the character by button press
    public void SetCharacter(string newCharacter)
    {
        SharedData.characterCode0 = newCharacter;
    }

    public void SetCharacters()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            SharedData.characterCodes[i] = players[i].GetComponentInChildren<Selector>().GetCurrentSelection();
            if (players[i].GetComponentInChildren<PlayerInput>() != null)
            {
                SharedData.devices[i] = players[i].GetComponentInChildren<PlayerInput>().devices[0];
            }
            if (players[i].GetComponentInChildren<PlayerInput>() != null)
            {
                SharedData.users[i] = players[i].GetComponentInChildren<PlayerInput>().user;
            }
        }
    }

    // Change the map by button press
    public void SetMap(int newMap)
    {
        SharedData.mapCode = newMap;
    }

    // Change the game mode by button press
    public void SetGameMode(string newGameMode)
    {
        SharedData.gameModeCode = newGameMode;
    }

    // Load the scene depending on the gamemode selected
    public void PlaySingleplayer()
    {
        SceneManager.LoadScene(SharedData.gameModeCode);
    }

    // Load the scene depending on the gamemode selected
    public void PlayMultiplayer()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1 && PlayerReadyCheck())
        {
            SetCharacters();
            SceneManager.LoadScene(SharedData.gameModeCode);
        }
    }

    // Load the main menu again
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        SharedData.maxPlayers = 0;
        SharedData.maxBots = 0;
        SharedData.mapCode = 0;
        Array.Clear(SharedData.characterCodes, 0, SharedData.characterCodes.Length);
        Array.Clear(SharedData.scores, 0, SharedData.scores.Length);
    }

    // Delete all joined players when backing out of multiplayer
    public void DeletePlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            Destroy(players[i]);
            SharedData.maxPlayers = 0;
            SharedData.maxBots = 0;
        }
    }

    // Hide all joined players when navigating the rest of the multiplayer menu
    public void HidePlayers()
    {
        if (playersVisible == false)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponentInChildren<PlayerSelector>() != null)
                {
                    players[i].GetComponentInChildren<Selector>().visible = false;
                    players[i].GetComponentInChildren<Selector>().SetVisible();
                }
                else
                {
                    Destroy(players[i]);
                    SharedData.maxPlayers -= 1;
                }
            }
        }
    }

    // Show all players once local multiplayer is selected
    public void ShowPlayers()
    {
        if (playersVisible == true)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponentInChildren<Selector>().visible = true;
                players[i].GetComponentInChildren<Selector>().SetVisible();
            }
        }
    }

    // Determines when players should be visible or not
    public void SetPlayersVisible(bool visible)
    {
        playersVisible = visible;
        EnablePlayerVisibility();
    }

    // Enables or disables capability for players to be visible
    public void EnablePlayerVisibility()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponentInChildren<Selector>().visible = playersVisible;
        }
    }

    // Checks to see if the game is ready to start or not
    public bool PlayerReadyCheck()
    {
        bool ready = true;
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponentInChildren<Selector>().ready == false)
            {
                ready = false;
                break;
            }
        }
        return ready;
    }

    public void Update()
    {
        PlayMultiplayer();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("local battle")
            && (SharedData.currentPlayers + SharedData.currentBots <= 1 || SharedData.currentPlayers == 0))
        {
            Bug[] winners = FindObjectsOfType<Bug>();
            for (int i = 0; i < SharedData.players.Length; i++)
            {
                for (int j = 0; j < winners.Length; j++)
                {
                    if (SharedData.players[i] == winners[j].gameObject)
                    {
                        SharedData.scores[i]++;
                    }
                }
            }
            SharedData.mapCode += 1;
            SceneManager.LoadScene(SharedData.gameModeCode);
        }
    }
}
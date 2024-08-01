using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

// Credit for SharedData goes to user1702092 on StackExchange: https://stackoverflow.com/questions/54030622/how-do-i-carry-over-data-between-scenes-in-unity
public static class SharedData
{
    // Data for player 0 (Singleplayer)
    public static string characterCode0 = "bee";
    // Data for all other players
    public static string[] characterCodes = new string[4];
    public static InputDevice[] devices = new InputDevice[4];
    public static InputUser[] users = new InputUser[4];
    public static GameObject[] players = new GameObject[4];
    public static int[] scores = new int[4];

    public static int maxPlayers = 0;
    public static int currentPlayers = 0;
    public static int maxBots = 0;
    public static int currentBots = 0;

    // Code for game mode
    public static string gameModeCode;
    // Code for map
    public static int mapCode;
}
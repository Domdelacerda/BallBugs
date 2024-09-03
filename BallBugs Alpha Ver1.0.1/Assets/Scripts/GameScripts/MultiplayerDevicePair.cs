using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.InputSystem.XInput;

public class MultiplayerDevicePair : MonoBehaviour
{
    public float followDistance = 3f;

    // The input manager in this scene
    public PlayerInputManager inputManager;
    private GameObject[] players;
    public List<GameObject> levels;

    public CinemachineTargetGroup targetGroup;
    public CinemachineConfiner2D confiner;

    // The file location of the bug prefabs
    public const string BUG_PREFABS_LOCATION = "Prefabs/BugPrefabs/";

    // Start is called before the first frame update
    void Start()
    {
        SharedData.currentPlayers = SharedData.maxPlayers;
        SharedData.currentBots = SharedData.maxBots;
        players = new GameObject[SharedData.currentPlayers + SharedData.currentBots];
        for (int i = 0; i < players.Length; i++)
        {
            GameObject playerPrefab = Resources.Load<GameObject>(BUG_PREFABS_LOCATION + SharedData.characterCodes[i]);
            inputManager.playerPrefab = playerPrefab;
            if (SharedData.devices[i] != null)
            {
                PlayerInput input = inputManager.JoinPlayer(i, -1, null, SharedData.devices[i]);
                players[i] = input.gameObject;
            }
            else
            {
                players[i] = Instantiate(playerPrefab);
                players[i].layer = LayerMask.NameToLayer("Enemy");
            }
            targetGroup.AddMember(players[i].transform, 1, followDistance);
            if (SharedData.scores[i] != 0)
            {
                players[i].GetComponent<Bug>().score = SharedData.scores[i];
                players[i].GetComponent<Bug>().scoreText.text = SharedData.scores[i].ToString();
            }
            if (SharedData.devices[i] is XInputController)
            {
                players[i].GetComponent<Bug>().slingshotControls = false;
            }
        }
        if (levels[SharedData.mapCode % levels.Count] != null)
        {
            levels[SharedData.mapCode % levels.Count].GetComponent<Level>().LoadLevel(players);
        }
        SharedData.players = players;
    }

    private void FixedUpdate()
    {
        confiner.InvalidateCache();
    }
}
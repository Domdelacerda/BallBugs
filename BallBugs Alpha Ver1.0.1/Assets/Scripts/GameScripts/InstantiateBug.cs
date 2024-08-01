using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.InputSystem;

public class InstantiateBug : MonoBehaviour
{
    // The new bug prefab
    private GameObject bugPrefab;
    // The camera where the bug will be instantiated
    public Transform mainCameraTransform;
    // The amount of time it takes for the countdown timer to complete
    private const int countdownTime = 3;
    // Import Game Object that contains game over functionality
    public GameObject gameEnder;

    // This is the distance that the camera should move to switch between levels
    private const int DISTANCE_BETWEEN_LEVELS = 24;
    // This is the distance that the camera starts at by default
    private const int STARTING_POINT = 12;
    // The file path of where bugs are located
    private const string CHAR_PREFABS_LOCATION = "Prefabs/BugPrefabs/";

    // Function that instantiates and fully assembles a new bug
    void GenerateBug()
    {
        Vector3 mainCameraPosition = new Vector3(mainCameraTransform.localPosition.x, mainCameraTransform.localPosition.y, 0);
        GameObject bug = Instantiate(bugPrefab, mainCameraPosition, bugPrefab.transform.rotation);
        bug.GetComponent<PlayerMovement>().gameEnder = gameEnder;
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 mainCameraPosition = new Vector3(STARTING_POINT + DISTANCE_BETWEEN_LEVELS * SharedData.mapCode, mainCameraTransform.localPosition.y, 0);
        mainCameraTransform.localPosition = mainCameraPosition;
        // Set the bug prefab to the prefab that matches the character key
        bugPrefab = Resources.Load<GameObject>(CHAR_PREFABS_LOCATION + SharedData.characterCode0);

        StartCoroutine(Countdown());
    }

    // Countdown before the game starts
    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(countdownTime);
        GenerateBug();
    }
}
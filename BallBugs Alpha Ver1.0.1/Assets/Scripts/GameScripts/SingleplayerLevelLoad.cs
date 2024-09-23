//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a level loader that functions properly in singleplayer
//-----------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleplayerLevelLoad : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Singleplayer Level Loader enables each level based on the selection in
    /// the main menu. It also moves the location of the main camera to where
    /// the level is, and instantiates the bug selected from the main menu.
    /// </summary>-------------------------------------------------------------

    private GameObject bugPrefab;
    public Transform mainCameraTransform;
    public GameObject gameEnder;
    public List<GameObject> levels;

    private const int COUNTDOWN_TIME = 3;
    private const string CHAR_PREFABS_LOCATION = "Prefabs/BugPrefabs/";

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        levels[SharedData.mapCode].SetActive(true);
        Vector3 mainCameraPosition = 
            levels[SharedData.mapCode].transform.position;
        mainCameraTransform.localPosition = mainCameraPosition;
        bugPrefab = Resources.Load<GameObject>(CHAR_PREFABS_LOCATION 
            + SharedData.characterCode0);
        StartCoroutine(Countdown());
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Wait for the game start countdown to finish before generating the bug
    /// int the scene.
    /// </summary>
    /// <returns>coroutine that executes the bug creation event.</returns>
    /// -----------------------------------------------------------------------
    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(COUNTDOWN_TIME);
        GenerateBug();
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Generates the selected bug from the main menu, and assigns it a game
    /// ender component.
    /// </summary>-------------------------------------------------------------
    void GenerateBug()
    {
        GameObject bug = Instantiate(bugPrefab, mainCameraTransform.position,
            bugPrefab.transform.rotation);
        bug.GetComponent<PlayerMovement>().gameEnder = gameEnder;
    }
}
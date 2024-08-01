using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionGenerator : MonoBehaviour
{
    // Positional coordinates and an empty 2D vector are declared
    float x;
    float y;
    Vector2 pos;

    // The camera where the random range is centered
    public Transform mainCameraTransform;

    // Debug counter for how many times the object generates too close to terrain
    int debugCount = 0;

    public void GeneratePosition()
    {
        // New x and y values are selected at random from a range within the game window
        x = Random.Range(mainCameraTransform.localPosition.x - 12f, mainCameraTransform.localPosition.x + 12f);
        y = Random.Range(mainCameraTransform.localPosition.y - 4f, mainCameraTransform.localPosition.y + 4f);

        // The x and y values are assigned to the vector
        pos = new Vector2(x, y);

        // The position of the game object is set equal to the position of the vector
        transform.position = pos;
        if (Physics2D.OverlapCircle(pos, 1f, LayerMask.GetMask("Terrain")))
        {
            GeneratePosition();
            debugCount++;
        }
        // Debug.Log("Times fixed: " + debugCount);
        // Debug.Log(pos);
    }
}

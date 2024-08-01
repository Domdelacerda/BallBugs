using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    // Import Game Object that contains game over functionality
    public GameObject gameEnder;

    // Import text mesh
    public TextMeshProUGUI TimeText;

    // Time value used for text display
    private int time = 60;

    public void StartCounter()
    {
        TimeText.text = time.ToString();
        StartCoroutine(Count());
    }

    IEnumerator Count()
    {
        yield return new WaitForSeconds(1);
        time--;
        TimeText.text = time.ToString();
        if (time <= 0)
        {
            time = 0;
            gameEnder.GetComponent<GameOver>().GameOverSequence();
        }
        else
        {
            StartCoroutine(Count());
        }
    }
}

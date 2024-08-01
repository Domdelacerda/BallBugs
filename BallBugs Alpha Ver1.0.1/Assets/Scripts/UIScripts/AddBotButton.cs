using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBotButton : MonoBehaviour
{
    public GameObject botSelectorPrefab;

    public void AddBot()
    {
        if (SharedData.maxPlayers + SharedData.maxBots < 4)
        {
            Instantiate(botSelectorPrefab);
        }
    }
}

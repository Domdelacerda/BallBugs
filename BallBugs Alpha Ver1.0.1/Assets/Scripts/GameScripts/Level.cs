using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Transform[] spawnPoints = new Transform[4];
    public Transform mainCameraTransform;
    public CinemachineConfiner2D confiner;
    public PolygonCollider2D cameraBounds; 

    public void LoadLevel(GameObject[] players)
    {
        mainCameraTransform.SetPositionAndRotation(gameObject.transform.position, Quaternion.identity);
        confiner.m_BoundingShape2D = cameraBounds;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && spawnPoints[i] != null)
            {
                players[i].transform.SetPositionAndRotation(spawnPoints[i].position, Quaternion.identity);
            }
        }
    }
}

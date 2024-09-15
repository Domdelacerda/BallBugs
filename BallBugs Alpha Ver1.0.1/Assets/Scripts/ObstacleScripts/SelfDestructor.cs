using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    public float timeTillDestruction = 1f;

    void Start()
    {
        Destroy(gameObject, timeTillDestruction);
    }
}

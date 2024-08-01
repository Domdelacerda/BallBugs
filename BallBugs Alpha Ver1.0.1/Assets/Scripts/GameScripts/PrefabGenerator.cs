using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{
    public GameObject prefab;
    public float timeBetweenInstances;
    public float minScale;
    public float maxScale;
    public float maxOffset;
    // Start is called before the first frame update
    void Start()
    {
        GeneratePrefab();
    }

    private IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(timeBetweenInstances);
        GeneratePrefab();
    }

    private void GeneratePrefab()
    {
        float offset = Random.Range(-maxOffset, maxOffset);
        Vector3 position = new Vector3(gameObject.transform.position.x + offset, gameObject.transform.position.y + offset, gameObject.transform.position.z);
        GameObject instance = Instantiate(prefab, position, prefab.transform.rotation);
        float scale = Random.Range(minScale, maxScale);
        instance.transform.localScale = instance.transform.localScale * scale;
        StartCoroutine(SpawnDelay());
    }
}

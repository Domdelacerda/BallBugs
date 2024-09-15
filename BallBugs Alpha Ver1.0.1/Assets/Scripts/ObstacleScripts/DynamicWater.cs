using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class DynamicWater : MonoBehaviour
{
    public SpriteShapeController controller;
    public GameObject wavePointPrefab;
    public GameObject wavePoints;
    public List<WaterSpring> springs;
    public float springConstant = 0.5f;
    public float dampingRatio = 0.1f;
    public float spread = 0.005f;

    [Range(1, 100)]
    public int wavesCount = 15;

    private const int TOP_CORNERS = 2;

    private void OnValidate()
    {
        StartCoroutine(CreateWaves());
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < springs.Count; i++)
        {
            springs[i].SpringMotion(springConstant, dampingRatio);
            springs[i].WavePointUpdate();
        }
        Wave();
    }

    private IEnumerator CreateWaves()
    {
        foreach (Transform child in wavePoints.transform)
        {
            StartCoroutine(Destroy(child.gameObject));
        }
        yield return null;
        SetWaves();
        yield return null;
    }

    private IEnumerator Destroy(GameObject remove)
    {
        yield return null;
        DestroyImmediate(remove);
    }

    private void SetWaves()
    {
        Spline spline = controller.spline;
        int pointCount = spline.GetPointCount();
        for (int i = TOP_CORNERS; i < pointCount - TOP_CORNERS; i++)
        {
            spline.RemovePointAt(TOP_CORNERS);
        }
        Vector3 topLeft = spline.GetPosition(1);
        Vector3 topRight = spline.GetPosition(2);
        float waveSpacing = (topRight.x - topLeft.x) / wavesCount;
        for (int i = wavesCount - 1; i > 0; i--)
        {
            float xPosition = topLeft.x + waveSpacing * i;
            Vector3 wavePoint = new Vector3(xPosition, topLeft.y, topLeft.z);
            spline.InsertPointAt(TOP_CORNERS, wavePoint);
            spline.SetHeight(TOP_CORNERS, 0.1f);
            spline.SetCorner(TOP_CORNERS, false);
            spline.SetTangentMode(TOP_CORNERS, ShapeTangentMode.Continuous);
        }
        CreateSprings(spline);
    }

    private void SmoothWaves(Spline spline, int index)
    {
        Vector3 position = spline.GetPosition(index);
        Vector3 prevPosition = position;
        Vector3 nextPosition = position;
        if (index > 1)
        {
            prevPosition = spline.GetPosition(index - 1);
        }
        if (index <= wavesCount)
        {
            nextPosition = spline.GetPosition(index + 1);
        }
        Vector3 forward = gameObject.transform.forward;
        float scale = Mathf.Min((nextPosition - position).magnitude,
            (prevPosition - position).magnitude) * 0.33f;
        Vector3 leftTangent = (prevPosition - position).normalized * scale;
        Vector3 rightTangent = (nextPosition - position).normalized * scale;
        SplineUtility.CalculateTangents(position, prevPosition, nextPosition,
            forward, scale, out rightTangent, out leftTangent);
        spline.SetLeftTangent(index, leftTangent);
        spline.SetRightTangent(index, rightTangent);
    }

    private void CreateSprings(Spline spline)
    {
        springs = new List<WaterSpring>();
        for (int i = 0; i <= wavesCount; i++)
        {
            SmoothWaves(spline, i + 1);
            GameObject wavePoint = Instantiate(wavePointPrefab, 
                wavePoints.transform, false);
            wavePoint.transform.localPosition = spline.GetPosition(i + 1);
            WaterSpring waterSpring = wavePoint.GetComponent<WaterSpring>();
            waterSpring.Init(controller);
            springs.Add(waterSpring);
        }
    }

    private void Wave()
    {
        int count = springs.Count;
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                springs[i - 1].velocity += spread * (springs[i].height 
                    - springs[i - 1].height);
            }
            if (i < count - 1)
            {
                springs[i + 1].velocity += spread * (springs[i].height
                    - springs[i + 1].height);
            }
        }
    }
}
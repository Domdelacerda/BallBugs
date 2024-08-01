using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MultiplayerCamera : MonoBehaviour
{
    public List<Transform> targets;

    public Vector3 offset;
    private Vector3 velocity;
    private float dampTime = 0.5f;

    public float minZoom = 10f;
    public float maxZoom = 40f;
    public float zoomLimiter = 50f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        Bug[] players = FindObjectsOfType<Bug>();
        for (int i = 0; i < players.Length; i++)
        {
            targets.Add(players[i].transform);
        }
    }

    private void FixedUpdate()
    {
        if (targets.Count == 0)
        {
            return;
        }

        Move();
        Zoom();
    }

    private void Zoom()
    {
        float[] sizes = GetGreatestDistance();
        float newZoomX = Mathf.Lerp(minZoom, maxZoom, sizes[0] / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoomX, Time.deltaTime);
        float newZoomY = Mathf.Lerp(minZoom, maxZoom, sizes[1] / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoomY, Time.deltaTime);
    }

    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, dampTime);
    }

    private float[] GetGreatestDistance()
    {
        if (targets[0] == null)
        {
            return new float[] { 0, 0 };
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                bounds.Encapsulate(targets[i].position);
            }
        }
        return new float[] { bounds.size.x, bounds.size.y };
    }

    private Vector3 GetCenterPoint()
    {
        if (targets[0] == null)
        {
            return Vector3.zero;
        }

        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                bounds.Encapsulate(targets[i].position);
            }
        }
        return bounds.center;
    }
}

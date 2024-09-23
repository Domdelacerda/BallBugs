//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have dynamic water that interacts with objects that fall in it
//-----------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class DynamicWater : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Dynamic Water responds to the player's actions and the environment in
    /// multiple ways. Objects that enter the water deform the surface to
    /// create waves which propogate along the surface. Objects also create a
    /// small splash effect on entry and exit. Any projectiles that enter the
    /// water produce bubbles as they move. Players and projectiles that enter
    /// the water have their gravity and drag scales modified to simulate
    /// swimming motion and water resistance.
    /// </summary>-------------------------------------------------------------
    
    public GameObject bubblesPrefab;
    public GameObject splashPrefab;

    public float playerInWaterGravityScale = 0.05f;
    public float playerInWaterDragScale = 0.25f;

    public float projectileInWaterGravityScale = 1f;
    public float projectileInWaterDragScale = 1.5f;

    public SpriteShapeController controller;
    public GameObject wavePointPrefab;
    public GameObject wavePoints;
    public List<WaterSpring> springs;

    [Range(1, 100)]
    public int wavesCount = 15;
    public float springConstant = 0.5f;
    public float dampingRatio = 0.1f;
    public float spread = 0.005f;

    private const int TOP_CORNERS = 2;
    private const float BUBBLE_LIFESPAN = 5f;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    private void Start()
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

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    public void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (other.gameObject.CompareTag("Projectile"))
        {
            rb.gravityScale *= projectileInWaterGravityScale;
            rb.drag *= projectileInWaterDragScale;
            if (bubblesPrefab != null)
            {
                GameObject bubbles = Instantiate(bubblesPrefab, other.transform);
                bubbles.GetComponent<ParticleSystem>().trigger.AddCollider
                    (gameObject.GetComponent<Collider2D>());
            }
        }
        else if (other.gameObject.CompareTag("Bug"))
        {
            rb.gravityScale *= playerInWaterGravityScale;
            rb.drag *= playerInWaterDragScale;
        }
        if (gameObject.scene.isLoaded && splashPrefab != null)
        {
            GameObject splash = Instantiate(splashPrefab,
                other.transform.position, Quaternion.identity);
            ParticleSystem particles = splash.GetComponent<ParticleSystem>();
            particles.trigger.AddCollider
                (gameObject.GetComponent<Collider2D>());
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (other.gameObject.CompareTag("Projectile"))
        {
            rb.gravityScale /= projectileInWaterGravityScale;
            rb.drag /= projectileInWaterDragScale;
            ParticleSystem bubbles = 
                other.GetComponentInChildren<ParticleSystem>();
            if (bubbles != null)
            {
                bubbles.Stop();
                Destroy(bubbles.gameObject, BUBBLE_LIFESPAN);
            }
        }
        else if (other.gameObject.CompareTag("Bug"))
        {
            rb.gravityScale /= playerInWaterGravityScale;
            rb.drag /= playerInWaterDragScale;
        }
        if (gameObject.scene.isLoaded && splashPrefab != null)
        {
            GameObject splash = Instantiate(splashPrefab,
                other.transform.position, Quaternion.identity);
            ParticleSystem particles = splash.GetComponent<ParticleSystem>();
            particles.trigger.AddCollider
                (gameObject.GetComponent<Collider2D>());
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Create a new set of waves after the max number of waves is changed.
    /// </summary>
    /// <returns>coroutine that executes the new wave creation event.</returns>
    /// -----------------------------------------------------------------------
    private IEnumerator CreateWaves()
    {
        foreach (Transform child in wavePoints.transform)
        {
            StartCoroutine(Remove(child.gameObject));
        }
        yield return null;
        SetWaves();
        yield return null;
    }

    /// <summary>--------------------------------------------------------------
    /// Destroy old waves to make room for new ones.
    /// </summary>
    /// <returns>coroutine that executes the wave destruction event.</returns>
    /// -----------------------------------------------------------------------
    private IEnumerator Remove(GameObject remove)
    {
        yield return null;
        DestroyImmediate(remove);
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Creates a new set of wave points based on the specified number of waves
    /// and the position of the corners of the sprite shape.
    /// </summary>-------------------------------------------------------------
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

    /// <summary>--------------------------------------------------------------
    /// Smoothes out adjacent waves.
    /// </summary>-------------------------------------------------------------
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

    /// <summary>--------------------------------------------------------------
    /// Creates a new set of springs to correspond to the number of wave 
    /// points.
    /// </summary>-------------------------------------------------------------
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

    /// <summary>--------------------------------------------------------------
    /// Causes the waves to oscillate up and down as well as spread to
    /// adjacent waves.
    /// </summary>-------------------------------------------------------------
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
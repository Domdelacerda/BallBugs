//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Simulate the movement of water waves
//-----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.U2D;

public class WaterSpring : MonoBehaviour
{
    private SpriteShapeController controller;
    private int waveIndex = 0;
    public float velocity = 0f;
    public float force = 0f;
    public float height = 0f;
    public float defaultHeight = 0f;
    public float resistance = 40f;

    public void Init(SpriteShapeController spriteShapeController)
    {
        waveIndex = transform.GetSiblingIndex() + 1;
        controller = spriteShapeController;
        velocity = 0;
        height = transform.localPosition.y;
        defaultHeight = transform.localPosition.y;
    }

    public void SpringMotion(float springConstant, float dampingRatio)
    {
        height = transform.localPosition.y;
        force = -springConstant * (height - defaultHeight) - dampingRatio 
            * velocity;
        velocity += force;
        Vector3 prevPosition = transform.localPosition;
        transform.localPosition = new Vector3(prevPosition.x,
            prevPosition.y + velocity, prevPosition.z);
    }

    public void WavePointUpdate()
    {
        if (controller != null)
        {
            Spline spline = controller.spline;
            Vector3 wavePosition = spline.GetPosition(waveIndex);
            spline.SetPosition(waveIndex, new Vector3(wavePosition.x, 
                transform.localPosition.y, wavePosition.z));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.attachedRigidbody;
        velocity += rb.velocity.y / resistance;
    }
}

//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a slime puddle class that functions as intended
//-----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class SlimePuddle : MonoBehaviour
{
    /// <summary>--------------------------------------------------------------
    /// Slime Puddle is a secondary effect that is triggered when a Slimeball
    /// is destroyed. The slime puddle lingers for a long amount of time, and 
    /// causes enemy players to be slowed down when they enter it. Allies,
    /// including the snail that fired the projectile, will be able to stick
    /// to the slime and climb it up walls and on cielings.
    /// 
    /// ******************************UPGRADES*********************************
    /// 
    /// Poison I, II, III: Enemies touching a slime puddle will now take poison
    /// damage over time, 1 tick of damage per second. Each upgrade level
    /// increases the poison damage per second by 1.
    /// Regeneration I, II: Allies touching a slime puddle will now regenerate
    /// health over time, 1 tick of health per second. Each upgrade level
    /// increases the regeneration amount per second by 1.
    /// 
    /// </summary>-------------------------------------------------------------

    public BoxCollider2D puddleCollider;
    public BoxCollider2D puddleTrigger;

    private const float DISTANCE_FROM_TERRAIN = 20;
    private const float PRECISION_VALUE = 0.1f;
    private const int MIDPOINT_CONSTANT = 2;

    public float lifetime = 5f;
    public bool colliderImmediate = false;
    public float colliderDelay = 0.5f;
    
    public bool wallClimbing = true;
    public float gravityReductionScale = 0.2f;

    public int poisonDamage = 0;
    public int heal = 0;
    public float poisonInterval = 0;

    public GameObject owner;
    public Transform collisionTransform;
    public SpriteMask spriteSlice;
    
    public PhysicsMaterial2D defaultMaterial;
    public PhysicsMaterial2D stickyMaterial;

    protected const int PLAYER_LAYER = 9;
    protected const int ENEMY_LAYER = 10;
    protected const int SHIELD_LAYER = 11;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        Destroy(gameObject, lifetime);
        StartCoroutine(TriggerDeactivate());
        Slicer();
        if (colliderImmediate == false)
        {
            puddleCollider.enabled = false;
            StartCoroutine(ColliderActivate());
        }
        gameObject.transform.SetParent(collisionTransform);
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Bug")
        {
            Bug bug = other.gameObject.GetComponent<Bug>();
            Bug og = owner.gameObject.GetComponent<Bug>();
            if (bug.defaultLayer == og.defaultLayer && wallClimbing == true)
            {
                other.attachedRigidbody.gravityScale = 
                    other.attachedRigidbody.gravityScale 
                    * gravityReductionScale;
                if (heal != 0)
                {
                    bug.Regenerate(heal, poisonInterval, 500);
                }
            }
            else if (poisonDamage != 0)
            {
                bug.Poison(poisonDamage, poisonInterval, 500);
            }
            other.sharedMaterial = stickyMaterial;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Bug")
        {
            Bug bug = other.gameObject.GetComponent<Bug>();
            Bug og = owner.gameObject.GetComponent<Bug>();
            if (bug.defaultLayer == og.defaultLayer && wallClimbing == true)
            {
                other.attachedRigidbody.gravityScale = 
                    other.attachedRigidbody.gravityScale 
                    / gravityReductionScale;
                bug.CancelRegen();
            }
            else
            {
                bug.CurePoison();
            }
            other.sharedMaterial = defaultMaterial;
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Delays the activation of the slime puddle's collider to account for the
    /// time the animation takes to play out.
    /// </summary>
    /// <returns>the coroutine that re-enables the collider.</returns>
    /// -----------------------------------------------------------------------
    IEnumerator ColliderActivate()
    {
        yield return new WaitForSeconds(colliderDelay);
        puddleCollider.enabled = true;
    }

    /// <summary>--------------------------------------------------------------
    /// Deactivates the slime puddle's trigger 1 frame before it is deleted to
    /// ensure that the on trigger exit method fires before the object is
    /// deleted.
    /// </summary>
    /// <returns>the coroutine that re-enables the collider.</returns>
    /// -----------------------------------------------------------------------
    IEnumerator TriggerDeactivate()
    {
        yield return new WaitForSeconds(lifetime - Time.deltaTime);
        puddleTrigger.enabled = false;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    public void Slicer()
    {
        int iterations = Mathf.RoundToInt(puddleCollider.size.x 
            / PRECISION_VALUE);
        Vector2 leftBoundary = SliceFinder(iterations, -DISTANCE_FROM_TERRAIN);
        Vector2 rightBoundary = SliceFinder(iterations, DISTANCE_FROM_TERRAIN);
        Vector2 newMiddle = (leftBoundary + rightBoundary) / MIDPOINT_CONSTANT;
        Vector2 scale = gameObject.transform.localScale;
        Vector2 oldMiddle = puddleCollider.bounds.center;
        float zRotation = gameObject.transform.eulerAngles.z * Mathf.Deg2Rad;
        newMiddle.x -= Mathf.Sin(zRotation) * scale.x / DISTANCE_FROM_TERRAIN;
        newMiddle.y += Mathf.Cos(zRotation) * scale.y / DISTANCE_FROM_TERRAIN;
        puddleCollider.size = new Vector2(Vector2.Distance(leftBoundary,
            rightBoundary) / scale.x, puddleCollider.size.y);
        puddleTrigger.size = puddleCollider.size;
        zRotation = zRotation / Mathf.Deg2Rad;
        float direction = Mathf.Sign(oldMiddle.x - newMiddle.x);
        if (zRotation < -135)
        {
            direction = Mathf.Sign(oldMiddle.x - newMiddle.x);
        } 
        else if (zRotation < -45)
        {
            direction = Mathf.Sign(oldMiddle.y - newMiddle.y);
        }
        else if (zRotation < 45)
        {
            direction = Mathf.Sign(newMiddle.x - oldMiddle.x);
        }
        else if (zRotation < 135)
        {
            direction = Mathf.Sign(newMiddle.y - oldMiddle.y);
        }
        puddleCollider.offset = new Vector2(direction 
            * Vector2.Distance(newMiddle, oldMiddle) / scale.x, 0);
        puddleTrigger.offset = new Vector2(puddleCollider.offset.x,
            puddleTrigger.offset.y);
        spriteSlice.transform.localScale = 
            new Vector2(Vector2.Distance(leftBoundary, rightBoundary) 
            / scale.x, spriteSlice.transform.localScale.y);
        spriteSlice.transform.localPosition = 
            new Vector2(puddleCollider.offset.x, 
            spriteSlice.transform.localPosition.y);
    }

    public Vector2 SliceFinder(int iterations, float signedTerrainDistance)
    {
        Vector2 scale = gameObject.transform.localScale;
        float zRotation = gameObject.transform.eulerAngles.z * Mathf.Deg2Rad;
        float incrementX = Mathf.Cos(zRotation) * scale.x 
            / signedTerrainDistance;
        float incrementY = Mathf.Sin(zRotation) * scale.y 
            / signedTerrainDistance;
        Vector2 middle = puddleCollider.bounds.center;
        middle.x += Mathf.Sin(zRotation) * scale.x / DISTANCE_FROM_TERRAIN;
        middle.y -= Mathf.Cos(zRotation) * scale.y / DISTANCE_FROM_TERRAIN;
        Vector2 slicePoint = new Vector2(middle.x + (incrementX * iterations),
            middle.y + (incrementY * iterations));
        for (int i = 0; i < iterations; i++)
        {
            middle.x += incrementX;
            middle.y += incrementY;
            Vector2 checkPoint = new Vector2(middle.x, middle.y);
            if (!Physics2D.OverlapPoint(checkPoint, 
                LayerMask.GetMask("Terrain")))
            {
                slicePoint = checkPoint;
                break;
            }
        }
        return slicePoint;
    }
}
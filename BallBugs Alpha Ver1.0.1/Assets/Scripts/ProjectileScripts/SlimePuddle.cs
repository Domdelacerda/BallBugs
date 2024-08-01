using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlimePuddle : MonoBehaviour
{
    // The collider of the puddle
    public BoxCollider2D puddleCollider;
    // The trigger of the puddle used for wallclimbing interaction
    public BoxCollider2D puddleTrigger;

    // How long the puddle will last before it's deleted
    public float lifetime = 5f;
    // This float represents how far below the puddle's collider terrain will be
    // Previously just used the precisionValue for that, but if precisionValue was
    // Below 0.1 then SliceFinder would not work
    // NEVER CHANGE THIS! 
    private const float distanceFromTerrain = 20;
    // The amount of precision wanted for where the collider should be sliced
    // NEVER change this one either since I messed something up with it
    private const float precisionValue = 0.1f;
    // The midpoint constant is used for finding the middle between 2 points
    private const int midpointConstant = 2;
    // Whether the puddle's collider is activated immediately on instantiation or not
    // This exists because of how long the puddle animation takes to play out
    public bool colliderImmediate = false;
    // The amount of time it takes after instantiation before the collider is activated
    // This number only matters if colliderImmediate is false
    public float colliderDelay = 0.5f;
    // The wallClimbing variable allows the snail to stick to it's own slime puddles
    // While they are on walls or even on cielings
    public bool wallClimbing = true;
    // The gravityReductionScale determines the percent that gravity is reduced by
    // When the snail is on it's slime puddles.
    // This variable only matters if wallClimbing is true
    public float gravityReductionScale = 0.2f;
    // The splashKinematic variable is used to determine if the puddle will become a child of
    // The object that it collides with or not. If the slime ball collides with a moving object
    // And splashKinematic is true, then it will move along with the object
    public bool splashKinematic = false;

    public int poisonDamage = 0;
    public int heal = 0;
    public float poisonInterval = 0;
    public int numPoisonIntervals = 0;

    // The bug that owns this slimeball
    public GameObject owner;

    // The collisionTransform Transform is the transform of the object that the
    // Slimeball hit
    public Transform collisionTransform;
    // Physics materials for when a bug comes in contact or exits a puddle
    public PhysicsMaterial2D defaultMaterial;
    public PhysicsMaterial2D stickyMaterial;

    // The player layer int represents the layer that all player characters exist on
    protected const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;
    // The shield layer int represents the layer that all shields exist on
    protected const int shieldLayer = 11;

    // Start is called before the first frame update
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
        if (splashKinematic == true)
        {
            gameObject.transform.SetParent(collisionTransform);
        }
    }

    // Code for Slicer made by me :D
    public void Slicer()
    {
        // Iterations is how many times the pointFinder loop will iterate
        // It is determined by the collider's width divided by precisionValue
        int iterations = Mathf.RoundToInt(puddleCollider.size.x / precisionValue);

        // The boundaries where collider slicing will occur
        Vector2 leftBoundary = SliceFinder(iterations, -distanceFromTerrain);
        Vector2 rightBoundary = SliceFinder(iterations, distanceFromTerrain);
        // The midpoint between the two boundaries
        Vector2 newMiddle = new Vector2((leftBoundary.x + rightBoundary.x) / midpointConstant, (leftBoundary.y + rightBoundary.y) / midpointConstant);
        // Before using, newMiddle has to be adjusted the opposite way that the boundaries were adjusted,
        // so instead of being under the collider to check for terrain, it is back in the center
        newMiddle.x -= Mathf.Sin(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad) * (gameObject.transform.localScale.x / distanceFromTerrain);
        newMiddle.y += Mathf.Cos(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad) * (gameObject.transform.localScale.y / distanceFromTerrain);
        // Debug.Log("Left: " + leftBoundary + "/ Right: " + rightBoundary + "/ Middle: " + newMiddle);
        // Debug.Log("Center: " + puddleCollider.bounds.center);

        // Change the width of the collider to the distance between the two edges
        // Whenever you want to augment a property of transform, which controls position, scale, and rotation
        // You have to create a new vector. You CANNOT do something like this: puddleCollider.size.y = 1
        puddleCollider.size = new Vector2(Vector2.Distance(leftBoundary, rightBoundary) / gameObject.transform.localScale.x, puddleCollider.size.y);
        puddleTrigger.size = new Vector2(puddleCollider.size.x, puddleTrigger.size.y);
        // Big stupid if-else statement used to determine which direction the collider should be offset
        // It has to be big and stupid because objects that don't have a transform component
        // (A.K.A. newMiddle, which is not a gameObject but just data representing a point in space)
        // Can't be rotated, so they must use the x and y coordinates of the world space rather than relative ones
        float direction = Mathf.Sign(puddleCollider.bounds.center.y - newMiddle.y);
        if (gameObject.transform.eulerAngles.z < -135)
        {
            direction = Mathf.Sign(puddleCollider.bounds.center.x - newMiddle.x);
        } 
        else if (gameObject.transform.eulerAngles.z < -45)
        {
            direction = Mathf.Sign(puddleCollider.bounds.center.y - newMiddle.y);
        }
        else if (gameObject.transform.eulerAngles.z < 45)
        {
            direction = Mathf.Sign(newMiddle.x - puddleCollider.bounds.center.x);
        }
        else if (gameObject.transform.eulerAngles.z < 135)
        {
            direction = Mathf.Sign(newMiddle.y - puddleCollider.bounds.center.y);
        }
        else if (gameObject.transform.eulerAngles.z <= 180)
        {
            direction = Mathf.Sign(puddleCollider.bounds.center.x - newMiddle.x);
        }
        // IMPORTANT: When using offset, the x and y inputs do not refer to the same x and y values as the world space
        // The direction that offset classifies as "up" changes depending on the collider's rotation
        // When the collider is rotated 90 degrees, the direction of "up" is now on the x axis instead of the y
        // For this reason, the y input of offset is zero since the collider should never move "up"
        puddleCollider.offset = new Vector2(direction * Vector2.Distance(newMiddle, puddleCollider.bounds.center) / gameObject.transform.localScale.x, 0);
        puddleTrigger.offset = new Vector2(puddleCollider.offset.x, puddleTrigger.offset.y);

        // Each slimePuddle gameObject has a child gameObject that contains a SpriteMask, accessed here:
        SpriteMask spriteSlice = gameObject.GetComponentInChildren(typeof(SpriteMask)) as SpriteMask;
        // Inside of the slimePuddle's spriteRenderer component, it's mask interaction parameter is set to "Visible Inside Mask"
        // This means that the slimePuddle's sprite will only be visible inside of the SpriteMask's extents
        // So to reflect the size and extents of the collider, the child gameObject is set to the same size and position
        spriteSlice.transform.localScale = new Vector2(Vector2.Distance(leftBoundary, rightBoundary) / gameObject.transform.localScale.x, spriteSlice.transform.localScale.y);
        spriteSlice.transform.localPosition = new Vector2(puddleCollider.offset.x, spriteSlice.transform.localPosition.y);
    }

    // Code for SliceFinder made by me :D
    public Vector2 SliceFinder(int iterations, float signedTerrainDistance)
    {
        // Values used to increment the checkPoint; based on precision parameter and the puddle's rotation
        float incrementX = Mathf.Cos(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad) * (gameObject.transform.localScale.x / signedTerrainDistance);
        float incrementY = Mathf.Sin(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad) * (gameObject.transform.localScale.y / signedTerrainDistance);
        // Debug.Log("Rotation: " + gameObject.transform.eulerAngles.z);
        // Debug.Log("X Increment: " + incrementX + " / Y Increment: " + incrementY);

        // The middle vector will always be "below" the slime puddle regardless of rotation
        // It is used to check whether there is still a collider "below" the puddle or not
        Vector2 middle = puddleCollider.bounds.center;
        // Middle is offset by the value of incrementY and incrementX,
        // But using the unsigned distanceFromTerrain value instead of the signed precision value
        // This is so that no matter the rotation, the checkPoint is always "under" the puddle
        middle.x += Mathf.Sin(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad) * (gameObject.transform.localScale.x / distanceFromTerrain);
        middle.y -= Mathf.Cos(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad) * (gameObject.transform.localScale.y / distanceFromTerrain);

        // The slicePoint vector is SliceFinder's return value
        // The default value is the farthest edge of the collider (where no slicing will occur)
        Vector2 slicePoint = new Vector2(middle.x + (incrementX * iterations), middle.y + (incrementY * iterations));

        for (int i = 0; i < iterations; i++)
        {
            // Each iteration, the middle is moved by an amount and in a direction determined by the precisionValue
            middle.x += incrementX;
            middle.y += incrementY;

            // The checkPoint vector exists to check whether there is still a collider under the puddle or not
            Vector2 checkPoint = new Vector2(middle.x, middle.y);

            // If the checkPoint is not overlapping any colliders (A.K.A. if there is no collider beneath the puddle)
            // The puddle's collider will be sliced at that point so that the puddle doesn't extend past platforms
            if (!Physics2D.OverlapPoint(checkPoint, LayerMask.GetMask("Terrain")))
            {
                slicePoint = checkPoint;
                break;
            }
        }
        return slicePoint;
    }

    // When the snail enters the trigger, it's gravity is reduced which allows it to move faster and climb walls
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetInstanceID() == owner.GetInstanceID() && wallClimbing == true)
        {
            other.attachedRigidbody.gravityScale = other.attachedRigidbody.gravityScale * gravityReductionScale;
            other.gameObject.GetComponent<Bug>().Regenerate(heal, poisonInterval, numPoisonIntervals);
        }
        else if ((other.gameObject.layer == playerLayer || other.gameObject.layer == enemyLayer) && poisonDamage != 0)
        {
            other.gameObject.GetComponent<Bug>().Poison(poisonDamage, poisonInterval, numPoisonIntervals);
        }
        other.sharedMaterial = stickyMaterial;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetInstanceID() == owner.GetInstanceID() && heal != 0)
        {
            collision.gameObject.GetComponent<Bug>().Regenerate(heal, poisonInterval, numPoisonIntervals);
        }
        else if ((collision.gameObject.layer == playerLayer || collision.gameObject.layer == enemyLayer) && poisonDamage != 0)
        {
            collision.gameObject.GetComponent<Bug>().Poison(poisonDamage, poisonInterval, numPoisonIntervals);
        }
    }

    // When the snail exits the trigger, it's gravity is returned to normal
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetInstanceID() == owner.GetInstanceID() && wallClimbing == true)
        {
            other.attachedRigidbody.gravityScale = other.attachedRigidbody.gravityScale / gravityReductionScale;
            //other.gameObject.GetComponent<Bug>().CancelRegen();
        }
        /*
        else if ((other.gameObject.layer == playerLayer || other.gameObject.layer == enemyLayer) && poisonDamage != 0)
        {
            other.gameObject.GetComponent<Bug>().CurePoison();
        }
        */
        other.sharedMaterial = defaultMaterial;
    }

    IEnumerator ColliderActivate()
    {
        yield return new WaitForSeconds(colliderDelay);
        // After the amount of time that the animation plays has elapsed, the collider is activated
        puddleCollider.enabled = true;
    }

    // TriggerDeactivate is necessary because if the puddle is deleted while the snail is still
    // Stuck on it, it will maintain reduced gravity after the puddle is gone.
    // Deactivating the trigger right before the puddle is deleted solves this issue
    IEnumerator TriggerDeactivate()
    {
        yield return new WaitForSeconds(lifetime - precisionValue);
        puddleTrigger.enabled = false;
    }
}
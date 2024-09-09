//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a rolypoly class that is fun to play
//-----------------------------------------------------------------------------

using UnityEngine;

public class Rolypoly : Bug, ISlingshot
{
    /// <summary>--------------------------------------------------------------
    /// Rolypoly is one of the player characters in the game. Rolypoly is a 
    /// melee class that attacks by launching its own body at opponents. 
    /// The charge meter determines Rolypoly's initial velocity when it
    /// launches itself at enemies. When attacking, the charge meter slowly
    /// drains over time, and Rolypoly deals damage based on how much charge is
    /// remaining in the meter.
    /// </summary>-------------------------------------------------------------
    
    public GameObject Visualizer;
    public GameObject AirResistance;

    public float maxPower = 10f;
    public float chargeDecay = 1.5f;
    public float homingRadius = 10f;
    public float chargePerBounce = 0.2f;
    private const float POWER_MULTIPLIER = 1.5f;

    public int rotationSpeed = 30;
    public int knockbackForce = 300;
    public int maxDamage = 40;
    public int maxBounces = 1;
    private int bounces = 0;

    public float invincibilityTime = 0.5f;

    private bool attacking = false;
    public bool shieldCharge = false;
    public bool homingBounce = false;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        defaultLayer = gameObject.layer;
        DisplayHealthBar(maxHealth, health);
        rechargeRoutine = StartCoroutine(Recharge());
    }

    void Update()
    {
        if (joystickDraw.magnitude == 0f && recharged == true && primed == true
            && slingshotControls == true && wrapped == false)
        {
            slingshotMode = true;
            Sling();
            Release();
        }
        if (joystickDraw.magnitude != 0f && recharged == true && 
            wrapped == false)
        {
            ChargingUp(true);
            CalculateVisualizer(currentCharge);
            SetVisualizerActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
            if (recharged == true && shoot == true)
            {
                slingshotMode = false;
                Sling();
                recharged = false;
                SetVisualizerActive(false);
            }
        }
        else
        {
            SetVisualizerActive(false);
            currentCharge -= chargeRate * chargeDecay * Time.deltaTime;
            if (currentCharge < 0f)
            {
                currentCharge = 0f;
                if (attacking == true)
                {
                    attacking = false;
                    bugAnimator.SetBool("IsRecharged", false);
                    StartCoroutine(Recharge());
                }
            }
            else if (currentCharge > 1)
            {
                currentCharge = 1f;
            }
        }
        shoot = false;
        SetDamageActive(attacking);
    }

    void FixedUpdate()
    {
        var aimingAngle = Mathf.Atan2(joystickDraw.x, joystickDraw.y) 
            * Mathf.Rad2Deg * -1f;
        rb.rotation += currentCharge * rotationSpeed 
            * Mathf.Sign(joystickDraw.x);
        if (joystickDraw.magnitude != 0f)
        {
            Visualizer.transform.localEulerAngles = new Vector3(0, 0, 
                aimingAngle - rb.rotation);
        }
        if (attacking)
        {
            var motionAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.y) 
                * Mathf.Rad2Deg * -1f;
            AirResistance.transform.localEulerAngles = new Vector3(0, 0, 
                motionAngle - rb.rotation);
            Color oldColor = 
                AirResistance.GetComponent<Renderer>().material.color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, 
                currentCharge);
            AirResistance.GetComponent<Renderer>().material.SetColor("_Color", 
                newColor);
        }
        healthBarCanvas.GetComponent<HealthBar>().UpdateChargeBar
            (currentCharge);
    }

    //-------------------------------------------------------------------------
    // COLLISION EVENTS
    //-------------------------------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        bounces--;
        if (homingBounce == true && attacking == true && bounces >= 0)
        {
            if (collision.gameObject.CompareTag("Terrain"))
            {
                currentCharge += chargePerBounce;
                Collider2D[] targets = Physics2D.OverlapCircleAll
                    (gameObject.transform.position, homingRadius,
                    LayerMask.GetMask("Player"));
                if (defaultLayer == PLAYER_LAYER && targets.Length < 2)
                {
                    targets = Physics2D.OverlapCircleAll
                        (gameObject.transform.position, homingRadius,
                        LayerMask.GetMask("Enemy"));
                }
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != gameObject.GetComponent<Collider2D>())
                    {
                        Vector2 direction = new Vector2
                            (targets[i].transform.position.x 
                            - gameObject.transform.position.x,
                            targets[i].transform.position.y 
                            - gameObject.transform.position.y);
                        rb.velocity = direction.normalized 
                            * collision.relativeVelocity.magnitude 
                            * POWER_MULTIPLIER;
                    }
                }
            }
        }
        GameObject parent = collision.gameObject.gameObject;
        if (collision.gameObject.layer == SHIELD_LAYER && attacking == true)
        {
            Bug bug = collision.gameObject.GetComponent<Bug>();
            if (bug == null)
            {
                bug = collision.gameObject.GetComponentInParent<Bug>();
            }
            if (bug.defaultLayer != defaultLayer)
            {
                bug.Shield(Mathf.RoundToInt(maxDamage * currentCharge));
                bug.InvincibilityFrames(invincibilityTime);
            }
        }
        else if ((parent.layer == PLAYER_LAYER || (parent.layer == ENEMY_LAYER
            && defaultLayer != ENEMY_LAYER)) && attacking == true)
        {
            Vector2 thisPosition = new Vector2
                (gameObject.transform.localPosition.x, 
                gameObject.transform.localPosition.y);
            Vector2 otherPosition = new Vector2
                (parent.transform.localPosition.x,
                parent.transform.localPosition.y);
            Vector2 distanceVector = new Vector2(otherPosition.x 
                - thisPosition.x, otherPosition.y - thisPosition.y);
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce
                (distanceVector.normalized * currentCharge * knockbackForce);
            collision.gameObject.GetComponent<Bug>().Damage
                (Mathf.RoundToInt(maxDamage * currentCharge));
            parent.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
    }

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Launches rolypoly in the direction the joystick was drawn before it
    /// was released.
    /// </summary>-------------------------------------------------------------
    public void Sling()
    {
        if (slingshotMode == true)
        {
            rb.velocity = joystickDrawSaveStates[2] * -1 * currentCharge
                * maxPower;
        }
        else
        {
            rb.velocity = -joystickDraw * currentCharge * maxPower;
        }
        attacking = true;
        bounces = maxBounces;
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Enables or disables the air resistance renderer, which indicates when
    /// rolypoly is able to deal damage. If the shield charge skill is enabled,
    /// it also enables or disables rolypoly's shield status.
    /// </summary>
    /// <param name="active">whether damage is being enabled or disabled.
    /// </param>
    /// -----------------------------------------------------------------------
    void SetDamageActive(bool active)
    {
        AirResistance.GetComponent<Renderer>().enabled = active;
        if (shieldCharge == true)
        {
            if (active == true)
            {
                gameObject.layer = SHIELD_LAYER;
            }
            else
            {
                gameObject.layer = defaultLayer;
            }
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Enables or disables the trajectory visualizer to see where a shot is 
    /// going to be fired.
    /// </summary>
    /// <param name="active">whether the visualizer is being enabled or 
    /// disabled.</param>
    /// -----------------------------------------------------------------------
    void SetVisualizerActive(bool active)
    {
        Visualizer.GetComponent<Renderer>().enabled = active;
    }

    /// <summary>--------------------------------------------------------------
    /// Calculates the new visualizer size based on the current charge and 
    /// joystick draw of firefly. 
    /// </summary>
    /// <param name="charge">the current charge of rolypoly.</param>
    /// -----------------------------------------------------------------------
    void CalculateVisualizer(float charge)
    {
        Visualizer.transform.localScale = 
            Visualizer.transform.localScale.normalized * (1f + charge) 
            * POWER_MULTIPLIER * joystickDraw.magnitude;
    }
}
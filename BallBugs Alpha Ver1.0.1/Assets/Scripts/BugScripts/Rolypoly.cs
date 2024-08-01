using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rolypoly : Bug, ISlingshot
{
    // The visualizer component used for aiming
    public GameObject Visualizer;
    // The air resistance/knockback game object
    public GameObject AirResistance;

    // The maximum power factor of rolypoly's charge attack
    public float maxPower = 10f;
    // The factor by which current charge decays
    public float chargeDecay = 1.5f;
    // The radius in which homing bounce searches for enemies
    public float homingRadius = 10f;
    // The number of charge regained per bounce
    public float chargePerBounce = 0.2f;

    // The speed at which rolypoly rolls while aiming
    public int rotationSpeed = 30;
    // The force of knockback on enemies when they are hit
    public int knockbackForce = 300;
    // The maximum damage dealth by rolypoly's charge attack
    public int maxDamage = 40;
    // The number of homing bounces
    public int maxBounces = 1;
    // Tally for number of bounces remaining
    private int bounces = 0;

    // The amount of invincibility time in seconds that rolypoly's attack gives
    public float invincibilityTime = 0.5f;

    // A variable to keep track of whether rolypoly can deal damage or not
    private bool attacking = false;
    public bool shieldCharge = false;
    public bool homingBounce = false;

    void Start()
    {
        defaultLayer = gameObject.layer;
        DisplayHealthBar(maxHealth, health);
        rechargeRoutine = StartCoroutine(Recharge());
    }

    // Update executes every frame
    void Update()
    {
        // Slingshot shooting controls (on joystick release)
        if (joystickDraw.magnitude == 0f && recharged == true && primed == true && slingshotControls == true && wrapped == false)
        {
            slingshotMode = true;
            Sling();
            Release();
        }
        // If the joystick is not centered (if it is being pulled back)
        if (joystickDraw.magnitude != 0f && recharged == true && wrapped == false)
        {
            ChargingUp(true);
            CalculateVisualizer(currentCharge);
            SetVisualizerActive(true);
            for (int i = joystickDrawSaveStates.Length - 1; i > 0; i--)
            {
                joystickDrawSaveStates[i] = joystickDrawSaveStates[i - 1];
            }
            joystickDrawSaveStates[0] = joystickDraw;
            // Manual shooting controls (on button press)
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
            if (currentCharge <= 0f)
            {
                currentCharge = 0f;
                if (attacking == true)
                {
                    attacking = false;
                    bugAnimator.SetBool("IsRecharged", false);
                    StartCoroutine(Recharge());
                }
            }
        }
        shoot = false;
        SetDamageActive(attacking);
    }

    // Implementation for the sling interface
    public void Sling()
    {
        // Move roly poly in the direction that the player is aiming
        if (slingshotMode == true)
        {
            rb.velocity = joystickDrawSaveStates[2] * -1 * currentCharge * maxPower;
        }
        else
        {
            rb.velocity = -joystickDraw * currentCharge * maxPower;
        }
        attacking = true;
        // rb.AddForce(joystickDraw * currentCharge * maxPower);
        bounces = maxBounces;
    }

    // Allow rolypoly to deal damage when active and don't allow otherwise
    void SetDamageActive(bool active)
    {
        AirResistance.GetComponent<Renderer>().enabled = active;
        AirResistance.GetComponent<CircleCollider2D>().enabled = active;
        if (shieldCharge == true)
        {
            if (active == true)
            {
                gameObject.layer = shieldLayer;
            }
            else
            {
                gameObject.layer = defaultLayer;
            }
        }
    }

    // Credit SetVisualizerActive goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void SetVisualizerActive(bool active)
    {
        Visualizer.GetComponent<Renderer>().enabled = active;
    }

    // Credit CalculateVisualizer goes to NightShade on youtube: https://youtu.be/kRgFiCjdLpY
    void CalculateVisualizer(float charge)
    {
        Visualizer.transform.localScale = Visualizer.transform.localScale.normalized * (1f + currentCharge) * 1.5f * joystickDraw.magnitude;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bounces--;
        if (homingBounce == true && attacking == true && bounces >= 0)
        {
            if (collision.gameObject.CompareTag("Terrain"))
            {
                currentCharge += chargePerBounce;
                if (currentCharge > 1)
                {
                    currentCharge = 1;
                }
                Collider2D[] targets = Physics2D.OverlapCircleAll(gameObject.transform.position, homingRadius, LayerMask.GetMask("Player"));
                if (defaultLayer == playerLayer && targets.Length < 2)
                {
                    targets = Physics2D.OverlapCircleAll(gameObject.transform.position, homingRadius, LayerMask.GetMask("Enemy"));
                }
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != gameObject.GetComponent<Collider2D>())
                    {
                        Vector2 direction = new Vector2(targets[i].transform.position.x - gameObject.transform.position.x,
                            targets[i].transform.position.y - gameObject.transform.position.y);
                        rb.velocity = direction.normalized * collision.relativeVelocity.magnitude * 1.5f;
                    }
                }
            }
        }
        // On collision with a shield
        if (collision.gameObject.layer == shieldLayer && attacking == true)
        {
            // Shield script here
            if (collision.gameObject.GetComponent<Bug>() != null)
            {
                if (collision.gameObject.GetComponent<Bug>().defaultLayer != defaultLayer)
                {
                    collision.gameObject.GetComponent<Bug>().Shield(Mathf.RoundToInt(maxDamage * currentCharge));
                    collision.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
                }
            }
            else
            {
                if (collision.gameObject.GetComponentInParent<Bug>().defaultLayer != defaultLayer)
                {
                    collision.gameObject.GetComponentInParent<Bug>().Shield(Mathf.RoundToInt(maxDamage * currentCharge));
                    collision.gameObject.GetComponentInParent<Bug>().InvincibilityFrames(invincibilityTime);
                }
            }
        }
        else if ((collision.gameObject.gameObject.layer == playerLayer || (collision.gameObject.gameObject.layer == enemyLayer 
            && defaultLayer != enemyLayer)) && attacking == true)
        {
            // Knock the player backwards
            Vector2 thisPosition = new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y);
            Vector2 otherPosition = new Vector2(collision.gameObject.gameObject.transform.localPosition.x, collision.gameObject.gameObject.transform.localPosition.y);
            Vector2 distanceVector = new Vector2(otherPosition.x - thisPosition.x, otherPosition.y - thisPosition.y);
            //other.attachedRigidbody.velocity = new Vector2(0, 0);
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(distanceVector.normalized * currentCharge * knockbackForce);
            // Damage the player
            collision.gameObject.GetComponent<Bug>().Damage(Mathf.RoundToInt(maxDamage * currentCharge));
            collision.gameObject.gameObject.GetComponent<Bug>().InvincibilityFrames(invincibilityTime);
        }
    }

    void FixedUpdate()
    {
        // The x and y position of the joystick are used to determine it's angle of rotation
        // This formula can be used to find the rotation of ANY VECTOR. Keep this in mind
        var aimingAngle = Mathf.Atan2(joystickDraw.x, joystickDraw.y) * Mathf.Rad2Deg * -1f;
        rb.rotation += currentCharge * rotationSpeed * Mathf.Sign(joystickDraw.x);
        if (joystickDraw.magnitude != 0f)
        {
            Visualizer.transform.localEulerAngles = new Vector3(0, 0, aimingAngle - rb.rotation);
        }
        if (attacking)
        {
            // The air resistance will rotate in the direction that rolypoly moving
            var motionAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.y) * Mathf.Rad2Deg * -1f;
            AirResistance.transform.localEulerAngles = new Vector3(0, 0, motionAngle - rb.rotation);
            // Change the transparency of the air resistance based on how much charge is left
            Color oldColor = AirResistance.GetComponent<Renderer>().material.color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, currentCharge);
            AirResistance.GetComponent<Renderer>().material.SetColor("_Color", newColor);
        }
        healthBarCanvas.GetComponent<HealthBar>().UpdateChargeBar(currentCharge);
    }
}
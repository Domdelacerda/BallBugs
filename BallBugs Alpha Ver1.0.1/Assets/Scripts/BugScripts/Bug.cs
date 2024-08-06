//-----------------------------------------------------------------------------
// Contributor(s): Dominic De La Cerda
// Project: BallBugs - 2D physics-based fighting game
// Purpose: Have a generic bug class that other bugs inherit from
//-----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Bug : MonoBehaviour, IDamageable, IPoisonable, IShieldable, 
    IHealable, IRegenerable, IFrames, IWrappable
{
    /// <summary>--------------------------------------------------------------
    /// Bug contains attributes and methods common among all bugs: stats such
    /// as the bug's score and health, states such as whether the bug is
    /// poisoned or reloaded, and actions such as aiming and charging. Bug
    /// exists so that the more specific bug classes don't have to repeat the
    /// same code and can instead use inheritance.
    /// </summary>-------------------------------------------------------------

    public Animator bugAnimator;
    public RuntimeAnimatorController bugController;

    public bool shoot = false;
    public bool recharged = false;
    public bool invincible = false;
    public bool primed = false;
    protected bool chargingJump = false;
    protected bool jumpReady = false;
    public bool slingshotMode = false;
    public bool slingshotControls = true;

    public bool wrapped = false;
    public bool poisoned = false;
    public bool regenerating = false;

    public int maxHealth = 100;
    public int health = 100;
    public float armor = 0f;
    public float shieldArmor = 0f;

    public int score = 0;
    
    public float cooldownTime = 3f;
    public float currentCharge = 0f;
    public float chargeRate = 0.25f;
    public int comboCounter = 0;

    public Rigidbody2D rb;
    public GameObject persistentHitbox;

    public GameObject healthBarCanvas;
    public TextMeshProUGUI scoreText;

    public GameObject damageDisplay;
    public GameObject healDisplay;
    public GameObject shieldDisplay;
    public GameObject poisonDisplay;

    private const float FLASH_INTERVAL = 0.075f;

    public Vector2 joystickDraw;
    public Vector2[] joystickDrawSaveStates = new Vector2[3];

    protected const int PLAYER_LAYER = 9;
    protected const int ENEMY_LAYER = 10;
    protected const int SHIELD_LAYER = 11;
    public int defaultLayer;

    public SpringJoint2D grapple;
    public Vector2 grapplePos;
    public GameObject currentWeb;
    protected Vector2 distance;

    protected Coroutine rechargeRoutine;
    protected Coroutine poisonRoutine;
    protected Coroutine regenRoutine;

    //-------------------------------------------------------------------------
    // GENERATED METHODS
    //-------------------------------------------------------------------------

    void Start()
    {
        defaultLayer = gameObject.layer;
        DisplayHealthBar(maxHealth, health);
        rechargeRoutine = StartCoroutine(Recharge());
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Set the player's aiming direction to the same direction that the aiming
    /// joystick is drawn back every frame, unless the player is currently
    /// immobilized.
    /// </summary>
    /// <param name="ctx">the action input that determines the direction the
    /// joystick is pulled in.</param>
    /// -----------------------------------------------------------------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (wrapped == false)
        {
            joystickDraw = ctx.ReadValue<Vector2>();
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Allow the player to attack when the attack button is pressed as long as
    /// they aren't currently immobilized.
    /// </summary>
    /// <param name="ctx">the action input that determines whether the player
    /// used the attack input or not.</param>
    /// -----------------------------------------------------------------------
    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && wrapped == false)
        {
            shoot = true;
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Allow the player to jump when the jump button is pressed as long as
    /// they aren't currently immobilized.
    /// </summary>
    /// <param name="ctx">the action input that determines whether the player
    /// used the jump input or not.</param>
    /// -----------------------------------------------------------------------
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && wrapped == false)
        {
            Ungrapple();
        }
        chargingJump = ctx.performed;
    }

    /// <summary>--------------------------------------------------------------
    /// Allow the player to crouch when the crouch button is pressed as long as
    /// they aren't currently immobilized.
    /// </summary>
    /// <param name="ctx">the action input that determines whether the player
    /// used the crouch input or not.</param>
    /// -----------------------------------------------------------------------
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && wrapped == false)
        {
            Ungrapple();
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Allow the player to pause the game when a pause button is pressed.
    /// </summary>
    /// <param name="ctx">the action input that determines whether the player
    /// used the pause input or not.</param>
    /// -----------------------------------------------------------------------
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameObject[] pauseButtons = GameObject.FindGameObjectsWithTag("Back");
            for (int i = 0; i < pauseButtons.Length; i++)
            {
                if (pauseButtons[i].GetComponent<Button>().enabled == true)
                {
                    pauseButtons[i].GetComponent<BackButton>().InvokeButton();
                    break;
                }
            }
        }
    }

    // Recharge timer called after attacking; waits a specified amount of
    // time before allowing the bug to shoot again
    public virtual IEnumerator Recharge()
    {
        yield return new WaitForSeconds(cooldownTime);
        recharged = true;
        if (bugAnimator != null)
        {
            bugAnimator.SetBool("IsRecharged", true);
        }
        comboCounter = 0;
    }

    // If the recharge coroutine is running, stop it from doing so
    public void StopRecharge()
    {
        if (rechargeRoutine != null)
        {
            StopCoroutine(rechargeRoutine);
        }
    }

    // Called while the attack joystick is held down so that charge is built up
    public void ChargingUp(bool shooting)
    {
        // Prime the next shot to be fired
        primed = shooting;
        currentCharge += chargeRate * Time.deltaTime;
        // Maximum charge is 1
        if (currentCharge >= 1f)
        {
            currentCharge = 1f;
        }
        // Minimum charge is 0
        else if (currentCharge < 0f)
        {
            currentCharge = 0f;
        }
    }

    // Upon releasing the joystick, disable the variables that allow the user to
    // shoot and start the recharge timer
    public void Release()
    {
        //shoot = false;
        joystickDrawSaveStates[0] = joystickDraw;
        recharged = false;
        primed = false;
        bugAnimator.SetBool("IsRecharged", false);
        rechargeRoutine = StartCoroutine(Recharge());
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        rb.velocity = newVelocity;
    }

    /// <summary>----------------------------------------------------------
    /// Determines whether or not a point in space overlaps the ball based
    /// on the position of its center and the size of its sprite
    /// </summary>
    /// <param name="point">the point to be checked.</param>
    /// <returns>whether or not the point overlaps the ball.</returns>
    /// -------------------------------------------------------------------
    public int Damage(int damageTaken)
    {
        int damage = 0;
        if (invincible == false)
        {
            damage = Mathf.RoundToInt(damageTaken * (1f - armor));
            if (damage > health)
            {
                damage = health;
            }
            health -= damage;
            if (damage != 0)
            {
                // Update the health bar when hit
                DisplayHealthBar(maxHealth, health);
                // Create the damage display when hit
                CreateDisplay(damageDisplay, damage);
            }
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
        return damage;
    }

    // Implmentation for poison interface
    public void Poison(int damagePerInterval, float interval, int numIntervals)
    {
        if (poisoned == false)
        {
            poisoned = true;
            poisonRoutine = StartCoroutine(Poisoned(damagePerInterval, interval, numIntervals));
        }
    }

    // Implementation for shield interface
    public int Shield(int damageTaken)
    {
        int damage = 0;
        if (invincible == false)
        {
            damage = Mathf.RoundToInt(damageTaken * (1f - shieldArmor));
            if (damage > health)
            {
                damage = health;
            }
            health -= damage;
            // Update the health bar when hit
            DisplayHealthBar(maxHealth, health);
            // Create the shield display when hit
            CreateDisplay(shieldDisplay, damage);
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
        return damage;
    }

    // Implmentation for heal interface
    public virtual int Heal(int healAmount)
    {
        if (health != maxHealth)
        {
            if (healAmount + health > maxHealth)
            {
                healAmount = maxHealth - health;
            }
            health += healAmount;
            if (healAmount != 0)
            {
                // Update the health bar when healed
                DisplayHealthBar(maxHealth, health);
                // Create the heal display when hit
                CreateDisplay(healDisplay, healAmount);
            }
        }
        return healAmount;
    }

    // Implmentation for regenerate interface
    public void Regenerate(int healPerInterval, float interval, int numIntervals)
    {
        if (regenerating == false)
        {
            regenerating = true;
            poisonRoutine = StartCoroutine(Regen(healPerInterval, interval, numIntervals));
        }
    }

    // Implementation for wrap interface
    public void Wrap(RuntimeAnimatorController wrappedBug, float seconds)
    {
        bugAnimator.runtimeAnimatorController = wrappedBug;
        enableHitbox(false);
        shoot = false;
        primed = false;
        wrapped = true;
        joystickDraw = Vector2.zero;
        GetComponent<PlayerMovement>().movement = Vector2.zero;
        StartCoroutine(Wrapped(seconds));
    }

    protected IEnumerator Wrapped(float wrappedTime)
    {
        yield return new WaitForSeconds(wrappedTime);
        bugAnimator.runtimeAnimatorController = bugController;
        bugAnimator.SetBool("IsRecharged", recharged);
        enableHitbox(true);
        wrapped = false;
    }

    protected IEnumerator Poisoned(int damagePerInterval, float interval, int numIntervals)
    {
        yield return new WaitForSeconds(interval);
        int damage = damagePerInterval;
        if (damage > health)
        {
            damage = health;
        }
        health -= damage;
        if (damage != 0)
        {
            DisplayHealthBar(maxHealth, health);
            CreateDisplay(poisonDisplay, damage);
        }
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        if (numIntervals > 1)
        {
            poisonRoutine = StartCoroutine(Poisoned(damagePerInterval, interval, numIntervals - 1));
        }
        else
        {
            poisoned = false;
        }
    }

    protected IEnumerator Regen(int healPerInterval, float interval, int numIntervals)
    {
        yield return new WaitForSeconds(interval);
        int heal = healPerInterval;
        if (heal + health > maxHealth)
        {
            heal = maxHealth - health;
        }
        health += heal;
        if (heal != 0)
        {
            DisplayHealthBar(maxHealth, health);
            CreateDisplay(healDisplay, heal);
        }
        if (numIntervals > 1)
        {
            regenRoutine = StartCoroutine(Regen(healPerInterval, interval, numIntervals - 1));
        }
        else
        {
            regenerating = false;
        }
    }

    public void CurePoison()
    {
        StopCoroutine(poisonRoutine);
        poisoned = false;
    }

    public void CancelRegen()
    {
        StopCoroutine(regenRoutine);
        regenerating = false;
    }

    public void enableHitbox(bool active)
    {
        if (persistentHitbox != null)
        {
            persistentHitbox.SetActive(active);
        }
    }

    // Implmentation for invincibility frames interface
    public void InvincibilityFrames(float seconds)
    {
        if (invincible == false)
        {
            invincible = true;
            StartCoroutine(Flash(seconds, FLASH_INTERVAL));
            StartCoroutine(Invincible(seconds));
        }
    }

    // Invincibility timer
    protected IEnumerator Invincible(float invincibleTime)
    {
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Credit for Flash goes to user Heisenbug on StackExchange: https://stackoverflow.com/questions/16114349/make-player-flash-when-hit
    protected IEnumerator Flash(float time, float intervalTime)
    {
        float elapsedTime = 0f;
        int index = 0;
        while (elapsedTime < time)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, index % 2);
            elapsedTime += Time.deltaTime;
            index++;
            yield return new WaitForSeconds(intervalTime);
            elapsedTime += intervalTime;
        }
    }

    // Create and assemble a new damage/heal display object
    public void CreateDisplay(GameObject type, int amount)
    {
        GameObject display = Instantiate(type, gameObject.transform.position, Quaternion.identity);
        display.GetComponent<DamageDisplay>().damage = amount;
        display.GetComponent<DamageDisplay>().owner = gameObject;
    }

    // Display the health bar
    public void DisplayHealthBar(int maxHealth, int currentHealth)
    {
        healthBarCanvas.GetComponent<HealthBar>().UpdateHealthBar(maxHealth, currentHealth);
    }

    // Display the charge bar
    public void DisplayChargeBar(float charge)
    {
        healthBarCanvas.GetComponent<HealthBar>().UpdateChargeBar(charge);
    }

    public void Grapple(Vector2 grapplePoint)
    {
        grapple.enabled = true;
        grapplePos = grapplePoint;
        grapple.connectedAnchor = grapplePoint;
        distance = new Vector2(grapplePoint.x - gameObject.transform.position.x, grapplePoint.y - gameObject.transform.position.y);
        grapple.distance = distance.magnitude;
    }

    public void UpdateGrapple(Vector2 grapplePoint, GameObject web)
    {
        if (grapple.enabled == true && currentWeb == web)
        {
            grapplePos = grapplePoint;
            grapple.connectedAnchor = grapplePoint;
        }
    }

    public void Ungrapple()
    {
        grapple.enabled = false;
        Destroy(currentWeb);
    }

    void FixedUpdate()
    {
        // The player rotates in the direction that the joystick is pointed
        if (joystickDraw.magnitude != 0)
        {
            // The x and y position of the joystick are used to determine it's angle of rotation
            // This formula can be used to find the rotation of ANY VECTOR. Keep this in mind
            var angle = Mathf.Atan2(joystickDraw.x, joystickDraw.y) * Mathf.Rad2Deg * -1f;
            rb.rotation = angle;
        }
        DisplayChargeBar(currentCharge);
    }

    void OnDestroy()
    {
        if (defaultLayer == PLAYER_LAYER)
        {
            SharedData.currentPlayers -= 1;
        }
        else
        {
            SharedData.currentBots -= 1;
        }
        if (grapple != null)
        {
            Ungrapple();
        }
    }
}
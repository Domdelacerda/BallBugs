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

    void FixedUpdate()
    {
        if (joystickDraw.magnitude != 0)
        {
            var angle = Mathf.Atan2(joystickDraw.x, joystickDraw.y) 
                * Mathf.Rad2Deg * -1f;
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

    //-------------------------------------------------------------------------
    // INPUT ACTIONS
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
            GameObject[] pauseButtons = 
                GameObject.FindGameObjectsWithTag("Back");
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

    //-------------------------------------------------------------------------
    // INTERFACE IMPLEMENTATIONS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Removes health from this bug based on the specified damage value
    /// and its armor percentage. A damage display is created to let the
    /// player know how much damage was taken. If the bug loses all of its
    /// health, it will be destroyed.
    /// </summary>
    /// <param name="damageTaken">the amount of damage dealt to this bug
    /// from an attack or hazard.</param>
    /// <returns>the amount of actual health lost after the damage calculation
    /// is made.</returns>
    /// -----------------------------------------------------------------------
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
                DisplayHealthBar(maxHealth, health);
                CreateDisplay(damageDisplay, damage);
            }
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
        return damage;
    }

    /// <summary>--------------------------------------------------------------
    /// Poisons this bug for a certain amount of time, and deals damage over a 
    /// specified number of intervals. Only works as long as the bug is not
    /// already poisoned.
    /// </summary>
    /// <param name="damagePerInterval">the amount of damage dealt to this bug 
    /// per poison interval.</param>
    /// <param name="interval">the time between damage intervals.</param>
    /// <param name="numIntervals">the number of intervals damage is taken
    /// over.</param>
    /// -----------------------------------------------------------------------
    public void Poison(int damagePerInterval, float interval,
        int numIntervals)
    {
        if (poisoned == false)
        {
            poisoned = true;
            poisonRoutine = StartCoroutine(Poisoned(damagePerInterval,
                interval, numIntervals));
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Removes health from this bug based on the specified damage value and 
    /// its shield percentage. A shield display is created to let the player 
    /// know how much damage was taken. If the bug loses all of its health, it 
    /// will be destroyed.
    /// </summary>
    /// <param name="damageTaken">the amount of damage dealt to this bug from 
    /// an attack or hazard.</param>
    /// <returns>the amount of actual health lost after the damage calculation
    /// is made.</returns>
    /// -----------------------------------------------------------------------
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
            DisplayHealthBar(maxHealth, health);
            CreateDisplay(shieldDisplay, damage);
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
        return damage;
    }

    /// <summary>--------------------------------------------------------------
    /// Adds health to this bug based on the specified heal value. A heal
    /// display is created to let the player know how much health was regained.
    /// If the bug gains more health than its max health, it will be reset back
    /// to its max health.
    /// </summary>
    /// <param name="healAmount">the amount of healing applied to this bug
    /// </param>
    /// <returns>the amount of actual health regained after the calculation
    /// is made.</returns>
    /// -----------------------------------------------------------------------
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
                DisplayHealthBar(maxHealth, health);
                CreateDisplay(healDisplay, healAmount);
            }
        }
        return healAmount;
    }

    /// <summary>--------------------------------------------------------------
    /// Causes this bug to regenerate health for a certain amount of time and 
    /// over a specified number of intervals. Only works as long as the bug is
    /// not already regenerating.
    /// </summary>
    /// <param name="healPerInterval">the amount of healing applied to bug 
    /// per poison interval.</param>
    /// <param name="interval">the time between healing intervals.</param>
    /// <param name="numIntervals">the number of intervals health is regained 
    /// over.</param>
    /// -----------------------------------------------------------------------
    public void Regenerate(int healPerInterval, float interval,
        int numIntervals)
    {
        if (regenerating == false)
        {
            regenerating = true;
            poisonRoutine = StartCoroutine(Regen(healPerInterval, interval,
                numIntervals));
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Immobilizes this bug and prevents them from aiming, shooting, jumping,
    /// or crouching for a given amount of time. Temporarily sets their 
    /// animator to a wrapped state. 
    /// </summary>
    /// <param name="wrappedBug">the animator controller the bug is given to
    /// indicate that it is immobilized.</param>
    /// <param name="seconds">the time the bug stays immobilized.</param>
    /// -----------------------------------------------------------------------
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

    /// <summary>--------------------------------------------------------------
    /// Makes this bug invincible for a certain amount of time and displays
    /// flashing to indicate that the bug is still invincible.
    /// </summary>
    /// <param name="seconds">the time the bug stays invincible.</param>
    /// -----------------------------------------------------------------------
    public void InvincibilityFrames(float seconds)
    {
        if (invincible == false)
        {
            invincible = true;
            StartCoroutine(Flash(seconds, FLASH_INTERVAL));
            StartCoroutine(Invincible(seconds));
        }
    }

    //-------------------------------------------------------------------------
    // COROUTINES
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Recharge the bug's attack, allowing them to attack again after a
    /// specified cooldown time has passed.
    /// </summary>
    /// <returns>coroutine that executes recharge event.</returns>
    /// -----------------------------------------------------------------------
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

    /// <summary>--------------------------------------------------------------
    /// Poisons this bug for a certain amount of time, and deals damage over a 
    /// specified number of intervals.
    /// </summary>
    /// <param name="damagePerInterval">the amount of damage dealt to this bug 
    /// per poison interval.</param>
    /// <param name="interval">the time between damage intervals.</param>
    /// <param name="numIntervals">the number of intervals damage is taken
    /// over.</param>
    /// <returns>coroutine that executes poison damage event.</returns>
    /// -----------------------------------------------------------------------
    protected IEnumerator Poisoned(int damagePerInterval, float interval, 
        int numIntervals)
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
            poisonRoutine = StartCoroutine(Poisoned(damagePerInterval, 
                interval, numIntervals - 1));
        }
        else
        {
            poisoned = false;
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Causes this bug to regenerate health for a certain amount of time and 
    /// over a specified number of intervals.
    /// </summary>
    /// <param name="healPerInterval">the amount of healing applied to this 
    /// bug per regeneration interval.</param>
    /// <param name="interval">the time between healing intervals.</param>
    /// <param name="numIntervals">the number of intervals health is regained
    /// over.</param>
    /// <returns>coroutine that executes health regeneration event.</returns>
    /// -----------------------------------------------------------------------
    protected IEnumerator Regen(int healPerInterval, float interval, 
        int numIntervals)
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
            regenRoutine = StartCoroutine(Regen(healPerInterval, interval, 
                numIntervals - 1));
        }
        else
        {
            regenerating = false;
        }
    }

    /// <summary>--------------------------------------------------------------
    /// After a certain amount of time being wrapped, the bug is released. The
    /// bug regains full movement control and their animator is reset back to
    /// its default animator.
    /// </summary>
    /// <param name="wrappedTime">the amount of time the bug stays wrapped for.
    /// </param>
    /// <returns>coroutine that executes the unwrap event.</returns>
    /// -----------------------------------------------------------------------
    protected IEnumerator Wrapped(float wrappedTime)
    {
        yield return new WaitForSeconds(wrappedTime);
        bugAnimator.runtimeAnimatorController = bugController;
        bugAnimator.SetBool("IsRecharged", recharged);
        enableHitbox(true);
        wrapped = false;
    }

    /// <summary>--------------------------------------------------------------
    /// After a certain amount of time being invincible, the bug is made
    /// vulnerable again and the flashing effect stops.
    /// </summary>
    /// <param name="invincibleTime">the amount of time the bug stays
    /// invincible for.</param>
    /// <returns>coroutine that executes the vulnerability event.</returns>
    /// -----------------------------------------------------------------------
    protected IEnumerator Invincible(float invincibleTime)
    {
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    /// <summary>--------------------------------------------------------------
    /// Flash this bug's sprite on and off after each interval passes for a
    /// specified amount of time.
    /// </summary>
    /// <param name="time">the total amount of time the bug flashes for.
    /// </param>
    /// <param name="intervalTime">the interval between when the bug flashes on
    /// and off.</param>
    /// <returns>coroutine that executes the vulnerability event.</returns>
    /// -----------------------------------------------------------------------
    protected IEnumerator Flash(float time, float intervalTime)
    {
        float elapsedTime = 0f;
        int index = 0;
        while (elapsedTime < time)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 
                255, 255, index % 2);
            elapsedTime += Time.deltaTime;
            index++;
            yield return new WaitForSeconds(intervalTime);
            elapsedTime += intervalTime;
        }
    }

    //-------------------------------------------------------------------------
    // PROGRAMMER-WRITTEN METHODS
    //-------------------------------------------------------------------------

    /// <summary>--------------------------------------------------------------
    /// Charge up the bug's next attack based on its charge rate, bounded below
    /// by zero and bounded above by one.
    /// </summary>
    /// <param name="shooting">indicates whether the bug is currently charging
    /// an attack or not.</param>
    /// -----------------------------------------------------------------------
    public void ChargingUp(bool shooting)
    {
        primed = shooting;
        if (currentCharge < 0f)
        {
            currentCharge = 0f;
        }
        if (currentCharge < 1f)
        {
            currentCharge += chargeRate * Time.deltaTime;
            if (currentCharge >= 1f)
            {
                currentCharge = 1f;
            }
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Cancel the recharge coroutine if it is currently running.
    /// </summary>-------------------------------------------------------------
    public void StopRecharge()
    {
        if (rechargeRoutine != null)
        {
            StopCoroutine(rechargeRoutine);
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Release the shot currently being charged, which entails setting the
    /// bug's animator to its uncharged state and starting the recharge 
    /// coroutine.
    /// </summary>-------------------------------------------------------------
    public void Release()
    {
        joystickDrawSaveStates[0] = joystickDraw;
        recharged = false;
        primed = false;
        bugAnimator.SetBool("IsRecharged", false);
        rechargeRoutine = StartCoroutine(Recharge());
    }

    /// <summary>--------------------------------------------------------------
    /// Sets this bug's velocity to a specified speed and direction
    /// </summary>
    /// <param name="newVelocity">the new velocity the bug is given.</param>
    /// -----------------------------------------------------------------------
    public void SetVelocity(Vector2 newVelocity)
    {
        rb.velocity = newVelocity;
    }

    /// <summary>--------------------------------------------------------------
    /// Removes the current poison status effect afflicting this bug.
    /// </summary>-------------------------------------------------------------
    public void CurePoison()
    {
        if (poisonRoutine != null)
        {
            StopCoroutine(poisonRoutine);
        }
        poisoned = false;
    }

    /// <summary>--------------------------------------------------------------
    /// Removes the current regeneration status effect afflicting this bug.
    /// </summary>-------------------------------------------------------------
    public void CancelRegen()
    {
        if (regenRoutine != null)
        {
            StopCoroutine(regenRoutine);
        }
        regenerating = false;
    }

    /// <summary>--------------------------------------------------------------
    /// Enables or disables this bug's persistent hitbox, if it exists. 
    /// Persistent hitboxes can include fangs, mandibles, or shields.
    /// </summary>
    /// <param name="active">whether the hotbox should be enabled or disabled.
    /// -----------------------------------------------------------------------
    public void enableHitbox(bool active)
    {
        if (persistentHitbox != null)
        {
            persistentHitbox.SetActive(active);
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Creates a new display of a specified type which displays the provided
    /// amount. Currently implemented displays include damage, heal, shield,
    /// and poison.
    /// </summary>
    /// <param name="type">the type of display to be created.</param>
    /// <param name="amount">the amount to be displayed.</param>
    /// -----------------------------------------------------------------------
    public void CreateDisplay(GameObject type, int amount)
    {
        GameObject display = Instantiate(type, gameObject.transform.position, 
            Quaternion.identity);
        DamageDisplay script = display.GetComponent<DamageDisplay>();
        script.damage = amount;
        script.owner = gameObject;
    }

    /// <summary>--------------------------------------------------------------
    /// Updates the bug's health bar based on the ratio of its current health
    /// to its max health.
    /// </summary>
    /// <param name="maxHealth">the bug's maximum health.</param>
    /// <param name="currentHealth">the bug's current health.</param>
    /// -----------------------------------------------------------------------
    public void DisplayHealthBar(int maxHealth, int currentHealth)
    {
        healthBarCanvas.GetComponent<HealthBar>().UpdateHealthBar(maxHealth, 
            currentHealth);
    }

    /// <summary>--------------------------------------------------------------
    /// Updates the bug's charge bar based on the ratio of its current charge
    /// to the maximum charge value, one.
    /// </summary>
    /// <param name="charge">the bug's current charge.</param>
    /// -----------------------------------------------------------------------
    public void DisplayChargeBar(float charge)
    {
        healthBarCanvas.GetComponent<HealthBar>().UpdateChargeBar(charge);
    }

    /// <summary>--------------------------------------------------------------
    /// Grapples this bug to a specified point on the map
    /// </summary>
    /// <param name="grapplePoint">the point in space that the bug is grappled
    /// to.</param>
    /// -----------------------------------------------------------------------
    public void Grapple(Vector2 grapplePoint)
    {
        grapple.enabled = true;
        grapplePos = grapplePoint;
        grapple.connectedAnchor = grapplePoint;
        distance = new Vector2(grapplePoint.x - 
            gameObject.transform.position.x, grapplePoint.y - 
            gameObject.transform.position.y);
        grapple.distance = distance.magnitude;
    }

    /// <summary>--------------------------------------------------------------
    /// Updates the current grapple point based on a new position
    /// </summary>
    /// <param name="grapplePoint">the point in space that the bug is going to
    /// be grappled to.</param>
    /// <param name="web">the web object that the other end of the grapple is
    /// anchored to.</param>
    /// -----------------------------------------------------------------------
    public void UpdateGrapple(Vector2 grapplePoint, GameObject web)
    {
        if (grapple.enabled == true && currentWeb == web)
        {
            grapplePos = grapplePoint;
            grapple.connectedAnchor = grapplePoint;
        }
    }

    /// <summary>--------------------------------------------------------------
    /// Ungrapples this bug, removing the tether keeping it grappled and the
    /// web object it was attached to.
    /// </summary>-------------------------------------------------------------
    public void Ungrapple()
    {
        if (grapple != null)
        {
            grapple.enabled = false;
        }
        Destroy(currentWeb);
    }
}
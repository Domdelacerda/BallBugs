using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEditor;
using Unity.VisualScripting;

// Bug is a generic class for all bug classes, containing attributes and methods
// common among all bugs
public class Bug : MonoBehaviour, IDamageable, IPoisonable, IShieldable, IHealable, IRegenerable, IFrames, IWrappable
{
    // Toggle for slingshot-type shooting controls
    public bool slingshotControls = true;

    // Animator controller for the bug
    public Animator bugAnimator;
    // The Animator Controller that determines the bug's default animations
    public RuntimeAnimatorController bugController;

    // Bool variable used to determine if the user drew back the joystick to
    // attack
    public bool shoot = false;
    // Bool variable used to determine if the bug's attack has recharged or
    // is still on cooldown
    public bool recharged = false;
    // Bool variable used to determine if the player can be damaged or not
    public bool invincible = false;
    // Bool variable that tells us whether the user was just aiming or not
    public bool primed = false;
    // Bool variable that tells us whether the user is charging up a jump or not
    protected bool chargingJump = false;
    // Bool variable that tells us whether the user is able to jump or not
    protected bool jumpReady = false;
    // Bool variable that tells us whether the user fired a shot with slingshot controls
    // or manual controls
    public bool slingshotMode = false;
    // Whether the bug is currently wrapped (immobilized) or not
    public bool wrapped = false;
    // Whether the bug is currently poisoned or not
    public bool poisoned = false;
    // Whether the bug is currently regenerating health or not
    public bool regenerating = false;

    // The maximum amount to health the bug can have
    public int maxHealth = 100;
    // The amount of health the bug currently has
    public int health = 100;
    // The amount of damage reduction the bug recives when taking damage
    public float armor = 0f;
    // The amount of damage reduction the bug recieves when their shield is hit
    public float shieldArmor = 0f;

    // The bug's current score for scored game modes
    public int score = 0;
    
    // Float variable used to determine the amount of time it takes for the
    // bug's attack to recharge after attacking
    public float cooldownTime = 3f;
    // Float varaible used for keeping track of how long the user has held
    // back the attack joystick, which changes the attack's properties
    public float currentCharge = 0f;
    // Float variable used for incrementing current charge each frame that
    // both the shoot joystick is held down and recharged is true
    public float chargeRate = 0.0008f;

    // The combo counter for consecutive shots
    public int comboCounter = 0;

    // Rigidbody attached to the bug
    public Rigidbody2D rb;

    // The healthbar used for displaying the bug's health and current charge
    public GameObject healthBarCanvas;
    // The text used for displaying the bug's score
    public TextMeshProUGUI scoreText;
    // The damage display prefab
    public GameObject damageDisplay;
    // The heal display prefab
    public GameObject healDisplay;
    // The shield display prefab
    public GameObject shieldDisplay;
    // The poison display prefab
    public GameObject poisonDisplay;
    // The secondary damage-dealing hitbox
    public GameObject persistentHitbox;

    // The flashing speed of the invincibility frames
    private const float flashInterval = 0.075f;

    // How far back the joystick is drawn
    public Vector2 joystickDraw;
    // Save states for the joystick's last positions, up to 3 frames earlier
    public Vector2[] joystickDrawSaveStates = new Vector2[3];

    // The playerLayer int represents the layer that all player characters exist on
    protected const int playerLayer = 9;
    // The enemy layer int represents the layer that all enemy characters exist on
    protected const int enemyLayer = 10;
    // The shield layer int represents the layer that all shields exist on
    protected const int shieldLayer = 11;
    // The defualt layer the bug is on at the start of the game
    public int defaultLayer;

    // The spring joint used for grappling onto walls
    public SpringJoint2D grapple;
    // The point where the current web is connected to
    public Vector2 grapplePos;
    // The current web the spider is swinging from
    public GameObject currentWeb;
    // Distance between the bug and the current web
    protected Vector2 distance;

    protected Coroutine rechargeRoutine;
    protected Coroutine poisonRoutine;
    protected Coroutine regenRoutine;

    // Start is called on the first frame
    void Start()
    {
        defaultLayer = gameObject.layer;
        DisplayHealthBar(maxHealth, health);
        rechargeRoutine = StartCoroutine(Recharge());
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (wrapped == false)
        {
            joystickDraw = ctx.ReadValue<Vector2>();
        }
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && wrapped == false)
        {
            shoot = true;
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Ungrapple();
        }
        chargingJump = ctx.performed;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Ungrapple();
        }
    }

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

    // Implmentation for damage interface
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
            StartCoroutine(Flash(seconds, flashInterval));
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
        if (defaultLayer == playerLayer)
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flytrap : MonoBehaviour
{
    // The maximum amount of times a character or projectile can touch
    // the flytrap before it snaps shut
    private const int strikes = 3;

    // The tally that keeps track of how many strikes are left before
    // the flytrap snaps shut
    int strikeCounter = strikes;

    // A boolean used to keep track of whether the trap is closed or not
    // This helps make sure that coroutines don't keep restarting every
    // Time a collision happens while the jaw is closed
    private bool closed = false;

    // The animator for the upper jaw of the flytrap
    public Animator upperJawAnimator;

    // The trigger collider of the upper jaw, used for determining if
    // Objects are inside of the trap at the time of closing
    public BoxCollider2D upperJawTrigger;

    // The sprite renderer for the flytrap's upper jaw
    public SpriteRenderer upperJawSpriteRenderer;

    // The sprite for when the flytrap has 3 strikes remaining
    public Sprite ThreeStrikesSprite;

    // The sprite for when the flytrap has 2 strikes remaining
    public Sprite TwoStrikesSprite;

    // The sprite for when the flytrap has 1 strike remaining
    public Sprite OneStrikeSprite;

    // The sprite renderer for the flytrap's lower jaw
    public SpriteRenderer lowerJawSpriteRenderer;

    // The sprite for when the flytrap's jaw is open
    public Sprite lowerJawOpenSprite;

    // The sprite for when the flytrap's jaw is closed
    public Sprite lowerJawClosedSprite;

    // The time it takes for the flytrap closing animation to play out
    private const float animationDelay = 0.33f;

    // The time it takes for the flytrap closing animation to play out
    private const float colliderDelay = 0.15f;

    // The amount of time that the flytrap remains shut
    public float cooldownTime = 5.0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        strikeCounter--;
        if (strikeCounter <= 0 && closed == false)
        {
            closed = true;
            upperJawAnimator.enabled = true;
            upperJawAnimator.SetBool("Closed", true);
            StartCoroutine(ColliderActivate());
            StartCoroutine(ClampDown());
        }
    }

    IEnumerator ColliderActivate()
    {
        yield return new WaitForSeconds(colliderDelay);
        upperJawTrigger.enabled = true;
    }

    IEnumerator ClampDown()
    {
        yield return new WaitForSeconds(animationDelay);
        lowerJawSpriteRenderer.sprite = lowerJawClosedSprite;
        upperJawTrigger.enabled = false;
        StartCoroutine(Reopen());
    }

    IEnumerator Reopen()
    {
        yield return new WaitForSeconds(cooldownTime);
        lowerJawSpriteRenderer.sprite = lowerJawOpenSprite;
        closed = false;
        upperJawAnimator.SetBool("Closed", false);
        strikeCounter = strikes;
        StartCoroutine(DisplaySprite());
    }

    IEnumerator DisplaySprite()
    {
        yield return new WaitForSeconds(animationDelay);
        upperJawAnimator.enabled = false;
    }

    void Start()
    {
        StartCoroutine(DisplaySprite());
    }

    void Update()
    {
        if (strikeCounter == 3)
        {
            upperJawSpriteRenderer.sprite = ThreeStrikesSprite;
        }
        else if (strikeCounter == 2)
        {
            upperJawSpriteRenderer.sprite = TwoStrikesSprite;
        }
        else if (strikeCounter == 1)
        {
            upperJawSpriteRenderer.sprite = OneStrikeSprite;
        }
        // Debug.Log(strikeCounter);
    }
}

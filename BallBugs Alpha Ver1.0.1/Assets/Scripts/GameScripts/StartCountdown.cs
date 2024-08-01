using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCountdown : MonoBehaviour
{
    // The different sprites used for each stage of the countdown
    public Sprite threeCountSprite;
    public Sprite twoCountSprite;
    public Sprite oneCountSprite;

    // Game object that will start the timer clock
    public GameObject timer;

    // The sprite renderer for the sign. Could have used GetComponent since this script 
    // Is a component of the sign, but I would have to use it too many times and getting
    // It each time would slow down performance
    public SpriteRenderer signSpriteRenderer;

    // Animator for the sign. Only plays the animation for go, so leave it disabled until then
    public Animator signAnimator;

    // Start is called before the first frame update
    void Start()
    {
        signSpriteRenderer.enabled = true;
        signSpriteRenderer.sprite = threeCountSprite;
        StartCoroutine(TwoCount());
    }

    // Change the countdown to 2 after 1 second
    IEnumerator TwoCount()
    {
        yield return new WaitForSeconds(1);
        signSpriteRenderer.sprite = twoCountSprite;
        StartCoroutine(OneCount());
    }

    // Change the countdown to 1 after 1 second
    IEnumerator OneCount()
    {
        yield return new WaitForSeconds(1);
        signSpriteRenderer.sprite = oneCountSprite;
        StartCoroutine(Go());
    }

    // Change the countdown to say "Go!!!!" after 1 second
    IEnumerator Go()
    {
        yield return new WaitForSeconds(1);
        signAnimator.enabled = true;
        StartCoroutine(DisableSprite());
        timer.GetComponent<Timer>().StartCounter();
    }

    // Disable the sprite renderer and animator after 1 second
    IEnumerator DisableSprite()
    {
        yield return new WaitForSeconds(1);
        signSpriteRenderer.enabled = false;
        signAnimator.enabled = false;
    }
}

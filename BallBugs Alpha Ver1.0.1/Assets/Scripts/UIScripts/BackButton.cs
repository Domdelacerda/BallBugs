using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            InvokeButton();
        }
    }

    public void InvokeButton()
    {
        gameObject.GetComponent<Button>().onClick.Invoke();
    }
}

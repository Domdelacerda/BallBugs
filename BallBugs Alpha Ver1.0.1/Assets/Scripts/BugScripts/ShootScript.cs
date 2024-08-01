using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootScript : MonoBehaviour
{
    public Joystick shootJoystick;

    public float shootSpeed = 5f;

    public Transform firePoint;
    public GameObject bulletPrefab;

    public Rigidbody2D rb;

    Vector2 shooting;

    // Update is called once per frame
    void Update()
    {
        shooting.x = shootJoystick.Horizontal;
        shooting.y = shootJoystick.Vertical;
        if (Input.GetKeyDown("a"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Shooting logic
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    void FixedUpdate()
    {
        var angle = Mathf.Atan2(shootJoystick.Horizontal, shootJoystick.Vertical) * Mathf.Rad2Deg * -1f;
        Debug.Log(angle);
        rb.rotation = angle;
    }
}

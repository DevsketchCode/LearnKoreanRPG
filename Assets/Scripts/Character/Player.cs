using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] spriteArray;
    public float movementSpeed = 0.15f;

    public Rigidbody2D rb;
    public Animator animator;
    Vector2 movement;

    public Joystick joystick;
    public bool useJoystick;
    public float joystickDeadZone = 0.1f; // Adjust this value as needed

    private void Start()
    {
        joystick = FindFirstObjectByType<Joystick>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (useJoystick)
        {
            Vector2 joystickInput;
            joystickInput.x = joystick.Horizontal;
            joystickInput.y = joystick.Vertical;

            // Apply dead zone
            if (joystickInput.magnitude < joystickDeadZone)
            {
                joystickInput = Vector2.zero; // Set to zero if within dead zone
            }

            movement = joystickInput * movementSpeed; // Apply movement speed *after* dead zone
            //movement.x = joystick.Horizontal * movementSpeed;
            //movement.y = joystick.Vertical * movementSpeed;
        } else
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement *= movementSpeed; // Apply movement speed after getting input *only for keyboard*
            //movement.x = Input.GetAxisRaw("Horizontal");
            //movement.y = Input.GetAxisRaw("Vertical");
        }

        // Swap sprite direction, using animator
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

    }

    // FixedUdpate is called 50 times a second by default
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);
    }
}

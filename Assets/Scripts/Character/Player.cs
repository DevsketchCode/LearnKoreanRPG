using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] spriteArray;
    public float movementSpeed = 0.15f;
    public UIManager uiManager;

    public Rigidbody2D rb;
    public Animator animator;
    Vector2 movement;

    public Joystick joystick;
    public bool useJoystick;
    public float joystickDeadZone = 0.1f; // Adjust this value as needed
    public bool canMove = true; // Flag to control movement

    private float lastSpawnTime;
    private float spawnImmunityDuration = 1.0f; // 1 second of portal immunity

    private void Start()
    {
        joystick = FindFirstObjectByType<Joystick>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return; // Early exit if movement is disabled

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
        } else
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement *= movementSpeed; // Apply movement speed after getting input *only for keyboard*
        }

        // Swap sprite direction, using animator
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
        
    }

    // FixedUdpate is called 50 times a second by default
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementSpeed * Time.fixedDeltaTime * movement);
    }

    public void DisableMovement()
    {
        canMove = false;
        movement = Vector2.zero; // Stop the users movement (walking animation still continues)
        Debug.Log("[PlayerMovement] Movement DISABLED");
    }

    public void EnableMovement()
    {
        canMove = true;
        Debug.Log("[PlayerMovement] Movement ENABLED");
    }

    public void OnSpawn()
    {
        lastSpawnTime = Time.time;
        Debug.Log("Player spawned/repositioned. Immunity active.");
    }
    public bool CanTransition()
    {
        // Returns true only if 1 second has passed since OnSpawn was called
        return Time.time > lastSpawnTime + spawnImmunityDuration;
    }
}

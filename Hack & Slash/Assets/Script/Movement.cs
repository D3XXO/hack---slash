using UnityEngine;

public class TopDownPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Input Settings")]
    public KeyCode dashKey = KeyCode.LeftShift;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 dashDirection;

    // Dash 
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true; // This is the key line for 2D
        }
        else
        {
            rb.freezeRotation = true; // Ensure rotation is frozen
        }
    }

    void Update()
    {
   
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // dash input and cooldown
        if (Input.GetKeyDown(dashKey) && dashCooldownLeft <= 0f && !isDashing && movement != Vector2.zero)
        {
            StartDash();
        }

        //  dash timers
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
            {
                EndDash();
            }
        }

        if (dashCooldownLeft > 0f)
        {
            dashCooldownLeft -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            //  dash velocity
            rb.velocity = dashDirection * dashSpeed;
        }
        else
        {
            // normal movement
            rb.velocity = movement * moveSpeed;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownLeft = dashCooldown;
        dashDirection = movement;
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero; // Stop immediately after dash
    }


    public bool IsDashing()
    {
        return isDashing;
    }

    public float GetDashCooldownPercentage()
    {
        return 1f - (dashCooldownLeft / dashCooldown);
    }
}
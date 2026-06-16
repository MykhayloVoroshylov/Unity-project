//PlayerSprint.cs

using UnityEngine;

public class PlayerSprint : MonoBehaviour
{
    public Animator anim; 
    public PlayerMovement movementScript; 
    
    [Header("Sprint Settings")]
    public float sprintSpeedMultiplier = 1.5f;
    public float maxSprintDuration = 3f;
    
    [Header("Stamina State")]
    public float currentSprintTime;
    public bool isSprinting = false;
    private bool isExhausted = false; // Cooldown lock flag
    private float originalSpeed;

    void Start()
    {
        // Try to auto-grab scripts if they weren't dragged in the inspector
        if (movementScript == null) movementScript = GetComponent<PlayerMovement>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (movementScript != null)
        {
            originalSpeed = movementScript.speed;
        }
        currentSprintTime = maxSprintDuration;
    }

    void Update()
    {
        if (movementScript == null || anim == null) return;

        // 1. Core Sprint Input Check
        // Added a strict check: you MUST be pressing a movement key to sprint/roll!
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (Input.GetKey(KeyCode.LeftShift) && currentSprintTime > 0 && !isExhausted && isMoving)
        {
            if (!isSprinting) StartSprinting();
        }
        else
        {
            if (isSprinting) StopSprinting();
        }

        // 2. Manage Stamina Depletion / Regeneration
        if (isSprinting)
        {
            currentSprintTime -= Time.deltaTime;
            if (currentSprintTime <= 0)
            {
                currentSprintTime = 0;
                isExhausted = true; 
                Debug.Log("Stamina fully depleted! Entering exhausted cooldown.");
                StopSprinting(); // FORCE immediate mechanical and visual shutdown!
            }
        }
        else
        {
            if (currentSprintTime < maxSprintDuration)
            {
                currentSprintTime += Time.deltaTime * 0.75f; 
                
                // Remove exhaustion lock once stamina recovers to 40%
                if (isExhausted && currentSprintTime >= (maxSprintDuration * 0.4f))
                {
                    isExhausted = false;
                    Debug.Log("Stamina recovered enough. Sprinting unlocked.");
                }
            }
        }
    }

    void StartSprinting()
    {
        isSprinting = true;
        movementScript.speed = originalSpeed * sprintSpeedMultiplier;
        
        anim.SetBool("isRolling", true); 
        anim.SetTrigger("StartSprint");
        Debug.Log("Sprinting Started! Current Speed: " + movementScript.speed);
        Debug.Log("Animator Bool set to: " + anim.GetBool("isRolling"));
    }

    void StopSprinting()
    {
        isSprinting = false;
        movementScript.speed = originalSpeed;
        
        anim.SetBool("isRolling", false);
        anim.SetTrigger("StopSprint");
        Debug.Log("Sprinting Stopped. Restored Speed: " + movementScript.speed);
    }
}
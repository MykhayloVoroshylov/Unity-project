using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// ==========================================
// 1. PLAYER HEALTH
// ==========================================
public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;
    
    // Server-authoritative health synced automatically to everyone
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    [Header("UI Reference")]
    public Image healthBarFill; 

    public override void OnNetworkSpawn()
    {
        if (IsServer) currentHealth.Value = maxHealth;
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar(currentHealth.Value);
    }

    private void OnHealthChanged(int oldVal, int newVal)
    {
        UpdateHealthBar(newVal);
        if (newVal <= 0) Die();
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return; // Only server handles calculations
        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
    }

    void UpdateHealthBar(int health)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)health / (float)maxHealth;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // Add multiplayer respawn or spectator logic here
    }
}

// ==========================================
// 2. PLAYER MOVEMENT & SPRINTING
// ==========================================
public class PlayerMovement : NetworkBehaviour
{
    public float speed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    public float sprintSpeedMultiplier = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    private float originalSpeed;
    private bool isSprinting;
    private Animator anim;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        originalSpeed = speed;
    }

    void Update()
    {
        // CRITICAL MULTIPLAYER FIX: Only execute input if this player belongs to THIS computer
        if (!IsOwner) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Basic Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Sprinting Implementation
        if (Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f && !isSprinting) StartSprinting();
        if ((Input.GetKeyUp(KeyCode.LeftShift) || move.magnitude <= 0.1f) && isSprinting) StopSprinting();

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void StartSprinting()
    {
        isSprinting = true;
        speed = originalSpeed * sprintSpeedMultiplier;
        if(anim != null) {
            anim.SetBool("isRolling", true); 
            anim.SetTrigger("StartSprint");
        }
    }

    void StopSprinting()
    {
        isSprinting = false;
        speed = originalSpeed;
        if(anim != null) {
            anim.SetBool("isRolling", false);
            anim.SetTrigger("StopSprint");
        }
    }
}

// ==========================================
// 3. MOUSE LOOK
// ==========================================
public class MouseLook : NetworkBehaviour
{
    public Transform playerBody; 
    public float mouseSensitivity = 200f;
    private float xRotation = 0f;
    private Camera localCamera;

    void Start()
    {
        localCamera = GetComponentInChildren<Camera>();
        
        if (!IsOwner)
        {
            // Turn off other players' cameras on your screen so split views don't glitch
            if (localCamera != null) localCamera.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (localCamera != null) localCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null) playerBody.Rotate(Vector3.up * mouseX);
    }
}
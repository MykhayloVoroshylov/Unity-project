using UnityEngine;
using Unity.Netcode; // Ready for Netcode later

public class PlayerMovement : NetworkBehaviour 
{
    public float speed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 2f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Animation")]
    public Animator armadilloAnimator; 

    void Start() 
    {
        controller = GetComponent<CharacterController>();
        // Fallback to automatically find the animator if childed
        if (armadilloAnimator == null)
        {
            armadilloAnimator = GetComponentInChildren<Animator>();
        }
    }

    void Update() 
    {
        // CRITICAL: Bypasses execution if this isn't the local player instance
        if (!IsOwner) return; 

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f;
        }

        // 1. Core Movement Inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded) 
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 2. Clear, explicit input tracking for your Armadillo animations
        if (armadilloAnimator != null) 
        {
            bool isMoving = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D));
            
            // Set float magnitude or trigger transitions
            float horizontalSpeed = new Vector3(move.x, 0, move.z).magnitude * speed;
            armadilloAnimator.SetFloat("MoveSpeed", isMoving ? horizontalSpeed : 0f);
        }
    }
}

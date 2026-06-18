using Unity.Netcode;
using UnityEngine;

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
        if (!IsOwner) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f && !isSprinting) StartSprinting();
        if ((Input.GetKeyUp(KeyCode.LeftShift) || move.magnitude <= 0.1f) && isSprinting) StopSprinting();

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void StartSprinting()
    {
        isSprinting = true;
        speed = originalSpeed * sprintSpeedMultiplier;
        if (anim != null)
        {
            anim.SetBool("isRolling", true);
            anim.SetTrigger("StartSprint");
        }
    }

    void StopSprinting()
    {
        isSprinting = false;
        speed = originalSpeed;
        if (anim != null)
        {
            anim.SetBool("isRolling", false);
            anim.SetTrigger("StopSprint");
        }
    }
}

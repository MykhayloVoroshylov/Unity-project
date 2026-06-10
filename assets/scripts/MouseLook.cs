using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody; // armadillo root
    public float mouseSensitivity = 200f;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // LEFT / RIGHT → rotate BODY
        playerBody.Rotate(Vector3.up * mouseX);

        // UP / DOWN → rotate CAMERA PIVOT
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
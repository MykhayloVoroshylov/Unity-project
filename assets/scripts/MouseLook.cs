using Unity.Netcode;
using UnityEngine;

public class MouseLook : NetworkBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 200f;

    private float xRotation;
    private Camera localCamera;

    void Awake()
    {
        localCamera = GetComponent<Camera>();
        if (localCamera == null) localCamera = GetComponentInChildren<Camera>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            if (localCamera != null) localCamera.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (localCamera != null) localCamera.enabled = false;

            AudioListener listener = GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (localCamera != null)
            localCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}

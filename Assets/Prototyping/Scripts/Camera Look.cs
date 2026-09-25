using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    public float mouseXSensitivity = 100f;
    public float mouseYSensitivity = 100f;
    public Transform playerBody;

    private InputAction lookInput;
    private float xRotation = 0f;
    private Camera cam;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponent<Camera>();

        lookInput = InputSystem.actions.FindAction("Look");
    }
    private void Update()
    {
        HandleRotation(cam.transform);
    }
    public void HandleRotation(Transform camera)
    {
        float mouseX = lookInput.ReadValue<Vector2>().x * mouseXSensitivity * Time.deltaTime;
        float mouseY = lookInput.ReadValue<Vector2>().y * mouseYSensitivity * Time.deltaTime;


        // Locks vertical rotation between stated values
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60, 60);

        camera.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Horizontal player rotation
        playerBody.Rotate(Vector3.up * mouseX);

    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    public float mouseXSensitivity = 100f;
    public float mouseYSensitivity = 100f;
    public Transform orientation;

    private InputAction lookInput;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private InputAction escInput;
    public GameObject pauseMenu;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        lookInput = InputSystem.actions.FindAction("Look");
        escInput = InputSystem.actions.FindAction("Quit Game");

        escInput.performed += OnEscape;
    }
    private void OnDisable()
    {
        escInput.performed -= OnEscape;
    }
    private void Update()
    {
        HandleRotation();
    }
    public void HandleRotation()
    {
        float mouseX = lookInput.ReadValue<Vector2>().x * mouseXSensitivity * Time.deltaTime;
        float mouseY = lookInput.ReadValue<Vector2>().y * mouseYSensitivity * Time.deltaTime;


        // Locks vertical rotation between stated values
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85, 90);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Horizontal player rotation
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

    }
    public void QuitGame()
    {
        pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnEscape(InputAction.CallbackContext context) => QuitGame();
}

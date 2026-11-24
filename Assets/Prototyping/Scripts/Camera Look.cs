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

    public bool isActive { get; private set; }
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
    private void LateUpdate()
    {
        HandleRotation();
    }
    public void HandleRotation()
    {
        if (!isActive) return;

        float mouseX = lookInput.ReadValue<Vector2>().x * mouseXSensitivity * Time.deltaTime;
        float mouseY = lookInput.ReadValue<Vector2>().y * mouseYSensitivity * Time.deltaTime;


        // Locks vertical rotation between stated values
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85, 90);

        yRotation += mouseX;

        SetRotation(new Vector3(xRotation, yRotation, 0f));
    }
    public void SetRotation(Vector3 rot)
    {
        // Camera vertical and horizontal rotation
        transform.localRotation = Quaternion.Euler(rot.x, rot.y, 0f);

        // Horizontal player rotation
        orientation.rotation = Quaternion.Euler(0f, rot.y, 0f);
    }
    public void PauseGame()
    {
        if (isActive)
        {
            pauseMenu.SetActive(true);
            SetActiveCam(false);
        }
        else
        {
            pauseMenu.SetActive(false);
            SetActiveCam(true);
        }
        
    }
    public void ResumeGame()
    {
        SetActiveCam(true);
    }
    public void SetActiveCam(bool active)
    {
        isActive = active;
        if (isActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnEscape(InputAction.CallbackContext context) => PauseGame();
}

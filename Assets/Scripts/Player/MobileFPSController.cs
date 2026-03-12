using UnityEngine;
using UnityEngine.InputSystem;

public class MobileFPSController : MonoBehaviour
{
    public bool IsWalking { get; private set; }
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;

    [SerializeField] private CameraBobbing cameraBobbing;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction lookAction;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float cameraSensitivity = 1.0f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Desktop settings")] [SerializeField]
    private float mouseSensitivityMultiplier = 100f;

    private float cameraPitch;
    private Vector3 playerVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cameraBobbing == null)
        cameraBobbing = GetComponent<CameraBobbing>();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }

    void Update()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        /* if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } */
        if (SettingsManager.Instance != null)
        {
            cameraSensitivity = SettingsManager.Instance.CurrentSettings.cameraSensitivity;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return; 
        HandleMovement();
        HandleLook();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        IsWalking = input.magnitude > 0.1f;
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        if (cameraBobbing != null)
        {
            cameraBobbing.isWalking = IsWalking;
        }
    }

    private void HandleLook()
    {
        Vector2 input = lookAction.ReadValue<Vector2>() * cameraSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch - input.y, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        transform.Rotate(Vector3.up, input.x);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
}

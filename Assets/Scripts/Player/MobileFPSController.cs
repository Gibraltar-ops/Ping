using UnityEngine;
using UnityEngine.InputSystem; 

public class MobileFPSController : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;

    [Header("Input Actions")]
 
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction lookAction;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float cameraSensitivity = 0.1f;
    [SerializeField] private float gravity = -9.81f;

    private float cameraPitch;
    private Vector3 playerVelocity;

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
        HandleMovement();
        HandleLook();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
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
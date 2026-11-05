using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person movement and look using the new Input System.
/// Attach to the Player root (needs a CharacterController). Assign the camera as cameraPivot.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look")]
    public Transform cameraPivot;
    public float lookSensitivity = 0.1f;
    public float maxLookAngle = 85f;

    CharacterController controller;
    InputAction moveAction, lookAction, sprintAction, jumpAction;
    float verticalVelocity;
    float pitch;

    public bool InputEnabled { get; set; } = true;
    public bool IsRunning { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsGrounded => controller != null && controller.isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
        sprintAction.AddBinding("<Keyboard>/rightShift");
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
    }

    void OnEnable()
    {
        moveAction.Enable(); lookAction.Enable(); sprintAction.Enable(); jumpAction.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction.Disable(); lookAction.Disable(); sprintAction.Disable(); jumpAction.Disable();
    }

    void OnDestroy()
    {
        moveAction.Dispose(); lookAction.Dispose(); sprintAction.Dispose(); jumpAction.Dispose();
    }

    void Update()
    {
        if (!InputEnabled) return;
        Look();
        Move();
    }

    void Look()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>() * lookSensitivity;
        transform.Rotate(Vector3.up, delta.x);
        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        IsMoving = input.sqrMagnitude > 0.01f;
        IsRunning = IsMoving && sprintAction.IsPressed();
        float speed = IsRunning ? runSpeed : walkSpeed;

        Vector3 move = (transform.right * input.x + transform.forward * input.y);
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (controller.isGrounded && jumpAction.WasPressedThisFrame())
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharControl : MonoBehaviour
{
    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = 20f;
    private float currentMoveSpeed = 0f;
    private float verticalVelocity = 0f;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float lookAngleLimit = 90f;
    [SerializeField] private float lookAngle = 0f;
    private Camera mainCamera;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = GetComponentInChildren<Camera>();
        moveAction = InputSystem.actions.FindAction("Move");
        runAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = speed;
    }

    void Update()
    {
        Vector2 moveVector = moveAction.ReadValue<Vector2>();
        currentMoveSpeed = runAction.IsPressed() ? runSpeed : speed;
        HandleMovement(moveVector);

        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        HandleCamera(lookVector);
    }

    private void HandleMovement(Vector2 move)
    {
        // CORREÇÃO: Pegando a direção correta olhando para frente com base na câmera
        Vector3 forward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        
        Vector3 moveDirection = (forward * move.y + right * move.x) * currentMoveSpeed;

        if (characterController.isGrounded)
        {
            if (jumpAction.WasPressedThisFrame())
            {
                verticalVelocity = jumpForce;
            }
            else
            {
                // Uma força constante para baixo um pouco maior garante que ele grude em rampas
                verticalVelocity = -4f; 
            }
        }
        else
        {

            verticalVelocity -= gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -30f); // velocidade terminal

        }

        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCamera(Vector2 lookVector)
    {
        lookAngle += -lookVector.y * lookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -lookAngleLimit, lookAngleLimit);
        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookVector.x * lookSensitivity, 0);
    }
}
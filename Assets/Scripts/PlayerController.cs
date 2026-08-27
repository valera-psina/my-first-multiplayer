using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _walkSpeed = 6f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _airControl = 1f;

    [Header("Camera")]
    [SerializeField] private Transform _cameraTransform;

    // Публичные свойства для CameraEffects и других систем
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded => _characterController.isGrounded;
    public bool IsSprinting { get; private set; }

    // Приватные переменные
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;
    private float _currentMoveSpeed;

    private void Start()
    {
        _currentMoveSpeed = _walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- Ввод ---
    private void OnMove(InputValue val) => MoveInput = val.Get<Vector2>();

    private void OnSprint(InputValue val)
    {
        IsSprinting = val.Get<float>() > 0.5f;
        _currentMoveSpeed = IsSprinting ? _sprintSpeed : _walkSpeed;
    }

    private void OnJump()
    {
        if (IsGrounded)
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
    }

    // --- Основной цикл ---
    private void Update()
    {
        // Гравитация
        if (IsGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += _gravity * Time.deltaTime;

        // Движение
        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = _cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 desiredMovement = (forward * MoveInput.y + right * MoveInput.x) * _currentMoveSpeed;

        if (IsGrounded)
            _horizontalVelocity = desiredMovement;
        else
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, desiredMovement, _airControl * Time.deltaTime);

        Vector3 movement = _horizontalVelocity;
        movement.y = _verticalVelocity;
        _characterController.Move(movement * Time.deltaTime);

        if (IsSprinting && !Keyboard.current.leftShiftKey.isPressed)
        {
            IsSprinting = false;
            _currentMoveSpeed = _walkSpeed;
        }
    }

    // --- Толкание физических объектов ---
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;
        if (hit.moveDirection.y < -0.3f)
            return;
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.linearVelocity = pushDir * 5f;
    }
}
using Unity.Cinemachine;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Transform _cameraHolder;

    [Header("Bobbing")]
    [SerializeField] private float _walkBobbingSpeed = 4f;
    [SerializeField] private float _walkBobbingAmount = 0.25f;
    [SerializeField] private float _sprintBobbingSpeed = 8f;
    [SerializeField] private float _sprintBobbingAmount = 0.25f;
    [SerializeField] private float _bobbingResetSpeed = 5f;

    [Header("Sway")]
    [SerializeField] private float _swayTiltAmount = 2f;
    [SerializeField] private float _swayPitchAmount = 2f;
    [SerializeField] private float _swaySmoothSpeed = 8f;

    [Header("Tilt (Attack)")]
    [SerializeField] private float _attackTiltDuration = 0.3f; // затухание после ручной анимации
    private float _currentTilt;
    private float _tiltVelocity;
    private bool _isTiltAnimating = false;

    // Внутренние переменные
    private Vector3 _cameraOriginalPosition;
    private Quaternion _cameraSwayOriginalRotation;
    private float _bobTimer;
    private float _bobbingSpeed;
    private float _bobbingAmount;

    // Ссылка на PlayerController (для получения данных о движении)
    private PlayerController _playerController;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        if (_playerController == null)
            Debug.LogError("CameraEffects: PlayerController not found on same GameObject!");

        if (_cinemachineCamera != null)
            _cameraOriginalPosition = _cinemachineCamera.transform.localPosition;
        if (_cameraHolder != null)
            _cameraSwayOriginalRotation = _cameraHolder.localRotation;

        _bobbingSpeed = _walkBobbingSpeed;
        _bobbingAmount = _walkBobbingAmount;
    }

    private void LateUpdate()
    {
        if (_playerController == null) return;

        // Получаем данные из контроллера
        Vector2 moveInput = _playerController.MoveInput;
        bool isGrounded = _playerController.IsGrounded;
        bool isSprinting = _playerController.IsSprinting;

        // Обновляем параметры боббинга в зависимости от спринта
        if (isSprinting)
        {
            _bobbingSpeed = _sprintBobbingSpeed;
            _bobbingAmount = _sprintBobbingAmount;
        }
        else
        {
            _bobbingSpeed = _walkBobbingSpeed;
            _bobbingAmount = _walkBobbingAmount;
        }

        // Применяем эффекты
        CameraBobbing(moveInput, isGrounded);
        CameraSway(moveInput);
    }

    private void CameraBobbing(Vector2 moveInput, bool isGrounded)
    {
        float horizontal = moveInput.x;
        float vertical = moveInput.y;
        bool isMoving = isGrounded && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f);

        Vector3 targetWorldPos = _cameraHolder.TransformPoint(_cameraOriginalPosition);

        if (isMoving)
        {
            _bobTimer += Time.deltaTime * _bobbingSpeed;
            float theta = _bobTimer;
            float xOffset = -Mathf.Cos(theta) * (_bobbingAmount * 0.5f);
            float yOffset = -Mathf.Cos(2f * theta) * _bobbingAmount;

            targetWorldPos += _cinemachineCamera.transform.right * xOffset;
            targetWorldPos += _cinemachineCamera.transform.up * yOffset;
        }

        Vector3 targetLocalPos = _cameraHolder.InverseTransformPoint(targetWorldPos);
        _cinemachineCamera.transform.localPosition = Vector3.Lerp(
            _cinemachineCamera.transform.localPosition,
            targetLocalPos,
            Time.deltaTime * _bobbingResetSpeed);
    }

    private void CameraSway(Vector2 moveInput)
    {
        if (_cameraHolder == null) return;

        // Базовый свей от движения
        Quaternion baseSway = _cameraSwayOriginalRotation;
        if (moveInput.magnitude > 0.1f)
        {
            Vector3 rightAxis = _cinemachineCamera.transform.right;
            Vector3 forwardAxis = _cinemachineCamera.transform.forward;
            float pitchAngle = -moveInput.y * _swayPitchAmount;
            float rollAngle = -moveInput.x * _swayTiltAmount;
            Quaternion swayRotation = Quaternion.AngleAxis(pitchAngle, rightAxis) *
                                      Quaternion.AngleAxis(rollAngle, forwardAxis);
            baseSway *= swayRotation;
        }

        // Если не идёт ручная анимация, применяем автоматическое затухание
        if (!_isTiltAnimating)
        {
            _currentTilt = Mathf.SmoothDamp(_currentTilt, 0f, ref _tiltVelocity, _attackTiltDuration);
        }

        Quaternion tiltRotation = Quaternion.AngleAxis(_currentTilt, _cinemachineCamera.transform.forward);
        Quaternion targetRotation = baseSway * tiltRotation;

        _cameraHolder.localRotation = Quaternion.Slerp(
            _cameraHolder.localRotation,
            targetRotation,
            Time.deltaTime * _swaySmoothSpeed
        );
    }

    // --- Публичные методы для управления наклоном (из MeleeAttack) ---

    // Мгновенная установка угла (без анимации)
    public void SetTilt(float angle)
    {
        _currentTilt = angle;
    }

    // Плавная анимация наклона
    public void AnimateTilt(float targetAngle, float duration)
    {
        StartCoroutine(TiltAnimationRoutine(targetAngle, duration));
    }

    private System.Collections.IEnumerator TiltAnimationRoutine(float targetAngle, float duration)
    {
        _isTiltAnimating = true;
        float startAngle = _currentTilt;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _currentTilt = Mathf.Lerp(startAngle, targetAngle, t);
            yield return null;
        }

        _currentTilt = targetAngle;
        _isTiltAnimating = false;
    }
}
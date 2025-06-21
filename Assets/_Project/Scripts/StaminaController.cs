using UnityEngine;
using UnityEngine.UI;
using MiktoGames;

namespace MiktoGames
{
    public class StaminaController : MonoBehaviour
    {
        // [SerializeField] ���� � Stamina Settings
        [Header("Stamina Settings")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _drainRate = 20f;
        [SerializeField] private float _regenRate = 10f;
        [SerializeField, HideInInspector] private float _currentStamina;

        // [SerializeField] ���� � Crouched Sprint Settings
        [Header("Crouched Sprint Settings")]
        [Tooltip("�������� ������� ������� ��� ����� ���� (������ ������, ��� drainRate)")]
        [SerializeField] private float _crouchedSprintDrainRate = 10f;

        // [SerializeField] ���� � UI Settings
        [Header("UI Settings")]
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private CanvasGroup _staminaCanvasGroup;
        [Tooltip("�������� ��������� UI (� ��������)")]
        [SerializeField] private float _fadeSpeed = 0.2f;

        // [SerializeField] �����������
        [Header("Dependencies")]
        [SerializeField] private PlayerMovement _playerMovement;

        // ��������� ����
        private bool _requiresSprintReset = false;

        #region Unity Methods

        private void Awake()
        {
            _currentStamina = _maxStamina;

            if (_staminaSlider != null)
            {
                _staminaSlider.maxValue = _maxStamina;
                _staminaSlider.value = _currentStamina;
            }

            if (_staminaCanvasGroup != null)
            {
                _staminaCanvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            if (_playerMovement == null)
                return;

            // ���������� ���� "������������", ����� ������� Shift ��������
            if (_requiresSprintReset && Input.GetKeyUp(KeyCode.LeftShift))
                _requiresSprintReset = false;

            // ���������� �������� ������������ PlayerMovement
            bool isSprinting = _playerMovement.IsSprinting;
            bool isCrouching = _playerMovement.IsCrouching;
            bool isCrouchedSprint = isSprinting && isCrouching;

            // ������ ������� ��� �������� �������
            if (isSprinting && _currentStamina > 0f)
            {
                if (isCrouchedSprint)
                {
                    _currentStamina -= _crouchedSprintDrainRate * Time.deltaTime;
                }
                else
                {
                    _currentStamina -= _drainRate * Time.deltaTime;
                }

                _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            }
            // ����������� �������, ���� ������ �� �������
            else if (!isSprinting && _currentStamina < _maxStamina)
            {
                _currentStamina += _regenRate * Time.deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            }

            // ���������� UI ���������
            if (_staminaSlider != null)
                _staminaSlider.value = _currentStamina;

            float targetAlpha = isSprinting ? 1f : (_currentStamina < _maxStamina ? 0.3f : 0f);

            if (_staminaCanvasGroup != null)
            {
                _staminaCanvasGroup.alpha = Mathf.Lerp(_staminaCanvasGroup.alpha, targetAlpha, Time.deltaTime / _fadeSpeed);
            }

            // ���� ������� ��������, ��������� ������; ����� � �������� ���
            if (_currentStamina <= 0f)
            {
                _playerMovement.CanSprint = false;
                _requiresSprintReset = true;
            }
            else if (!_requiresSprintReset)
            {
                _playerMovement.CanSprint = true;
            }
        }

        #endregion
    }
}

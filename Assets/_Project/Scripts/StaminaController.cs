using UnityEngine;
using UnityEngine.UI;
using MiktoGames;

namespace MiktoGames
{
    public class StaminaController : MonoBehaviour
    {
        // [SerializeField] пол€ Ц Stamina Settings
        [Header("Stamina Settings")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _drainRate = 20f;
        [SerializeField] private float _regenRate = 10f;
        [SerializeField, HideInInspector] private float _currentStamina;

        // [SerializeField] пол€ Ц Crouched Sprint Settings
        [Header("Crouched Sprint Settings")]
        [Tooltip("—корость расхода стамины при шифте сид€ (обычно меньше, чем drainRate)")]
        [SerializeField] private float _crouchedSprintDrainRate = 10f;

        // [SerializeField] пол€ Ц UI Settings
        [Header("UI Settings")]
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private CanvasGroup _staminaCanvasGroup;
        [Tooltip("—корость затухани€ UI (в секундах)")]
        [SerializeField] private float _fadeSpeed = 0.2f;

        // [SerializeField] зависимости
        [Header("Dependencies")]
        [SerializeField] private PlayerMovement _playerMovement;

        // ѕриватные пол€
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

            // —брасываем флаг "истощЄнности", когда клавиша Shift отпущена
            if (_requiresSprintReset && Input.GetKeyUp(KeyCode.LeftShift))
                _requiresSprintReset = false;

            // »спользуем свойства обновленного PlayerMovement
            bool isSprinting = _playerMovement.IsSprinting;
            bool isCrouching = _playerMovement.IsCrouching;
            bool isCrouchedSprint = isSprinting && isCrouching;

            // –асход стамины при активном спринте
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
            // –егенераци€ стамины, если спринт не активен
            else if (!isSprinting && _currentStamina < _maxStamina)
            {
                _currentStamina += _regenRate * Time.deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            }

            // ќбновление UI элементов
            if (_staminaSlider != null)
                _staminaSlider.value = _currentStamina;

            float targetAlpha = isSprinting ? 1f : (_currentStamina < _maxStamina ? 0.3f : 0f);

            if (_staminaCanvasGroup != null)
            {
                _staminaCanvasGroup.alpha = Mathf.Lerp(_staminaCanvasGroup.alpha, targetAlpha, Time.deltaTime / _fadeSpeed);
            }

            // ≈сли стамина истощена, отключаем спринт; иначе Ц включаем его
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

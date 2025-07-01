using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiktoGames
{
    [System.Serializable]
    public class HexagonSlot
    {
        [SerializeField] private GameObject _arrow;
        [SerializeField] private GameObject _rod;

        public GameObject Arrow => _arrow;
        public GameObject Rod => _rod;
    }

    public class HexagonPuzzleController : MonoBehaviour
    {
        // Вращение происходит по шагам: 30°.
        public const float RotationAngleStep = 30f;
        private const int TotalSlots = 6; // Массив слотов состоит из 6 элементов.

        #region Inspector Parameters

        [Header("Slot Settings")]
        [Tooltip("Массив из 6 элементов, каждый содержит стрелочку и палочку. Все объекты изначально отключены.")]
        [SerializeField] private HexagonSlot[] slots = new HexagonSlot[6];

        [Header("Puzzle Settings")]
        [Tooltip("Случайно включается от minActiveSlots до maxActiveSlots элементов.")]
        [SerializeField] private int minActiveSlots = 1;
        [SerializeField] private int maxActiveSlots = 5;

        [Header("Rotating Object Settings")]
        [Tooltip("Объект, который будет вращаться. Если не задан, ищется дочерний с именем \"Hex\".")]
        [SerializeField] private GameObject rotatingHexagon;
        [Tooltip("Базовый угол вращения по оси Z, например 90°.")]
        [SerializeField] private float hexagonStartingAngle = 90f;
        [Tooltip("Время, за которое происходит поворот на 30°.")]
        [SerializeField] private float rotationDuration = 0.2f;

        [Header("Solution Settings")]
        [Tooltip("Допуск в градусах для проверки полного оборота (например, 5° допускает, что оборот от 355 до 365° считается правильным).")]
        [SerializeField] private float solutionTolerance = 5f;

        [Header("Cooldown Settings")]
        [Tooltip("Время отключения взаимодействия при ошибке, в секундах.")]
        [SerializeField] private float cooldownDuration = 30f;

        [Header("Solved Material Settings")]
        [SerializeField] private Renderer solutionObjectRenderer;
        [SerializeField] private Material unsolvedMaterial;
        [SerializeField] private Material solvedMaterial;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Звук вращения (зациклить).")]
        [SerializeField] private AudioClip rotateSound;
        [SerializeField] private AudioClip solvedSound;
        [SerializeField] private AudioClip errorSound;

        #endregion

        #region Private Variables

        private bool[] winningCombination;

        // _totalRotation – накопленный угол за текущую сессию вращения.
        private float _totalRotation;
        // _currentAngle вычисляется как hexagonStartingAngle + (_totalRotation mod 360).
        // Для визуализации используем диапазон углов Unity (от –180 до +180) через Mathf.DeltaAngle.
        private float _currentAngle;
        private bool _isRotating = false;
        private bool _keyHeld = false;
        private bool _isSolved = false;
        private bool _isCooldown = false;
        // Скорость вращения (градусы/сек): чтобы за rotationDuration секунд пройти 30°.
        private float _rotationSpeed;

        #endregion

        #region Initialization

        private void Awake()
        {
            // Если rotatingHexagon не назначен, ищем дочерний объект с именем "Hex".
            if (rotatingHexagon == null)
            {
                Transform hexTransform = transform.Find("Hex");
                if (hexTransform != null)
                    rotatingHexagon = hexTransform.gameObject;
            }

            GenerateWinningCombination();
            ApplyWinningCombination();
            InitializePuzzle();
            _rotationSpeed = RotationAngleStep / rotationDuration;

            if (solutionObjectRenderer != null && unsolvedMaterial != null)
                solutionObjectRenderer.material = unsolvedMaterial;
        }

        /// <summary>
        /// Инициализирует головоломку: _currentAngle устанавливается ровно равным hexagonStartingAngle,
        /// _totalRotation обнуляется, и генерируется новая комбинация активных слотов (без смещения).
        /// </summary>
        private void InitializePuzzle()
        {
            _isSolved = false;
            _isCooldown = false;
            _totalRotation = 0f;
            _currentAngle = hexagonStartingAngle;
            ApplyRotation();
            GenerateWinningCombination();
            ApplyWinningCombination();
            Debug.Log($"[Init] CurrentAngle: {_currentAngle}°");
        }

        #endregion

        #region Public Methods – Input

        /// <summary>
        /// Устанавливает флаг удержания клавиши, вызывается из PlayerInteraction.
        /// </summary>
        public void SetKeyHeld(bool held)
        {
            _keyHeld = held;
        }

        /// <summary>
        /// При нажатии клавиши E начинается сессия вращения: обнуляется _totalRotation и запускается звук вращения.
        /// </summary>
        public void BeginRotation()
        {
            if (_isSolved || _isCooldown)
                return;
            _keyHeld = true;
            _isRotating = true;
            _totalRotation = 0f;
            if (audioSource != null && rotateSound != null)
            {
                audioSource.loop = true;
                audioSource.clip = rotateSound;
                audioSource.Play();
            }
        }

        /// <summary>
        /// При удержании клавиши E накапливается вращение.
        /// _currentAngle вычисляется как hexagonStartingAngle + (_totalRotation mod 360) с нормализацией через Mathf.DeltaAngle.
        /// </summary>
        public void UpdateRotation(float deltaTime)
        {
            if (!_isRotating)
                return;
            float deltaRotation = _rotationSpeed * deltaTime;
            _totalRotation += deltaRotation;
            float rawAngle = hexagonStartingAngle + (_totalRotation % 360f);
            _currentAngle = Mathf.DeltaAngle(0f, rawAngle);  // Значение в диапазоне [-180, 180]
            ApplyRotation();
        }

        /// <summary>
        /// При отпускании клавиши E завершается сессия вращения.
        /// Выводится в консоль общее накопленное вращение.
        /// Если _totalRotation близко к кратному 360° (с допуском ±solutionTolerance) – пазл считается решённым.
        /// Иначе – запускается заложенный таймер, после которого генерируется новый вариант.
        /// </summary>
        public void EndRotation()
        {
            _keyHeld = false;
            if (!_isRotating)
                return;
            _isRotating = false;
            Debug.Log($"Total rotation: {_totalRotation} degrees");

            if (audioSource != null && audioSource.isPlaying && audioSource.clip == rotateSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            int fullTurns = Mathf.RoundToInt(_totalRotation / 360f);
            if (fullTurns < 1)
                fullTurns = 1;
            float targetTotal = fullTurns * 360f;
            float diff = Mathf.Abs(_totalRotation - targetTotal);
            if (diff <= solutionTolerance)
            {
                _totalRotation = targetTotal;
                float rawAngle = hexagonStartingAngle + (_totalRotation % 360f);
                _currentAngle = Mathf.DeltaAngle(0f, rawAngle);
                ApplyRotation();
                SolvePuzzle();
                return;
            }

            // Если условие не выполнено, проигрываем звук ошибки и запускаем сброс с последующим таймером.
            if (audioSource != null && errorSound != null)
                audioSource.PlayOneShot(errorSound);
            ResetPuzzleVariant();
        }

        #endregion

        #region Rotation Helper

        /// <summary>
        /// Применяет _currentAngle к вращаемому объекту.
        /// _currentAngle уже нормализовано в диапазоне [-180, 180].
        /// </summary>
        private void ApplyRotation()
        {
            if (rotatingHexagon != null)
            {
                rotatingHexagon.transform.localRotation = Quaternion.Euler(0f, -90f, _currentAngle);
            }
        }

        #endregion

        #region Solution & Reset

        /// <summary>
        /// Если условие выполнено, пазл считается решённым:
        /// воспроизводится звук успеха, у solutionObjectRenderer меняется материал, выводится сообщение.
        /// </summary>
        private void SolvePuzzle()
        {
            _isSolved = true;
            if (audioSource != null && solvedSound != null)
                audioSource.PlayOneShot(solvedSound);
            Debug.Log("Puzzle Solved");
            if (solutionObjectRenderer != null && solvedMaterial != null)
                solutionObjectRenderer.material = solvedMaterial;
        }

        /// <summary>
        /// Сбрасывает головоломку: шестиугольник возвращается в базовое положение (rotation Z = hexagonStartingAngle),
        /// новые комбинации не генерируются сразу – это произойдёт после таймера ошибки.
        /// </summary>
        private void ResetPuzzleVariant()
        {
            _isSolved = false;
            _isCooldown = true;
            _currentAngle = hexagonStartingAngle;
            _totalRotation = 0f;
            ApplyRotation();
            Debug.Log($"[Reset] Puzzle reset. CurrentAngle set to {hexagonStartingAngle}°");
            if (audioSource != null && errorSound != null)
                audioSource.PlayOneShot(errorSound);
            StartCoroutine(CooldownRoutine());
        }

        /// <summary>
        /// По окончании кулдауна генерируется новая головоломка.
        /// </summary>
        private IEnumerator CooldownRoutine()
        {
            Debug.Log("Interaction disabled for cooldown.");
            yield return new WaitForSeconds(cooldownDuration);
            _isCooldown = false;
            Debug.Log("Cooldown finished. Interaction enabled.");
            InitializePuzzle();
        }

        #endregion

        #region Slot Methods

        private void GenerateWinningCombination()
        {
            winningCombination = new bool[TotalSlots];
            for (int i = 0; i < TotalSlots; i++)
                winningCombination[i] = false;
            int activeCount = Random.Range(minActiveSlots, maxActiveSlots + 1);
            int[] indices = new int[TotalSlots];
            for (int i = 0; i < TotalSlots; i++)
                indices[i] = i;
            for (int i = 0; i < TotalSlots; i++)
            {
                int randomIndex = Random.Range(i, TotalSlots);
                int temp = indices[i];
                indices[i] = indices[randomIndex];
                indices[randomIndex] = temp;
            }
            for (int j = 0; j < activeCount; j++)
            {
                int index = indices[j];
                winningCombination[index] = true;
            }
        }

        private void ApplyWinningCombination()
        {
            for (int i = 0; i < TotalSlots; i++)
            {
                if (slots == null || i >= slots.Length)
                    continue;
                bool isActive = winningCombination[i];
                if (slots[i] != null)
                {
                    if (slots[i].Arrow != null)
                        slots[i].Arrow.SetActive(isActive);
                    if (slots[i].Rod != null)
                        slots[i].Rod.SetActive(isActive);
                }
            }
        }

        #endregion

        #region Renderer Material Initialization

        private void OnEnable()
        {
            if (solutionObjectRenderer != null && unsolvedMaterial != null)
                solutionObjectRenderer.material = unsolvedMaterial;
        }

        #endregion

        #region Public Property

        public bool IsInteractable => !_isCooldown;

        #endregion
    }
}

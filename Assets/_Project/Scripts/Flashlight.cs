using System.Collections;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Toggle Settings")]
    [SerializeField, Tooltip("Клавиша для включения/выключения фонарика")]
    private KeyCode toggleKey = KeyCode.F;

    [Header("Light Settings")]
    [SerializeField, Tooltip("Компонент Light, представляющий фонарик")]
    private Light flashlight;
    [SerializeField, Tooltip("Базовая интенсивность фонарика. Используется для расчёта мерцания и плавного включения")]
    private float baseIntensity = 1f;
    [SerializeField, Tooltip("Время плавного включения/выключения (в секундах)")]
    private float smoothToggleDuration = 0.5f;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Источник звука для фонарика")]
    private AudioSource audioSource;
    [SerializeField, Tooltip("Звук при включении фонарика")]
    private AudioClip toggleOnSound;
    [SerializeField, Tooltip("Звук при выключении фонарика")]
    private AudioClip toggleOffSound;

    [Header("Flicker Settings")]
    [SerializeField, Tooltip("Минимальный множитель интенсивности для мерцания (например, 0.8)")]
    private float flickerMinIntensity = 0.8f;
    [SerializeField, Tooltip("Максимальный множитель интенсивности для мерцания (например, 1.0)")]
    private float flickerMaxIntensity = 1.0f;
    [SerializeField, Tooltip("Минимальный интервал между мерцаниями (в секундах)")]
    private float flickerIntervalMin = 0.2f;
    [SerializeField, Tooltip("Максимальный интервал между мерцаниями (в секундах)")]
    private float flickerIntervalMax = 1.0f;
    [SerializeField, Tooltip("Длительность мерцания (в секундах)")]
    private float flickerDuration = 0.1f;

    // Флаг активности фонарика.
    private bool isOn = false;
    // Ссылки на корутины
    private Coroutine flickerCoroutine;
    private Coroutine toggleCoroutine;

    private void Awake()
    {
        // Если не задан компонент Light, пытаемся получить его из этого объекта.
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (flashlight != null)
        {
            // Сохраняем базовую интенсивность
            baseIntensity = flashlight.intensity;
            // Изначально фонарик выключен и его интенсивность = 0
            flashlight.intensity = 0f;
            flashlight.enabled = false;
        }

        // Если AudioSource не указан, пробуем найти его на объекте.
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Переключаем фонарик по нажатию заданной клавиши.
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        isOn = !isOn;

        // Останавливаем ранее запущенную корутину переключения, если она активна.
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }

        if (isOn)
        {
            // Включение: включаем Light компонент, устанавливаем интенсивность в 0.
            flashlight.enabled = true;
            flashlight.intensity = 0f;

            // Проигрываем звук включения (если настроен).
            if (audioSource != null && toggleOnSound != null)
                audioSource.PlayOneShot(toggleOnSound);

            // Запускаем плавное включение.
            toggleCoroutine = StartCoroutine(SmoothFadeIn());
        }
        else
        {
            // Выключение: проигрываем звук отключения (если настроен).
            if (audioSource != null && toggleOffSound != null)
                audioSource.PlayOneShot(toggleOffSound);

            // Запускаем плавное выключение.
            toggleCoroutine = StartCoroutine(SmoothFadeOut());
        }
    }

    private IEnumerator SmoothFadeIn()
    {
        // Если уже запущен эффект мерцания, останавливаем его на период перехода.
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        float elapsed = 0f;
        while (elapsed < smoothToggleDuration)
        {
            elapsed += Time.deltaTime;
            flashlight.intensity = Mathf.Lerp(0f, baseIntensity, elapsed / smoothToggleDuration);
            yield return null;
        }
        flashlight.intensity = baseIntensity;

        // После плавного включения запускаем корутину мерцания.
        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator SmoothFadeOut()
    {
        // Останавливаем эффект мерцания, если он работает.
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        float startIntensity = flashlight.intensity;
        float elapsed = 0f;
        while (elapsed < smoothToggleDuration)
        {
            elapsed += Time.deltaTime;
            flashlight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / smoothToggleDuration);
            yield return null;
        }
        flashlight.intensity = 0f;
        flashlight.enabled = false;
    }

    private IEnumerator FlickerRoutine()
    {
        // Эффект мерцания: фонарик периодически слегка изменяет интенсивность.
        while (isOn)
        {
            float waitTime = Random.Range(flickerIntervalMin, flickerIntervalMax);
            yield return new WaitForSeconds(waitTime);

            float flickerIntensity = baseIntensity * Random.Range(flickerMinIntensity, flickerMaxIntensity);
            flashlight.intensity = flickerIntensity;
            yield return new WaitForSeconds(flickerDuration);
            flashlight.intensity = baseIntensity;
        }
    }
}
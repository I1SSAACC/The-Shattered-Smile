using System.Collections;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Toggle Settings")]
    [SerializeField, Tooltip("������� ��� ���������/���������� ��������")]
    private KeyCode toggleKey = KeyCode.F;

    [Header("Light Settings")]
    [SerializeField, Tooltip("��������� Light, �������������� �������")]
    private Light flashlight;
    [SerializeField, Tooltip("������� ������������� ��������. ������������ ��� ������� �������� � �������� ���������")]
    private float baseIntensity = 1f;
    [SerializeField, Tooltip("����� �������� ���������/���������� (� ��������)")]
    private float smoothToggleDuration = 0.5f;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("�������� ����� ��� ��������")]
    private AudioSource audioSource;
    [SerializeField, Tooltip("���� ��� ��������� ��������")]
    private AudioClip toggleOnSound;
    [SerializeField, Tooltip("���� ��� ���������� ��������")]
    private AudioClip toggleOffSound;

    [Header("Flicker Settings")]
    [SerializeField, Tooltip("����������� ��������� ������������� ��� �������� (��������, 0.8)")]
    private float flickerMinIntensity = 0.8f;
    [SerializeField, Tooltip("������������ ��������� ������������� ��� �������� (��������, 1.0)")]
    private float flickerMaxIntensity = 1.0f;
    [SerializeField, Tooltip("����������� �������� ����� ���������� (� ��������)")]
    private float flickerIntervalMin = 0.2f;
    [SerializeField, Tooltip("������������ �������� ����� ���������� (� ��������)")]
    private float flickerIntervalMax = 1.0f;
    [SerializeField, Tooltip("������������ �������� (� ��������)")]
    private float flickerDuration = 0.1f;

    // ���� ���������� ��������.
    private bool isOn = false;
    private bool _isOnInteractable;
    private bool _isInteractable = true;

    // ������ �� ��������
    private Coroutine flickerCoroutine;
    private Coroutine toggleCoroutine;

    private void Awake()
    {
        // ���� �� ����� ��������� Light, �������� �������� ��� �� ����� �������.
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (flashlight != null)
        {
            // ��������� ������� �������������
            baseIntensity = flashlight.intensity;
            // ���������� ������� �������� � ��� ������������� = 0
            flashlight.intensity = 0f;
            flashlight.enabled = false;
        }

        // ���� AudioSource �� ������, ������� ����� ��� �� �������.
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_isInteractable == false)
            return;

        // ����������� ������� �� ������� �������� �������.
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    public void DisableInteractable()
    {
        _isOnInteractable = isOn;
        _isInteractable = false;

        if (isOn)
            Disable();
    }

    public void EnableInteractable()
    {
        _isInteractable = true;

        if (_isOnInteractable)
            Enable();
    }

    private void ToggleFlashlight()
    {
        isOn = !isOn;

        // ������������� ����� ���������� �������� ������������, ���� ��� �������.
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }

        if (isOn)
            Enable();
        else
            Disable();
    }

    private void Enable()
    {
        flashlight.enabled = true;
        flashlight.intensity = 0f;

        // ����������� ���� ��������� (���� ��������).
        if (audioSource != null && toggleOnSound != null)
            audioSource.PlayOneShot(toggleOnSound);

        // ��������� ������� ���������.
        toggleCoroutine = StartCoroutine(SmoothFadeIn());
    }

    private void Disable()
    {
        // ����������: ����������� ���� ���������� (���� ��������).
        if (audioSource != null && toggleOffSound != null)
            audioSource.PlayOneShot(toggleOffSound);

        // ��������� ������� ����������.
        toggleCoroutine = StartCoroutine(SmoothFadeOut());
    }

    private IEnumerator SmoothFadeIn()
    {
        // ���� ��� ������� ������ ��������, ������������� ��� �� ������ ��������.
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

        // ����� �������� ��������� ��������� �������� ��������.
        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator SmoothFadeOut()
    {
        // ������������� ������ ��������, ���� �� ��������.
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
        // ������ ��������: ������� ������������ ������ �������� �������������.
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
using System.Collections;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light _flashlight;
    [SerializeField] private float _smoothToggleDuration = 0.1f;
    [SerializeField] private Vector2 _flickerIntensity = new(0.8f, 1.0f);
    [SerializeField] private Vector2 _flickerInterval = new(0.2f, 1.0f);
    [SerializeField] private float _flickerDuration = 0.1f;

    private float _baseIntensity;
    private bool _isOn;
    private bool _isOnInteractable;
    private bool _isInteractable = true;

    private Coroutine _flickerCoroutine;
    private Coroutine _toggleCoroutine;

    private void Awake()
    {
        _baseIntensity = _flashlight.intensity;
        _flashlight.intensity = 0f;
        _flashlight.enabled = false;
    }

    public void EnableInteractable()
    {
        _isInteractable = true;

        if (_isOnInteractable)
            Enable();
    }

    public void DisableInteractable()
    {
        _isOnInteractable = _isOn;
        _isInteractable = false;

        if (_isOn)
            Disable();
    }

    public void ToggleFlashlight()
    {
        if (_isInteractable == false)
            return;

        _isOn = !_isOn;

        if (_toggleCoroutine != null)
        {
            StopCoroutine(_toggleCoroutine);
            _toggleCoroutine = null;
        }

        if (_isOn)
            Enable();
        else
            Disable();
    }

    private void Enable()
    {
        _flashlight.enabled = true;
        _flashlight.intensity = 0f;
        _toggleCoroutine = StartCoroutine(StartFadeCoroutine(_flashlight.intensity, _baseIntensity));
        SfxPlayer3D.Instance.PlayFlashlightEnable(transform);
    }

    private void Disable()
    {
        _toggleCoroutine = StartCoroutine(StartFadeCoroutine(_flashlight.intensity, 0));
        SfxPlayer3D.Instance.PlayFlashlightDisable(transform);
    }

    private IEnumerator StartFadeCoroutine(float startIntensity, float endItensity)
    {
        if (_flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        float elapsed = 0f;

        while (elapsed < _smoothToggleDuration)
        {
            elapsed += Time.deltaTime;
            _flashlight.intensity = Mathf.Lerp(startIntensity, endItensity, elapsed / _smoothToggleDuration);
            yield return null;
        }

        _flashlight.intensity = endItensity;

        if (endItensity == 0f)
            _flashlight.enabled = false;
        else
            _flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (_isOn)
        {
            float waitTime = Random.Range(_flickerInterval.x, _flickerInterval.y);
            yield return new WaitForSeconds(waitTime);

            float flickerIntensity = _baseIntensity * Random.Range(_flickerIntensity.x, _flickerIntensity.y);
            _flashlight.intensity = flickerIntensity;
            yield return new WaitForSeconds(_flickerDuration);

            _flashlight.intensity = _baseIntensity;
        }
    }
}
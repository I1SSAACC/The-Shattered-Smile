using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum FootstepType
{
    OrdinaryWalking,
    OrdinaryRunning,
    WoodWalking,
    WoodRunning,
}

public class SfxPlayer3D : MonoBehaviour
{
    public static SfxPlayer3D Instance { get; private set; }

    [SerializeField] private CustomAudioSource _prefab;
    [SerializeField] private AudioClip _flashlightEnable;
    [SerializeField] private AudioClip _flashlightDisable;

    [SerializeField] private Dictionary<FootstepType, AudioClip[]> _footstepSounds = new();
    [SerializeField] private AudioClip[] _ordinaryWalkingFootsteps;
    [SerializeField] private AudioClip[] _ordinaryRunningFootsteps;
    [SerializeField] private AudioClip[] _woodWalkingFootsteps;
    [SerializeField] private AudioClip[] _woodRunningFootsteps;
    [SerializeField] private AudioClip _jumpStartSound;
    [SerializeField] private AudioClip _jumpLandSound;

    private Pool<CustomAudioSource> _pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFootstepDictionary();
        _pool = new(_prefab, transform, 200);
    }

    private void InitializeFootstepDictionary()
    {
        _footstepSounds.Clear();
        _footstepSounds.Add(FootstepType.OrdinaryWalking, _ordinaryWalkingFootsteps);
        _footstepSounds.Add(FootstepType.OrdinaryRunning, _ordinaryRunningFootsteps);
        _footstepSounds.Add(FootstepType.WoodWalking, _woodWalkingFootsteps);
        _footstepSounds.Add(FootstepType.WoodRunning, _woodRunningFootsteps);
    }

    public void PlayFootstep(FootstepType type, Transform target)
    {
        if (_pool.TryGet(out CustomAudioSource audioSource) &&
            _footstepSounds.TryGetValue(type, out AudioClip[] clips) &&
            clips != null && clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(clip, target);
        }
    }

    public void PlayFlashlightEnable(Transform target)
    {
        if (_pool.TryGet(out CustomAudioSource audioSource))
            audioSource.PlayOneShot(_flashlightEnable, target);
    }

    public void PlayFlashlightDisable(Transform target)
    {
        if (_pool.TryGet(out CustomAudioSource audioSource))
            audioSource.PlayOneShot(_flashlightDisable, target);
    }

    public void PlayJumpStartSound(Transform target)
    {
        if (_pool.TryGet(out CustomAudioSource audioSource))
            audioSource.PlayOneShot(_jumpStartSound, target);
    }

    public void PlayJumpLandSound(Transform target)
    {
        if (_pool.TryGet(out CustomAudioSource audioSource))
            audioSource.PlayOneShot(_jumpLandSound, target);
    }
}
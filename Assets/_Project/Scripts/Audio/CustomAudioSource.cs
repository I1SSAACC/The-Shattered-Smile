using System;
using System.Collections;
using UnityEngine;

public class CustomAudioSource : MonoBehaviour, IDeactivatable<CustomAudioSource>
{
    [SerializeField] private AudioSource _source;

    private Transform _target;

    public event Action<CustomAudioSource> Deactivated;

    public void Deactivate() =>
        Deactivated?.Invoke(this);

    public void PlayOneShot(AudioClip clip, Transform target)
    {
        _target = target;
        _source.PlayOneShot(clip);
        StartCoroutine(PlaingCoroutine());
    }

    private IEnumerator PlaingCoroutine()
    {
        while (_source.isPlaying)
        {
            transform.position = _target.position;
            yield return null;
        }

        Deactivate();
    }
}
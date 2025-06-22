using System;
using System.Collections;
using MiktoGames;
using UnityEngine;

public class Locker : MonoBehaviour
{
    [SerializeField] private LockerAnimator _lockerAnimator;
    [SerializeField] private float _moveDuration;
    [SerializeField] private float _fastMoveDuration;
    [SerializeField] private float _timeWaitToMove;

    public Transform _entryStartTarget;
    public Transform _entryTarget;
    public Transform _exitTarget;

    private bool _isPlayerHidden = false;
    private bool _isBusy = false;
    private WaitForSeconds _waitToMove;

    private void Awake() =>
        _waitToMove = new(_timeWaitToMove);

    public void ToggleLocker(Player player)
    {
        if (_isBusy)
            return;

        if (_isPlayerHidden == false)
            StartCoroutine(EnteringLocker(player));
        else
            StartCoroutine(ExitingLocker(player));
    }

    private IEnumerator EnteringLocker(Player player)
    {
        if (player == null)
            throw new ArgumentNullException("Не удалось получить игрока");

        _isBusy = true;

        player.DisableControl();

        yield return StartCoroutine(MovePlayer(player.transform, _entryStartTarget, _fastMoveDuration));

        _lockerAnimator.PlayOpen();

        yield return _waitToMove;

        yield return StartCoroutine(MovePlayer(player.transform, _entryTarget, _moveDuration));

        player.DisableFlashlight();
        _lockerAnimator.PlayClose();
        player.CameraTransform.localRotation = Quaternion.identity;

        yield return null;

        _isPlayerHidden = true;
        _isBusy = false;
    }

    private IEnumerator ExitingLocker(Player player)
    {
        if (player == null)
            throw new ArgumentNullException("Не удалось получить игрока");

        _isBusy = true;
        _lockerAnimator.PlayOpen();

        yield return _waitToMove;

        yield return StartCoroutine(MovePlayer(player.transform, _exitTarget, _moveDuration));

        player.CameraTransform.localRotation = Quaternion.identity;
        player.EnableControl();

        yield return null;

        _lockerAnimator.PlayClose();
        player.EnableFlashlight();
        _isPlayerHidden = false;
        _isBusy = false;
    }

    private IEnumerator MovePlayer(Transform playerTransform, Transform target, float duration)
    {
        playerTransform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            playerTransform.SetPositionAndRotation
            (
                Vector3.Lerp(startPosition, target.position, t),
                Quaternion.Slerp(startRotation, target.rotation, t)
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        playerTransform.SetPositionAndRotation(target.position, target.rotation);
    }
}
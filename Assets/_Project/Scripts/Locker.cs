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

    private WaitForSeconds _waitToMove;

    private void Awake() =>
        _waitToMove = new(_timeWaitToMove);

    public void ToggleLocker(Player player)
    {
        if (player.IsHidden == false)
            StartCoroutine(EnteringLocker(player));
        else
            StartCoroutine(ExitingLocker(player));
    }

    private IEnumerator EnteringLocker(Player player)
    {
        player.DisableControl();

        yield return StartCoroutine(Utils.MovePlayer(player.transform, _entryStartTarget, _fastMoveDuration));

        _lockerAnimator.PlayOpen();

        yield return _waitToMove;

        yield return StartCoroutine(Utils.MovePlayer(player.transform, _entryTarget, _moveDuration));

        player.DisableFlashlight();
        _lockerAnimator.PlayClose();
        player.CameraTransform.localRotation = Quaternion.identity;
        player.SetHiddenStatus(true);
    }

    private IEnumerator ExitingLocker(Player player)
    {
        player.SetHiddenStatus(false);
        _lockerAnimator.PlayOpen();

        yield return _waitToMove;

        yield return StartCoroutine(Utils.MovePlayer(player.transform, _exitTarget, _moveDuration));

        player.EnableControl();
        _lockerAnimator.PlayClose();
        player.EnableFlashlight();
    }
}

using System.Collections;
using MiktoGames;
using UnityEngine;

public class LockerController : MonoBehaviour
{
    public Animator lockerAnimator;

    public float moveDuration = 1f;
    public float fastMoveDuration = 0.3f;
    public float WaitToMove = 0.3f;

    public Transform entryStartPosition;
    public Transform insidePosition;
    public Transform exitPosition;

    public bool IsInteractable = true;

    private bool isPlayerHidden = false;
    private bool isBusy = false;

    public void ToggleLocker()
    {
        if (!isBusy)
        {
            if (!isPlayerHidden)
                StartCoroutine(EnterLocker());
            else
                StartCoroutine(ExitLocker());
        }
    }

    IEnumerator EnterLocker()
    {
        isBusy = true;
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player не найден на сцене.");
            yield break;
        }

        if (player.TryGetComponent<HeadBobbing>(out var bobbing))
            bobbing.enabled = false;
        if (player.TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = false;
        if (player.TryGetComponent<MouseLook>(out var look))
            look.enabled = false;

        Vector3 currPos = player.transform.position;
        Quaternion currRot = player.transform.rotation;
        yield return StartCoroutine(MovePlayer(
            player.transform,
            currPos, entryStartPosition.position,
            currRot, entryStartPosition.rotation,
            fastMoveDuration));

        lockerAnimator.SetBool("IsOpen", true);

        yield return new WaitForSeconds(WaitToMove);

        yield return StartCoroutine(MovePlayer(
            player.transform,
            entryStartPosition.position, insidePosition.position,
            entryStartPosition.rotation, insidePosition.rotation,
            moveDuration));

        lockerAnimator.SetBool("IsOpen", false);

        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.identity;

        yield return null;

        isPlayerHidden = true;
        isBusy = false;
    }

    IEnumerator ExitLocker()
    {
        isBusy = true;
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player не найден на сцене.");
            yield break;
        }

        lockerAnimator.SetBool("IsOpen", true);

        yield return new WaitForSeconds(WaitToMove);

        Vector3 startPos = player.transform.position;
        Quaternion currentRot = player.transform.rotation;
        yield return StartCoroutine(MovePlayer(
            player.transform,
            startPos, exitPosition.position,
            currentRot, currentRot,
            moveDuration, false));

        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.identity;

        if (player.TryGetComponent<MouseLook>(out var look))
            look.enabled = true;
        if (player.TryGetComponent<HeadBobbing>(out var bobbing))
            bobbing.enabled = true;
        if (player.TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = true;

        yield return null;
        lockerAnimator.SetBool("IsOpen", false);

        isPlayerHidden = false;
        isBusy = false;
    }
    IEnumerator MovePlayer(Transform playerTransform, Vector3 startPos, Vector3 endPos,
                           Quaternion startRot, Quaternion endRot, float duration, bool interpolateRotation = true)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            playerTransform.position = Vector3.Lerp(startPos, endPos, t);
            if (interpolateRotation)
                playerTransform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = endPos;
        if (interpolateRotation)
            playerTransform.rotation = endRot;
    }
}
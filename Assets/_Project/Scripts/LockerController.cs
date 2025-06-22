using System.Collections;
using MiktoGames;
using UnityEngine;

public class LockerController : MonoBehaviour
{
    [Header("��������� ��������")]
    public Animator lockerAnimator; // ������ �� Animator ������

    [Header("��������� �����������")]
    [Tooltip("����� �������� ����������� �� entryStartPosition � insidePosition (��� �����) ��� �� ������� ������� � exitPosition (��� ������)")]
    public float moveDuration = 1f;
    [Tooltip("����� �������� ����������� �� entryStartPosition (��� �����)")]
    public float fastMoveDuration = 0.3f;
    [Tooltip("�������� ����� ��������� ����� � �������� �������� ����������� (��������, 0.3 �������)")]
    public float WaitToMove = 0.3f;

    [Header("������� ��� ������")]
    [Tooltip("�����, ���� ����� ������ ������������ ��� ������� �� � (������ � ������)")]
    public Transform entryStartPosition;
    [Tooltip("������� ����� ������ ������")]
    public Transform insidePosition;
    [Tooltip("������� ����� ��� ������ �� ������")]
    public Transform exitPosition;

    [Header("��������� ��������������")]
    public bool IsInteractable = true; // ����, ����������� ��������������

    private bool isPlayerHidden = false; // true � ����� ������ ������, false � �������
    private bool isBusy = false;         // ��������� ��������� �������������� �� ����� ���������� �������

    /// <summary>
    /// ����������� ��������� ������: ���� ��� �����.
    /// </summary>
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
            Debug.LogError("Player �� ������ �� �����.");
            yield break;
        }

        // ��������� ���������� �������
        if (player.TryGetComponent<HeadBobbing>(out var bobbing))
            bobbing.enabled = false;
        if (player.TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = false;
        if (player.TryGetComponent<MouseLook>(out var look))
            look.enabled = false;

        // 1. ������ ���������� ������ �� ��� ������� ������� � entryStartPosition (� ���������)
        Vector3 currPos = player.transform.position;
        Quaternion currRot = player.transform.rotation;
        yield return StartCoroutine(MovePlayer(
            player.transform,
            currPos, entryStartPosition.position,
            currRot, entryStartPosition.rotation,
            fastMoveDuration));

        // 2. ��������� ��������� �����
        lockerAnimator.SetBool("IsOpen", true);

        // 3. ��� �������� �����
        yield return new WaitForSeconds(WaitToMove);

        // 4. ������ ���������� ������ �� entryStartPosition � insidePosition � ������ ������������� ������� � ��������
        yield return StartCoroutine(MovePlayer(
            player.transform,
            entryStartPosition.position, insidePosition.position,
            entryStartPosition.rotation, insidePosition.rotation,
            moveDuration));

        // ����������� ���������� ��������� ������� ������
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.identity;

        // 5. ��������� �����
        yield return null;
        lockerAnimator.SetBool("IsOpen", false);

        isPlayerHidden = true;
        isBusy = false;
    }

    IEnumerator ExitLocker()
    {
        isBusy = true;
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player �� ������ �� �����.");
            yield break;
        }

        // 1. ��������� ����� ��� ������
        lockerAnimator.SetBool("IsOpen", true);

        // 2. ��� �������� �����
        yield return new WaitForSeconds(WaitToMove);

        // 3. ������ ���������� ������ �� ��� ������� ������� � exitPosition.
        // ��� ���� ������� ���������������, � �������� �������� ����������.
        Vector3 startPos = player.transform.position;
        Quaternion currentRot = player.transform.rotation; // ��������� ������� �������
        yield return StartCoroutine(MovePlayer(
            player.transform,
            startPos, exitPosition.position,
            currentRot, currentRot, // ���������� ��������, ����� �������� �� ����������
            moveDuration, false));   // false = �� ��������������� �������

        // ���� �����, ���������� ��������� ������� ������
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.identity;

        // 4. �������� ���������� �������
        if (player.TryGetComponent<MouseLook>(out var look))
            look.enabled = true;
        if (player.TryGetComponent<HeadBobbing>(out var bobbing))
            bobbing.enabled = true;
        if (player.TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = true;

        // 5. ��������� �����
        yield return null;
        lockerAnimator.SetBool("IsOpen", false);

        isPlayerHidden = false;
        isBusy = false;
    }

    /// <summary>
    /// ������ ���������� ������ �� startPos � endPos � ������������� ������� �, �����������, ��������.
    /// </summary>
    IEnumerator MovePlayer(Transform playerTransform, Vector3 startPos, Vector3 endPos,
                           Quaternion startRot, Quaternion endRot, float duration, bool interpolateRotation = true)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            playerTransform.position = Vector3.Lerp(startPos, endPos, t);
            if (interpolateRotation)
            {
                playerTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }
            // ���� �������� �� ���������������, ��������� �������� ��� ���������
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = endPos;
        if (interpolateRotation)
            playerTransform.rotation = endRot;
    }
}
using System.Collections;
using MiktoGames;
using UnityEngine;

public class LockerController : MonoBehaviour
{
    [Header("Настройки анимации")]
    public Animator lockerAnimator; // Ссылка на Animator локера

    [Header("Параметры перемещения")]
    [Tooltip("Время плавного перемещения от entryStartPosition к insidePosition (при входе) или от текущей позиции к exitPosition (при выходе)")]
    public float moveDuration = 1f;
    [Tooltip("Время быстрого перемещения до entryStartPosition (при входе)")]
    public float fastMoveDuration = 0.3f;
    [Tooltip("Задержка между открытием двери и запуском плавного перемещения (например, 0.3 секунды)")]
    public float WaitToMove = 0.3f;

    [Header("Позиции для игрока")]
    [Tooltip("Точка, куда игрок быстро перемещается при нажатии на Е (подход к локеру)")]
    public Transform entryStartPosition;
    [Tooltip("Целевая точка внутри локера")]
    public Transform insidePosition;
    [Tooltip("Целевая точка при выходе из локера")]
    public Transform exitPosition;

    [Header("Параметры взаимодействия")]
    public bool IsInteractable = true; // Флаг, разрешающий взаимодействие

    private bool isPlayerHidden = false; // true – игрок внутри локера, false – снаружи
    private bool isBusy = false;         // Блокирует повторное взаимодействие во время выполнения корутин

    /// <summary>
    /// Переключает состояние локера: вход или выход.
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
            Debug.LogError("Player не найден на сцене.");
            yield break;
        }

        // Отключаем управление игроком
        if (player.TryGetComponent<HeadBobbing>(out var bobbing))
            bobbing.enabled = false;
        if (player.TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = false;
        if (player.TryGetComponent<MouseLook>(out var look))
            look.enabled = false;

        // 1. Быстро перемещаем игрока от его текущей позиции к entryStartPosition (с поворотом)
        Vector3 currPos = player.transform.position;
        Quaternion currRot = player.transform.rotation;
        yield return StartCoroutine(MovePlayer(
            player.transform,
            currPos, entryStartPosition.position,
            currRot, entryStartPosition.rotation,
            fastMoveDuration));

        // 2. Мгновенно открываем дверь
        lockerAnimator.SetBool("IsOpen", true);

        // 3. Ждём заданное время
        yield return new WaitForSeconds(WaitToMove);

        // 4. Плавно перемещаем игрока от entryStartPosition к insidePosition с полной интерполяцией позиции и вращения
        yield return StartCoroutine(MovePlayer(
            player.transform,
            entryStartPosition.position, insidePosition.position,
            entryStartPosition.rotation, insidePosition.rotation,
            moveDuration));

        // Опционально сбрасываем локальный поворот камеры
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.identity;

        // 5. Закрываем дверь
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
            Debug.LogError("Player не найден на сцене.");
            yield break;
        }

        // 1. Открываем дверь для выхода
        lockerAnimator.SetBool("IsOpen", true);

        // 2. Ждём заданное время
        yield return new WaitForSeconds(WaitToMove);

        // 3. Плавно перемещаем игрока от его текущей позиции к exitPosition.
        // При этом позиция интерполируется, а вращение остается неизменным.
        Vector3 startPos = player.transform.position;
        Quaternion currentRot = player.transform.rotation; // сохраняем текущий поворот
        yield return StartCoroutine(MovePlayer(
            player.transform,
            startPos, exitPosition.position,
            currentRot, currentRot, // одинаковые значения, чтобы вращение не изменялось
            moveDuration, false));   // false = не интерполировать поворот

        // Если нужно, сбрасываем локальный поворот камеры
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
            cam.transform.localRotation = Quaternion.identity;

        // 4. Включаем управление игроком
        if (player.TryGetComponent<MouseLook>(out var look))
            look.enabled = true;
        if (player.TryGetComponent<HeadBobbing>(out var bobbing))
            bobbing.enabled = true;
        if (player.TryGetComponent<PlayerMovement>(out var movement))
            movement.enabled = true;

        // 5. Закрываем дверь
        yield return null;
        lockerAnimator.SetBool("IsOpen", false);

        isPlayerHidden = false;
        isBusy = false;
    }

    /// <summary>
    /// Плавно перемещает объект от startPos к endPos с интерполяцией позиции и, опционально, вращения.
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
            // Если вращение не интерполируется, оставляем значение без изменений
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = endPos;
        if (interpolateRotation)
            playerTransform.rotation = endRot;
    }
}
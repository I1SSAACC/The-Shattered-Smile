using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator MovePlayer(Transform playerTransform, Transform target, float duration)
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
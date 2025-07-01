using UnityEngine;

public class LockerAnimator : MonoBehaviour
{
    private const string IsOpen = nameof(IsOpen);

    [SerializeField] private Animator _lockerAnimator;

    public void PlayOpen() =>
        _lockerAnimator.SetBool(IsOpen, true);

    public void PlayClose() =>
        _lockerAnimator.SetBool(IsOpen, false);
}
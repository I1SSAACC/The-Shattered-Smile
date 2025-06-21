using UnityEngine;

[System.Serializable]
public class HexagonSlot
{
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _rod;

    public GameObject Arrow => _arrow;
    public GameObject Rod => _rod;
}
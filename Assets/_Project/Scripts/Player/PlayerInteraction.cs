using UnityEngine;

namespace MiktoGames
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private float interactionDistance = 3f;
        private Camera playerCamera;

        // ���������� ���������� ��� ����������� ��������� �����������
        private HexagonPuzzleController puzzleController;
        private Locker lockerController;

        private void Awake()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogError("PlayerInteraction: No MainCamera found. Ensure a camera is tagged MainCamera.");
        }

        private void Update()
        {
            // ������� �� ������ ������.
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                // ����� ���������� �������� ������ ���������� � ������� (��� ��� ��������)
                puzzleController = hit.collider.GetComponentInParent<HexagonPuzzleController>();
                lockerController = hit.collider.GetComponentInParent<Locker>();
            }
            else
            {
                puzzleController = null;
                lockerController = null;
            }

            // ���� ��������� �������������� ������ � ������������ ��� ������.
            if (puzzleController != null && puzzleController.IsInteractable)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    puzzleController.SetKeyHeld(true);
                    puzzleController.BeginRotation();
                }
                if (Input.GetKey(KeyCode.E))
                {
                    puzzleController.UpdateRotation(Time.deltaTime);
                }
                if (Input.GetKeyUp(KeyCode.E))
                {
                    puzzleController.SetKeyHeld(false);
                    puzzleController.EndRotation();
                }
            }
            // ���� ��������� �����, ����� �������� ��� ����� ������������
            else if (lockerController != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    lockerController.ToggleLocker(_player);
                }
            }
        }
    }
}
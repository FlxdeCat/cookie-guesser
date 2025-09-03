using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;

    private BoxController currentBox;

    private void Update()
    {
        CheckForBox();

        if (currentBox != null && Input.GetMouseButtonDown(0) && !GameManager.Instance.isGameOver)
        {
            currentBox.BreakBox(MainRoomSpawner.Instance.GetBoxType(currentBox));
        }
    }

    private void CheckForBox()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) && !GameManager.Instance.isGameOver)
        {
            BoxController box = hit.collider.GetComponentInParent<BoxController>();

            if (box != null && !box.IsOpened)
            {
                if (currentBox != box)
                {
                    HideCurrentUI();

                    currentBox = box;
                    currentBox.ShowUI();
                }
                return;
            }
        }

        // If no box detected OR out of range, hide UI
        HideCurrentUI();
    }

    private void HideCurrentUI()
    {
        if (currentBox != null)
        {
            currentBox.HideUI();
            currentBox = null;
        }
    }
}

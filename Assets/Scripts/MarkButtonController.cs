using UnityEngine;

public class MarkButtonController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;

    [Header("Colors")]
    [SerializeField] private Color hoverColor = new Color(30f / 255f, 120f / 255f, 220f / 255f);
    [SerializeField] private Color clickColor = new Color(20f / 255f, 90f / 255f, 200f / 255f);

    private MeshRenderer meshRenderer;
    private Collider col;
    private Color originalColor;
    private bool isHovering;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        if (meshRenderer != null && meshRenderer.material != null)
            originalColor = meshRenderer.material.color;
        else
            originalColor = Color.white;
    }

    private void Update()
    {
        if (playerCamera == null || meshRenderer == null || col == null) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (isHovering) SetHover(false);
            return;
        }

        CheckForHover();

        if (isHovering && Input.GetMouseButtonDown(0))
        {
            SetColor(clickColor);
            HandleClick();
        }

        if (isHovering && Input.GetMouseButtonUp(0))
        {
            SetColor(hoverColor);
        }
    }

    private void CheckForHover()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider == col)
            {
                if (!isHovering) SetHover(true);
                return;
            }
        }

        if (isHovering) SetHover(false);
    }

    private void SetHover(bool hover)
    {
        isHovering = hover;
        SetColor(hover ? hoverColor : originalColor);
    }

    private void SetColor(Color c)
    {
        if (meshRenderer != null) meshRenderer.material.color = c;
    }

    private void HandleClick()
    {
        MainRoomSpawner.Instance.OnMarkClicked();
    }
}

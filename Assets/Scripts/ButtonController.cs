using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public Color normalColor = Color.red;
    public Color hoverColor = new Color(1f, 0.3f, 0.3f);
    public float pressDepth = 0.08f;
    public float pressSpeed = 10f;

    private Vector3 originalPosition;
    private Renderer buttonRenderer;
    private bool isPressed = false;

    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        originalPosition = transform.position;
        buttonRenderer = GetComponent<Renderer>();
        buttonRenderer.material.color = normalColor;

        audioSource = GetComponent<AudioSource>();
    }

    void OnMouseEnter()
    {
        buttonRenderer.material.color = hoverColor;
    }

    void OnMouseExit()
    {
        buttonRenderer.material.color = normalColor;
    }

    void OnMouseDown()
    {
        if (!isPressed && !GameManager.Instance.isGameOver)
        {
            audioSource?.PlayOneShot(clickSound);
            StartCoroutine(PressAnimation());
        }
    }

    System.Collections.IEnumerator PressAnimation()
    {
        isPressed = true;

        Vector3 pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);

        // Move down
        while (Vector3.Distance(transform.position, pressedPosition) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, pressedPosition, Time.deltaTime * pressSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f); // small delay

        // Move up
        while (Vector3.Distance(transform.position, originalPosition) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * pressSpeed);
            yield return null;
        }

        OnButtonClick();
        isPressed = false;
    }

    void OnButtonClick()
    {
        GameManager.Instance.CashIn();
    }
}

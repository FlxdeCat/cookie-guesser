using TMPro;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class RewardPanelData
{
    public BoxRewardType type;
    public GameObject panel;
    public UnityEngine.Events.UnityEvent OnReward;
}

public enum BoxRewardType
{
    Cookie,
    Skull,
}

public class BoxController : MonoBehaviour
{
    [Header("Box Objects")]
    [SerializeField] private GameObject openBox;
    [SerializeField] private GameObject closedBox;

    [Header("UI Elements")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject openPanel;
    [SerializeField] private GameObject markPanel;
    [SerializeField] private TextMeshProUGUI markAccuracy;

    [Header("Reward Panels")]
    [SerializeField] private RewardPanelData[] rewardPanels;

    [Header("Effects")]
    [SerializeField] private AudioClip breakSound;

    private bool isOpened = false;
    private bool isMarked = false;
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private GameObject debugCube;

    private void Awake()
    {
        openBox.SetActive(false);
        closedBox.SetActive(true);
        canvas.SetActive(false);
        openPanel.SetActive(false);

        foreach (var panelData in rewardPanels)
        {
            panelData.panel.SetActive(false);

            if (panelData.type == BoxRewardType.Cookie)
            {
                panelData.OnReward.AddListener(RewardCookies);
            }
            else if (panelData.type == BoxRewardType.Skull)
            {
                panelData.OnReward.AddListener(GameOver);
            }
        }

        audioSource = GetComponent<AudioSource>();
    }

    public bool IsOpened => isOpened;

    public bool IsMarked => isMarked;

    public void ShowUI()
    {
        canvas.SetActive(true);
        if (!isOpened) openPanel.SetActive(true);
    }

    public void HideUI()
    {
        if (!isOpened)
        {
            openPanel.SetActive(false);
            canvas.SetActive(false);
        }
    }

    public void RewardCookies()
    {
        int rewardAmount = GameManager.Instance.GetRandomCookieReward();
        RewardPanelData cookiePanel = System.Array.Find(rewardPanels, p => p.type == BoxRewardType.Cookie);
        TextMeshProUGUI rewardText = cookiePanel.panel.GetComponentInChildren<TextMeshProUGUI>();
        rewardText.text = $"+{rewardAmount}";
        GameManager.Instance.AddCookies(rewardAmount);
    }

    public void GameOver()
    {
        GameManager.Instance.GameOver();
    }

    public void BreakBox(BoxRewardType type)
    {
        if (isOpened) return;

        isOpened = true;

        // Switch box models
        closedBox.SetActive(false);
        openBox.SetActive(true);

        // Play effects
        if (audioSource != null && breakSound != null) audioSource.PlayOneShot(breakSound);

        // Hide interaction panel
        openPanel.SetActive(false);

        // Show reward panel based on type
        foreach (var panelData in rewardPanels)
        {
            if (panelData.type == type)
            {
                panelData.panel.SetActive(true);
                AnimateReward(panelData.panel.transform);
                panelData.OnReward?.Invoke();
                break;
            }
        }
    }

    public void ApplyMark(float accuracyPercent)
    {
        isMarked = true;
        if (markPanel != null) markPanel.SetActive(true);
        if (markAccuracy != null)
        {
            markAccuracy.text = $"{accuracyPercent:0.##}%";
        }
    }

    private void AnimateReward(Transform target)
    {
        StartCoroutine(ScaleIn(target, 0.5f));
    }

    private System.Collections.IEnumerator ScaleIn(Transform target, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float time = 0;

        target.localScale = startScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            target.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            yield return null;
        }
        target.localScale = endScale;
    }

    public void ToggleDebugCube(BoxRewardType type)
    {
        if (debugCube != null)
        {
            debugCube.SetActive(true);
            if (type == BoxRewardType.Cookie)
            {
                debugCube.GetComponent<Renderer>().material.color = Color.green;
            }
            else if (type == BoxRewardType.Skull)
            {
                debugCube.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
}

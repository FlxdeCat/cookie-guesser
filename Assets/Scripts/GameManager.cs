using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameOver = false;

    [Header("UI Elements")]
    public TextMeshProUGUI cookieText;

    [Header("Reward Settings")]
    [SerializeField] private int minCookies = 1;
    [SerializeField] private int maxCookies = 3;

    public int MinCookies => minCookies;
    public int MaxCookies => maxCookies;

    private int totalCookies = 0;

    private AudioSource audioSource;

    [Header("Game Over Settings")]
    public float gameOverWaitDuration = 1.0f;

    [Header("Shop Upgrade SFX")]
    public AudioClip successSfx;
    public AudioClip failSfx;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateCookieText();
        isGameOver = false;

        audioSource = GetComponent<AudioSource>();
    }

    public int GetRandomCookieReward()
    {
        return Random.Range(minCookies, maxCookies + 1);
    }

    public void AddCookies(int amount)
    {
        totalCookies += amount;
        UpdateCookieText();
    }

    private void UpdateCookieText()
    {
        if (cookieText != null)
            cookieText.text = $"{totalCookies}";
    }

    public int GetTotalCookies()
    {
        return totalCookies;
    }

    public bool TrySpendCookies(int amount)
    {
        if (amount <= 0) return true;
        if (totalCookies >= amount)
        {
            totalCookies -= amount;
            UpdateCookieText();
            return true;
        }
        return false;
    }

    public void CashIn()
    {
        HighScoreData highScoreData;
        string filePath = Path.Combine(Application.persistentDataPath, "highscore.json");

        if (File.Exists(filePath))
        {
            highScoreData = JsonUtility.FromJson<HighScoreData>(File.ReadAllText(filePath));
            if (totalCookies > highScoreData.highScore)
            {
                highScoreData.highScore = totalCookies;
            }
        }
        else
        {
            highScoreData = new HighScoreData { highScore = 0 };
        }

        string json = JsonUtility.ToJson(highScoreData, true);
        File.WriteAllText(filePath, json);

        SceneManager.LoadScene("Menu");
    }

    public void GameOver()
    {
        isGameOver = true;
        StartCoroutine(HandleGameOverAudio());
    }

    private IEnumerator HandleGameOverAudio()
    {
        if (audioSource != null) audioSource.volume = 0f;
        yield return new WaitForSeconds(gameOverWaitDuration);
        SceneManager.LoadScene("Menu");
    }

    public void IncreaseCookieReward()
    {
        minCookies++;
        maxCookies++;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void FlashCookieText(Color flashColor, int flashes = 3, float halfPeriod = 0.12f, AudioClip clip = null)
    {
        if (cookieText == null) return;
        StartCoroutine(FlashCookieTextRoutine(flashColor, flashes, halfPeriod, clip));
    }

    private IEnumerator FlashCookieTextRoutine(Color flashColor, int flashes, float halfPeriod, AudioClip clip)
    {
        Color original = cookieText.color;
        if (clip != null) PlaySfx(clip);

        for (int i = 0; i < flashes; i++)
        {
            cookieText.color = flashColor;
            yield return new WaitForSeconds(halfPeriod);
            cookieText.color = original;
            yield return new WaitForSeconds(halfPeriod);
        }

        cookieText.color = original;
    }
}

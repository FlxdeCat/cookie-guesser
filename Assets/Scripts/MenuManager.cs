using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Collections;

[System.Serializable]
public class HighScoreData
{
    public int highScore;
}

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;
    public Button playButton;
    public Button exitButton;

    public TextMeshProUGUI loadingText;

    private string filePath;
    private HighScoreData highScoreData;
    private AudioSource audioSource;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.1f;

        filePath = Path.Combine(Application.persistentDataPath, "highscore.json");
        LoadHighScore();
        UpdateHighScoreText();

        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
            playButton.onClick.AddListener(PlayGame);
        }

        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.AddListener(ExitGame);
        }

        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
    }

    void LoadHighScore()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            highScoreData = JsonUtility.FromJson<HighScoreData>(json);
        }
        else
        {
            highScoreData = new HighScoreData { highScore = 0 };
            SaveHighScore();
        }
    }

    void SaveHighScore()
    {
        string json = JsonUtility.ToJson(highScoreData, true);
        File.WriteAllText(filePath, json);
    }

    void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScoreData.highScore}";
        }
    }

    public void PlayGame()
    {
        if (loadingText != null)
        {
            playButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(true);
            audioSource.volume = 0.05f;
            StartCoroutine(LoadSceneWithAnimation("Game"));
        }
        else
        {
            SceneManager.LoadScene("Game");
        }
    }

    IEnumerator LoadSceneWithAnimation(string sceneName)
    {
        if (loadingText != null)
        {
            string baseText = "Loading";
            int dotCount = 0;

            // Start async loading
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                // Animate dots
                dotCount = (dotCount + 1) % 4; // cycles through 0,1,2,3
                loadingText.text = baseText + new string('.', dotCount);
                yield return new WaitForSeconds(0.5f);

                // Activate scene when finished
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }
        }
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

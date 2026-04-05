using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score = 0;

    [Header("HUD")]
    public Text scoreText; // Text hiện KILLS lúc chơi (Legacy Text)

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text highScoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f;
    }

    void Start()
    {
        UpdateScoreUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "KILLS: " + score;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            // Đưa panel lên trên cùng rồi hiện
            gameOverPanel.transform.SetAsLastSibling();
            gameOverPanel.SetActive(true);

            int currentHighscore = PlayerPrefs.GetInt("HighScore", 0);
            if (score > currentHighscore)
            {
                currentHighscore = score;
                PlayerPrefs.SetInt("HighScore", currentHighscore);
                PlayerPrefs.Save();
            }

            if (finalScoreText != null)  finalScoreText.text  = "SCORE: " + score;
            if (highScoreText != null)   highScoreText.text   = "HIGHSCORE: " + currentHighscore;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

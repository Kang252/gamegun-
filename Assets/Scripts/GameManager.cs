using UnityEngine;
using TMPro; // Khai báo sử dụng thư viện TextMeshPro
using UnityEngine.SceneManagement; // Dùng để Restart game

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score = 0;
    public TMP_Text scoreText; // Text hiện KILLS lúc chơi
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel; // Kéo thả cái Panel Game Over vào đây
    public TMP_Text finalScoreText; // Text hiện điểm số cuối cùng

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Reset lại thời gian nếu game đang bị đứng
        Time.timeScale = 1f;
    }

    void Start()
    {
        UpdateScoreUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // Giấu màn hình GameOver lúc đầu
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

    // Hàm gọi khi thua
    public void GameOver()
    {
        Time.timeScale = 0f; // Dừng thời gian (Mọi thứ đứng im)
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Hiện bảng điểm
            if (finalScoreText != null) finalScoreText.text = "SCORE: " + score;
        }
    }

    // Hàm gắn vào nút "PLAY AGAIN"
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

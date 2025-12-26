using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

// 游戏模式选择
public enum GameMode
{
    None, 
    Standard,
    Endless  
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameRunning = false;
    private GameObject player;

    public GameMode currentGameMode;
    public GameObject modeSelectUI;
    public GameObject gameOverUI; 
    public GameObject victoryUI; 
    public TextMeshProUGUI countdownText;  
    public TextMeshProUGUI scoreText;  
    public TextMeshProUGUI finalScoreText; 

    private float totalGameTime = 120f;
    private float remainingTime; 

    private int currentScore = 0; 
    public float scorePerSecond = 1f; 
    private float scoreTimer = 0f;
    private Vector3 lastPlayerPos;

    // 开始游戏前准备
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameOverUI.SetActive(false);
        victoryUI.SetActive(false);
        modeSelectUI.SetActive(true);

        if (scoreText != null) scoreText.text = "";
        if (finalScoreText != null) finalScoreText.text = "";
        if (countdownText != null) countdownText.text = "";

        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (!isGameRunning) return;

        if (currentGameMode == GameMode.Standard)
        {
            UpdateCountdown();
        }
        else if (currentGameMode == GameMode.Endless)
        {
            UpdateScore();
        }
    }

    // 标准故事模式
    public void StartStandardMode()
    {
        currentGameMode = GameMode.Standard;
        StartGame();
        remainingTime = totalGameTime;
        countdownText.text = $"Time:{Mathf.Floor(remainingTime / 60)}:{(remainingTime % 60):00}";
    }

    // 无尽模式
    public void StartEndlessMode()
    {
        currentGameMode = GameMode.Endless;
        StartGame();
        if (player != null)
        {
            lastPlayerPos = player.transform.position;
            currentScore = 0;
            scoreText.text = $"Score:{currentScore}";
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }    

    // 开始游戏
    private void StartGame()
    {
        isGameRunning = true;
        modeSelectUI.SetActive(false);
        Time.timeScale = 1f;
    }

    // 故事模式倒计时
    private void UpdateCountdown()
    {
        remainingTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        countdownText.text = $"Time:{minutes:00}:{seconds:00}";

        if (remainingTime <= 0)
        {
            TriggerVictory();
        }
    }

    // 触发胜利条件
    public void TriggerVictory()
    {
        isGameRunning = false;
        Time.timeScale = 0f;
        victoryUI.SetActive(true);
        countdownText.text = "";
    }    

    // 无尽模式更新分数
    private void UpdateScore()
    {
        scoreTimer += Time.deltaTime;
        if (scoreTimer >= 1f / scorePerSecond)
        {
            currentScore += 1;
            scoreTimer -= 1f / scorePerSecond;
            scoreText.text = $"Score:{currentScore}";
            Debug.Log("Update Score:" + currentScore);
        }
    }


    // 触发GameOver
    public void TriggerGameOver()
    {
        if (!isGameRunning) return;

        isGameRunning = false;
        Time.timeScale = 0f;
        gameOverUI.SetActive(true);

        if (currentGameMode == GameMode.Endless)
        {
            finalScoreText.text = $"Final Score:{currentScore}";
            scoreText.text = "";
        }
    }

    // 重新开始游戏
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 返回模式选择
    public void BackToModeSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueToNextPlot()
    {
        Time.timeScale = 1f;
        victoryUI.SetActive(false);
        Debug.Log("Not found!");
        Invoke("BackToModeSelect", 2f);
    }    
}

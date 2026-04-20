using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI Reference")]
    public WinPanelUI winPanel;

    public static int currentLevel = 1;
    public static int targetScore = 250;
    public static int totalMoves = 25;
    public static int playerCoins = 0;
    public static int bombCount = 0;

    [Header("UI")]
    public TMP_Text movesText;

    [Header("Difficulty Balance")]
    [Tooltip("Level 1 için başlangıç hamle sayısı")]
    public int baseMoves = 22;

    [Tooltip("Hedef puan her 1000 arttığında eklenecek ekstra hamle miktarı")]
    public float movesPerThousandScore = 5f;

    [Tooltip("Her levelde hedef puan artış oranı (örneğin 1.25 = %25 artış)")]
    public float scoreGrowthRate = 1.25f;

    [Tooltip("Hamle başına kazanılacak temel coin miktarı")]
    public int baseCoinPerMove = 12;

    public int currentScore { get; private set; }
    public int remainingMoves { get; private set; }
    public bool isGameActive { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        playerCoins = PlayerPrefs.GetInt("Coins", 0);
        currentLevel = PlayerPrefs.GetInt("StartLevel", 1);

        RecalculateLevelParameters();
        StartLevel();
    }

    void RecalculateLevelParameters()
    {
        targetScore = Mathf.RoundToInt(250 * Mathf.Pow(scoreGrowthRate, currentLevel - 1));

        totalMoves = baseMoves + Mathf.RoundToInt((targetScore / 1000f) * movesPerThousandScore);

        float growthFactor = 1f + (currentLevel * 0.0155f);

        totalMoves = Mathf.RoundToInt(totalMoves * growthFactor);

        totalMoves = Mathf.Clamp(totalMoves, 16, 115);

        totalMoves = Mathf.RoundToInt(totalMoves);
    }


    public void StartLevel()
    {
        currentScore = 0;
        remainingMoves = totalMoves;
        isGameActive = true;

        if (winPanel != null)
            winPanel.panelRoot.SetActive(false);

        UpdateMovesUI();
    }

    void UpdateMovesUI()
    {
        if (movesText == null) return;

        string movesWord = "Moves";
        if (LanguageManager.Instance != null)
            movesWord = LanguageManager.Instance.Get("Moves");

        if (ShopManager.Instance != null && ShopManager.Instance.IsUnlimitedMovesActive())
            movesText.text = $"{movesWord}: ∞";
        else
            movesText.text = $"{movesWord}: {remainingMoves}";
    }

    public void AddScore(int points)
    {
        currentScore += points;
        if (currentScore >= targetScore)
            WinLevel();
    }

    public void UseMove()
    {
        if (ShopManager.Instance != null && ShopManager.Instance.IsUnlimitedMovesActive())
        {
            UpdateMovesUI();
            return;
        }

        remainingMoves--;
        UpdateMovesUI();

        if (remainingMoves <= 0 && currentScore < targetScore)
            GameOver();
    }

    public void ConsumeBomb()
    {
        if (bombCount > 0)
            bombCount--;
    }

    int CalculateStars()
    {
        float scoreRatio = (float)currentScore / targetScore;

        if (scoreRatio >= 1.0f) return 3;      
        if (scoreRatio >= 0.75f) return 2;     
        if (scoreRatio >= 0.5f) return 1;      
        return 0;                              
    }

    void WinLevel()
    {
        if (!isGameActive) return;
        isGameActive = false;

        int baseCoins = Mathf.RoundToInt(remainingMoves * baseCoinPerMove * (1 + (currentLevel * 0.05f)));
        bool doubleActive = ShopManager.Instance != null && ShopManager.Instance.IsDoubleCoinsActive();
        int finalCoins = doubleActive ? baseCoins * 2 : baseCoins;

        int stars = CalculateStars();
        winPanel?.Show(stars);

        if (winPanel != null && winPanel.coinEarnedText != null)
        {
            string label = LanguageManager.Instance != null
                ? LanguageManager.Instance.Get("Kazanılan Coin:")
                : "Kazanılan Coin:";
            winPanel.coinEarnedText.text = label + " " + finalCoins;
        }

        int coins = PlayerPrefs.GetInt("Coins", 0);
        coins += finalCoins;
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();

        PlayerPrefs.SetInt("LastLevel", currentLevel + 1);
        PlayerPrefs.Save();
    }

    public void PrepareNextLevel()
    {
        currentLevel++;
        PlayerPrefs.SetInt("StartLevel", currentLevel);
        PlayerPrefs.SetInt("LastLevel", currentLevel);
        PlayerPrefs.Save();

        RecalculateLevelParameters();
        SceneManager.LoadScene("Level1");
    }

    void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;

        if (LifeManager.Instance != null)
            LifeManager.Instance.UseLife();

        if (LifeManager.Instance != null && !LifeManager.Instance.HasLife())
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        Invoke(nameof(ReloadSameLevel), 1.2f);
    }

    void ReloadSameLevel()
    {
        SceneManager.LoadScene("Level1");
    }
}

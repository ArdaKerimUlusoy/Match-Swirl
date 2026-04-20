using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Texts")]
    public TMP_Text scoreText;
    public TMP_Text movesText;
    public TMP_Text levelText;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (LevelManager.Instance == null || LanguageManager.Instance == null) return;

        if (scoreText != null)
            scoreText.text = $"{LevelManager.Instance.currentScore} / {LevelManager.targetScore}";

        if (levelText != null)
        {
            string levelWord = LanguageManager.Instance.Get("Level");
            levelText.text = $"{levelWord} {LevelManager.currentLevel}";
        }
    }
}
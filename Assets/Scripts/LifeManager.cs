using UnityEngine;
using System;
using TMPro;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;

    [Header("Life Settings")]
    public int maxLives = 3;
    public int currentLives;
    public float refillMinutes = 5f;

    [Header("UI References")]
    [Tooltip("Geri sayım göstergesi (örneğin 'Next life in: 04:59')")]
    public TMP_Text timerText;

    private const string LIVES_KEY = "Lives";
    private const string TIME_KEY = "LifeTime";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLives();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        RefillLifeIfNeeded();
        UpdateTimerUI();
    }

    void LoadLives()
    {
        currentLives = PlayerPrefs.GetInt(LIVES_KEY, maxLives);

        if (!PlayerPrefs.HasKey(TIME_KEY))
            PlayerPrefs.SetString(TIME_KEY, DateTime.Now.ToString());
    }

    void RefillLifeIfNeeded()
    {
        if (currentLives >= maxLives) return;

        DateTime lastTime = DateTime.Parse(PlayerPrefs.GetString(TIME_KEY));
        double minutesPassed = (DateTime.Now - lastTime).TotalMinutes;

        if (minutesPassed >= refillMinutes)
        {
            int livesToAdd = Mathf.FloorToInt((float)(minutesPassed / refillMinutes));
            currentLives = Mathf.Min(currentLives + livesToAdd, maxLives);

            PlayerPrefs.SetInt(LIVES_KEY, currentLives);
            PlayerPrefs.SetString(TIME_KEY, DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        if (currentLives >= maxLives)
        {
            timerText.text = "Full";
            return;
        }

        DateTime lastTime = DateTime.Parse(PlayerPrefs.GetString(TIME_KEY));
        double secondsPassed = (DateTime.Now - lastTime).TotalSeconds;
        double totalRefillSeconds = refillMinutes * 60;
        double remaining = totalRefillSeconds - secondsPassed;

        if (remaining < 0) remaining = 0;

        TimeSpan t = TimeSpan.FromSeconds(remaining);
        timerText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
    }

    public bool HasLife()
    {
        return currentLives > 0;
    }

    public void UseLife()
    {
        if (currentLives <= 0) return;

        currentLives--;
        PlayerPrefs.SetInt(LIVES_KEY, currentLives);
        PlayerPrefs.SetString(TIME_KEY, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    public bool IsFull()
    {
        return currentLives >= maxLives;
    }

    public void AddLife(int amount)
    {
        if (IsFull()) return;

        currentLives = Mathf.Min(maxLives, currentLives + amount);
        PlayerPrefs.SetInt(LIVES_KEY, currentLives);
        PlayerPrefs.SetString(TIME_KEY, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
}

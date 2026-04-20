using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Language")]
    public TMP_Dropdown languageDropdown;

    [Header("Lives UI")]
    public TMP_Text livesText;
    public Button playButton;

    [Header("Market")]
    public GameObject marketPanel;
    public TMP_Text coinsText;
    public Button marketButton;
    public TMP_Text coinsText2;
    public Image coinIcon;

    [Header("Market Timers")]
    public TMP_Text doubleCoinTimerText;
    public TMP_Text unlimitedMovesTimerText;

    [Header("Market Buttons")]
    public Button doubleCoinButton;
    public Button unlimitedMovesButton;
    public Button lifeButton; 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        if (languageDropdown != null)
            languageDropdown.value = PlayerPrefs.GetInt("Language", 0);

        UpdateLivesUI();
        UpdateCoinsUI();
        LoadSettings();

        ReconnectShopButtons();
    }

    private void Update()
    {
        UpdateLivesUI();
        UpdateMarketTimers();
    }

    void ReconnectShopButtons()
    {
        if (ShopManager.Instance == null)
        {
            return;
        }

        doubleCoinButton.onClick.RemoveAllListeners();
        unlimitedMovesButton.onClick.RemoveAllListeners();
        if (lifeButton != null)
            lifeButton.onClick.RemoveAllListeners();

        doubleCoinButton.onClick.AddListener(() => ShopManager.Instance.PurchaseDoubleCoins());
        unlimitedMovesButton.onClick.AddListener(() => ShopManager.Instance.PurchaseUnlimitedMoves());
        if (lifeButton != null)
            lifeButton.onClick.AddListener(() => ShopManager.Instance.PurchaseLife());
    }

    void UpdateMarketTimers()
    {
        if (ShopManager.Instance == null) return;

        if (ShopManager.Instance.IsDoubleCoinsActive())
        {
            TimeSpan t = ShopManager.Instance.GetDoubleRemaining();
            doubleCoinTimerText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
            doubleCoinButton.interactable = false;
        }
        else
        {
            doubleCoinTimerText.text = "Satın Al";
            doubleCoinButton.interactable = true;
        }

        if (ShopManager.Instance.IsUnlimitedMovesActive())
        {
            TimeSpan t = ShopManager.Instance.GetUnlimitedRemaining();
            unlimitedMovesTimerText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
            unlimitedMovesButton.interactable = false;
        }
        else
        {
            unlimitedMovesTimerText.text = "Satın Al";
            unlimitedMovesButton.interactable = true;
        }
    }

    public void OpenMarket()
    {
        mainMenuPanel.SetActive(false);
        if (marketPanel != null) marketPanel.SetActive(true);
        UpdateCoinsUI();
    }

    public void CloseMarket()
    {
        if (marketPanel != null) marketPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        UpdateCoinsUI();
    }

    public void UpdateCoinsUI()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        coinsText.text = coins.ToString();
        coinsText2.text = coins.ToString();
    }

    void UpdateLivesUI()
    {
        if (LifeManager.Instance == null) return;

        int current = LifeManager.Instance.currentLives;
        int max = LifeManager.Instance.maxLives;

        if (current >= max)
        {
            livesText.text = "FULL";
            playButton.interactable = true;
            return;
        }

        livesText.text = $"{current}/{max}";
        playButton.interactable = LifeManager.Instance.HasLife();
    }

    public void OnPlayPressed()
    {
        int level = PlayerPrefs.GetInt("LastLevel", 1);
        PlayerPrefs.SetInt("StartLevel", level);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Level1");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMusicVolume(value);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);

        if (BoardManager.Instance != null)
            BoardManager.Instance.SetSFXVolume(value);
    }

    public void OnLanguageChanged(int index)
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.ChangeLanguage(index);
    }

    void LoadSettings()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        musicSlider.value = music;
        sfxSlider.value = sfx;

        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMusicVolume(music);

        if (BoardManager.Instance != null)
            BoardManager.Instance.SetSFXVolume(sfx);
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsUIManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button soundToggleButton;

    [Header("Panel")]
    [SerializeField] private RectTransform settingsPanel;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.25f;

    [Header("Sound")]
    [SerializeField] private float defaultVolume = 0.7f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private bool soundOn = true;

    private const string VOLUME_KEY = "MASTER_VOLUME";

    private void Awake()
    {
        settingsPanel.localScale = Vector3.zero;
        settingsPanel.gameObject.SetActive(false);

        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
        AudioListener.volume = savedVolume;
        soundOn = savedVolume > 0f;

        settingsButton.onClick.AddListener(ToggleSettings);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        soundToggleButton.onClick.AddListener(ToggleSound);
    }

    private void ToggleSettings()
    {
        if (isAnimating) return;

        isOpen = !isOpen; 

        StopAllCoroutines();
        StartCoroutine(AnimatePanel(isOpen));
    }

    private IEnumerator AnimatePanel(bool open)
    {
        isAnimating = true;
        settingsPanel.gameObject.SetActive(true);

        Vector3 startScale = settingsPanel.localScale;
        Vector3 targetScale = open ? Vector3.one : Vector3.zero;

        float time = 0f;
        while (time < animDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / animDuration);

            settingsPanel.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        settingsPanel.localScale = targetScale;

        if (!open)
            settingsPanel.gameObject.SetActive(false);

        isAnimating = false; 
    }

    private void ToggleSound()
    {
        soundOn = !soundOn;

        if (soundOn)
        {
            float volume = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
            AudioListener.volume = volume;
        }
        else
        {
            AudioListener.volume = 0f;
        }
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

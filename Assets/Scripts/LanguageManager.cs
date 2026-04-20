using UnityEngine;
using System.Collections.Generic;
using System;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;
    public static Action OnLanguageChanged;

    public enum Language { English, Turkish, German }
    public Language currentLanguage = Language.English;

    private Dictionary<string, Dictionary<Language, string>> dictionary =
        new Dictionary<string, Dictionary<Language, string>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupDictionary();
        currentLanguage = (Language)PlayerPrefs.GetInt("Language", 0);
    }

    void SetupDictionary()
    {
        Add("Play", "Play", "Oyna", "Spielen");
        Add("Settings", "Settings", "Ayarlar", "Einstellungen");
        Add("Level", "Level", "Seviye", "Stufe");
        Add("Moves", "Moves", "Hamle", "Zug");
        Add("Market", "Market", "Market", "Markt");
        Add("Satýn Al", "Buy", "Satýn Al", "Kaufen");
        Add("New Game", "New Game", "Yeni Oyun", "Neues Spiel");
    }

    void Add(string key, string en, string tr, string de)
    {
        dictionary[key] = new Dictionary<Language, string>
        {
            { Language.English, en },
            { Language.Turkish, tr },
            { Language.German, de }
        };
    }

    public string Get(string key)
    {
        if (dictionary.ContainsKey(key))
            return dictionary[key][currentLanguage];

        return key;
    }
    public void ChangeLanguage(int index)
    {
        currentLanguage = (Language)index;

        PlayerPrefs.SetInt("Language", index);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }
}

using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public int costLife = 30;
    public int costDoubleCoins = 100;
    public int costUnlimitedMoves = 150;

    public float doubleCoinsMinutes = 15f;
    public float unlimitedMovesMinutes = 15f;

    private const string COINS_KEY = "Coins";
    private const string DOUBLE_EXPIRE_KEY = "DoubleCoinsExpire";
    private const string UNLIMITED_EXPIRE_KEY = "UnlimitedMovesExpire";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey(COINS_KEY))
            PlayerPrefs.SetInt(COINS_KEY, 0);

        if (LifeManager.Instance == null)
            LifeManager.Instance = FindObjectOfType<LifeManager>();
        if (MenuUIManager.Instance == null)
            MenuUIManager.Instance = FindObjectOfType<MenuUIManager>();
    }

    public int GetCoins() => PlayerPrefs.GetInt(COINS_KEY, 0);

    public void AddCoins(int amount)
    {
        int now = GetCoins();
        now += amount;
        PlayerPrefs.SetInt(COINS_KEY, now);
        PlayerPrefs.Save();
    }

    bool TrySpendCoins(int amount)
    {
        int coins = GetCoins();
        if (coins < amount)
        {
            return false;
        }

        coins -= amount;
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.Save();
        return true;
    }

    public void PurchaseLife()
    {
        if (LifeManager.Instance == null)
            LifeManager.Instance = FindObjectOfType<LifeManager>();
        if (MenuUIManager.Instance == null)
            MenuUIManager.Instance = FindObjectOfType<MenuUIManager>();

        if (LifeManager.Instance == null)
        {
            return;
        }

        if (LifeManager.Instance.IsFull())
        {
            return;
        }

        if (!TrySpendCoins(costLife)) return;

        LifeManager.Instance.AddLife(1);
        MenuUIManager.Instance?.UpdateCoinsUI();
    }

    public void PurchaseDoubleCoins()
    {
        if (!ReconnectIfNeeded()) return;
        if (IsDoubleCoinsActive()) return;
        if (!TrySpendCoins(costDoubleCoins)) return;

        DateTime expire = DateTime.Now.AddMinutes(doubleCoinsMinutes);
        PlayerPrefs.SetString(DOUBLE_EXPIRE_KEY, expire.ToString());
        PlayerPrefs.Save();

        MenuUIManager.Instance?.UpdateCoinsUI();
    }

    public void PurchaseUnlimitedMoves()
    {
        if (!ReconnectIfNeeded()) return;
        if (IsUnlimitedMovesActive()) return;
        if (!TrySpendCoins(costUnlimitedMoves)) return;

        DateTime expire = DateTime.Now.AddMinutes(unlimitedMovesMinutes);
        PlayerPrefs.SetString(UNLIMITED_EXPIRE_KEY, expire.ToString());
        PlayerPrefs.Save();

        MenuUIManager.Instance?.UpdateCoinsUI();
    }

    bool ReconnectIfNeeded()
    {
        if (LifeManager.Instance == null)
            LifeManager.Instance = FindObjectOfType<LifeManager>();
        if (MenuUIManager.Instance == null)
            MenuUIManager.Instance = FindObjectOfType<MenuUIManager>();

        return LifeManager.Instance != null;
    }

    public bool IsDoubleCoinsActive()
    {
        if (!PlayerPrefs.HasKey(DOUBLE_EXPIRE_KEY)) return false;
        if (!DateTime.TryParse(PlayerPrefs.GetString(DOUBLE_EXPIRE_KEY), out DateTime exp)) return false;
        return DateTime.Now < exp;
    }

    public bool IsUnlimitedMovesActive()
    {
        if (!PlayerPrefs.HasKey(UNLIMITED_EXPIRE_KEY)) return false;
        if (!DateTime.TryParse(PlayerPrefs.GetString(UNLIMITED_EXPIRE_KEY), out DateTime exp)) return false;
        return DateTime.Now < exp;
    }

    public TimeSpan GetDoubleRemaining()
    {
        if (!PlayerPrefs.HasKey(DOUBLE_EXPIRE_KEY)) return TimeSpan.Zero;
        if (!DateTime.TryParse(PlayerPrefs.GetString(DOUBLE_EXPIRE_KEY), out DateTime exp)) return TimeSpan.Zero;
        return exp - DateTime.Now;
    }

    public TimeSpan GetUnlimitedRemaining()
    {
        if (!PlayerPrefs.HasKey(UNLIMITED_EXPIRE_KEY)) return TimeSpan.Zero;
        if (!DateTime.TryParse(PlayerPrefs.GetString(UNLIMITED_EXPIRE_KEY), out DateTime exp)) return TimeSpan.Zero;
        return exp - DateTime.Now;
    }
}

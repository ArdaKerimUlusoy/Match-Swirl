using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGame : MonoBehaviour
{
    public void StartNewGame()
    {
        PlayerPrefs.DeleteKey("StartLevel");
        PlayerPrefs.DeleteKey("LastLevel");
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.DeleteKey("Lives");
        PlayerPrefs.DeleteKey("LifeTime");
        PlayerPrefs.DeleteKey("SFXVolume");
        PlayerPrefs.DeleteKey("MusicVolume");

        PlayerPrefs.DeleteKey("DoubleCoinsExpire");
        PlayerPrefs.DeleteKey("UnlimitedMovesExpire");

        LevelManager.bombCount = 0;
        LevelManager.playerCoins = 0;

        PlayerPrefs.SetInt("StartLevel", 1);
        PlayerPrefs.SetInt("LastLevel", 1);

        int defaultLives = 3;
        PlayerPrefs.SetInt("Lives", defaultLives);
        PlayerPrefs.SetString("LifeTime", System.DateTime.Now.ToString());

        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1");
    }
}

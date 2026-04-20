using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPanelUI : MonoBehaviour
{
    public GameObject panelRoot;
    public Image[] starImages;
    public Sprite fullStar;
    public Sprite emptyStar;
    public TMP_Text coinEarnedText;

    public void Show(int stars)
    {
        panelRoot.SetActive(true);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < stars)
                starImages[i].sprite = fullStar;
            else
                starImages[i].sprite = emptyStar;
        }
    }

    public void OnNextLevelClicked()
    {
        LevelManager.Instance.PrepareNextLevel();
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
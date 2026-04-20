using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string key;
    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        if (LanguageManager.Instance != null && textComponent != null)
        {
            textComponent.text = LanguageManager.Instance.Get(key);
        }
        else
        {
            Invoke("DelayedRefresh", 0.1f);
        }
    }

    void DelayedRefresh()
    {
        if (LanguageManager.Instance != null && textComponent != null)
            textComponent.text = LanguageManager.Instance.Get(key);
    }
}
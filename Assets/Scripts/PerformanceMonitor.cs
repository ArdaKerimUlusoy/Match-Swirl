using UnityEngine;
using UnityEngine.InputSystem;

public class PerformanceMonitor : MonoBehaviour
{
    private bool show = false;
    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Keyboard.current.hKey.wasPressedThisFrame)
            show = !show;
    }

    void OnGUI()
    {
        if (!show) return;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 22;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;

        GUIStyle shadowStyle = new GUIStyle(labelStyle);
        shadowStyle.normal.textColor = Color.black;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.75f));
        boxStyle.normal.textColor = Color.white;
        boxStyle.fontSize = 24;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.alignment = TextAnchor.UpperCenter;

        float panelWidth = 320;
        float panelHeight = 170;
        Rect panelRect = new Rect(
            20,
            Screen.height - panelHeight - 20,
            panelWidth,
            panelHeight
        );

        GUI.Box(panelRect, "PERFORMANCE", boxStyle);

        //Performans hesaplamalarýný buradan yaptýrýyorum! (Unutma)
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        long mem = System.GC.GetTotalMemory(false) / (1024 * 1024);

        string stats =
            $"FPS: {fps:0.0} ({msec:0.0} ms)\n" +
            $"MEMORY: {mem} MB\n\n" +
            "H : Hide Panel";

        Rect textRect = new Rect(panelRect.x + 20, panelRect.y + 50, panelWidth - 40, panelHeight);

        GUI.Label(new Rect(textRect.x + 1, textRect.y + 1, textRect.width, textRect.height), stats, shadowStyle);
        GUI.Label(textRect, stats, labelStyle);
    }

    Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}

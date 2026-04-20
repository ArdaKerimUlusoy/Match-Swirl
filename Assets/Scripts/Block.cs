using UnityEngine;

public class Block : MonoBehaviour
{
    public int row, column, colorIndex;
    public bool isBomb = false;
    public bool isLightning = false;

    private SpriteRenderer sr;
    private Sprite[] colorSet;

    public void Setup(int r, int c, int colorIdx)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        row = r;
        column = c;
        colorIndex = colorIdx;

        isBomb = false;
        isLightning = false;

        sr.color = Color.white; 
        ApplySprite();
    }

    public void ApplySprite()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        sr.color = Color.white; 

        if (isBomb)
        {
            sr.sprite = BoardManager.Instance.bombSprite;
            return;
        }

        if (isLightning)
        {
            sr.sprite = BoardManager.Instance.lightningSprite;
            return;
        }

        switch (colorIndex)
        {
            case 0: colorSet = BoardManager.Instance.purpleSprites; break;
            case 1: colorSet = BoardManager.Instance.greenSprites; break;
            case 2: colorSet = BoardManager.Instance.yellowSprites; break;
            case 3: colorSet = BoardManager.Instance.blueSprites; break;
            case 4: colorSet = BoardManager.Instance.redSprites; break;
            case 5: colorSet = BoardManager.Instance.pinkSprites; break;
        }

        sr.sprite = colorSet[0];
    }

    public void UpdateVisual(int count, int A, int B, int C)
    {
        if (isBomb || isLightning) return;

        if (count > C) sr.sprite = colorSet[3];
        else if (count > B) sr.sprite = colorSet[2];
        else if (count > A) sr.sprite = colorSet[1];
        else sr.sprite = colorSet[0];
    }

    public void Highlight(bool on)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.color = on ? Color.gray : Color.white;
    }

    public void MoveTo(Vector3 target)
    {
        StopAllCoroutines();
        StartCoroutine(Animate(target));
    }

    System.Collections.IEnumerator Animate(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
    }
}

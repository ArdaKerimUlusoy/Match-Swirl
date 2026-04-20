using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Settings")]
    public int rows = 8;
    public int columns = 8;
    public float cellSize = 1.1f;
    public GameObject blockPrefab;

    [Header("Effects")]
    [Tooltip("Assets klasöründen hazırladığın Particle System prefabını buraya sürükle.")]
    public ParticleSystem explosionPrefab;

    private Stack<ParticleSystem> fxPool = new Stack<ParticleSystem>();
    private float fxLifetime = 1.2f;

    [Header("PDF Rules (Thresholds)")]
    public int A = 4;
    public int B = 7;
    public int C = 9;

    [Header("Sprites")]
    public Sprite[] purpleSprites, greenSprites, yellowSprites, blueSprites, redSprites, pinkSprites;
    public Sprite bombSprite;

    [Header("Special Sprites")]
    public Sprite lightningSprite;

    [Header("Audio Clips")]
    public AudioSource audioSource;
    public AudioClip breakClip;
    public AudioClip bombClip;
    public AudioClip fallClip;
    public AudioClip swapClip;
    public AudioClip lightningClip; 

    [Header("Special Effects")]
    public ParticleSystem bombFXPrefab;

    [Range(0f, 1f)]
    public float musicVolume = 0.4f;

    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;


    private Block[,] grid;
    private Stack<Block> blockPool = new Stack<Block>();
    private Block selectedBlock;
    private Camera mainCam;
    private bool isBombMode = false;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        if (audioSource != null)
            audioSource.volume = sfxVolume;
    }


    private void Start()
    {
        GenerateBoard();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectClickFromScreen(Mouse.current.position.ReadValue());
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            DetectClickFromScreen(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }

        if (Keyboard.current.bKey.wasPressedThisFrame && LevelManager.bombCount > 0)
            isBombMode = !isBombMode;
    }

    void PlayBombFX(Vector3 pos, Color color)
    {
        if (bombFXPrefab == null) return;

        ParticleSystem fx = Instantiate(bombFXPrefab, pos, Quaternion.identity);
        var main = fx.main;
        main.startColor = color;
        fx.Play();
        Destroy(fx.gameObject, 1.2f);
    }
    void PlaySound(AudioClip clip, float volumeMultiplier = 1f, float pitchRandom = 0.05f)
    {
        if (audioSource == null || clip == null) return;
        if (sfxVolume <= 0f) return; 

        audioSource.pitch = Random.Range(1f - pitchRandom, 1f + pitchRandom);
        audioSource.PlayOneShot(clip, volumeMultiplier);
        audioSource.pitch = 1f;
    }
    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        if (audioSource != null)
            audioSource.volume = sfxVolume; 
    }

    ParticleSystem GetFXFromPool()
    {
        if (explosionPrefab == null) return null;
        if (fxPool.Count > 0)
        {
            var fx = fxPool.Pop();
            fx.gameObject.SetActive(true);
            return fx;
        }
        var inst = Instantiate(explosionPrefab, transform);
        return inst;
    }

    void ReturnFXToPool(ParticleSystem fx)
    {
        if (fx == null) return;
        fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fx.gameObject.SetActive(false);
        fxPool.Push(fx);
    }

    void PlayExplosionEffect(Block b)
    {
        if (explosionPrefab == null || b == null) return;

        ParticleSystem fx = GetFXFromPool();
        if (fx == null) return;

        fx.transform.position = b.transform.position;
        var main = fx.main;
        switch (b.colorIndex)
        {
            case 0: main.startColor = new Color(0.6f, 0.2f, 0.8f); break;
            case 1: main.startColor = Color.green; break;
            case 2: main.startColor = Color.yellow; break;
            case 3: main.startColor = Color.blue; break;
            case 4: main.startColor = Color.red; break;
            case 5: main.startColor = new Color(1f, 0.4f, 0.7f); break;
            default: main.startColor = Color.white; break;
        }

        fx.Play();
        StartCoroutine(ReturnFXAfterDelay(fx, fxLifetime));
    }

    IEnumerator ReturnFXAfterDelay(ParticleSystem fx, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnFXToPool(fx);
    }

    Vector3 GetTargetPosition(int r, int c)
    {
        float offsetX = (columns - 1) * cellSize / 2f;
        float offsetY = (rows - 1) * cellSize / 2f;
        return new Vector3((c * cellSize) - offsetX, (-r * cellSize) + offsetY, 0);
    }

    public Block GetBlockFromPool()
    {
        if (blockPool.Count > 0)
        {
            Block b = blockPool.Pop();
            b.gameObject.SetActive(true);
            return b;
        }
        return Instantiate(blockPrefab, transform).GetComponent<Block>();
    }

    public void ReturnToPool(Block b)
    {
        if (b == null) return;

        b.StopAllCoroutines();
        b.isBomb = false;
        b.isLightning = false;

        var sr = b.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.white;

        b.gameObject.SetActive(false);
        blockPool.Push(b);
    }


    void GenerateBoard()
    {
        grid = new Block[rows, columns];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                CreateBlock(r, c);
            }
        }
        UpdateAllIcons();
        CheckDeadlock();
    }

    void CreateBlock(int r, int c)
    {
        Block b = GetBlockFromPool();
        b.transform.localPosition = GetTargetPosition(r, c);
        b.Setup(r, c, Random.Range(0, 6));
        grid[r, c] = b;
    }

    void DetectClick()
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.isGameActive) return;
        Vector2 worldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
            HandleLogic(hit.collider.GetComponent<Block>());
    }

    void DetectClickFromScreen(Vector2 screenPos)
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.isGameActive)
            return;

        Vector2 worldPos = mainCam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Block b = hit.collider.GetComponent<Block>();
            if (b != null)
                HandleLogic(b);
        }
    }

    void HandleLogic(Block clicked)
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.isGameActive) return;

        if (isBombMode)
        {
            ExecuteAreaBlast(clicked.row, clicked.column);
            isBombMode = false;
            LevelManager.Instance.ConsumeBomb();
            return;
        }

        if (clicked.isBomb)
        {
            ExecuteAreaBlast(clicked.row, clicked.column);
            return;
        }

        if (clicked.isLightning)
        {
            clicked.Highlight(true);
            StartCoroutine(HandleLightningSwipe(clicked));
            return;
        }

        List<Block> group = GetConnectedBlocks(clicked);

        if (group.Count >= 2)
        {
            if (selectedBlock != null)
            {
                selectedBlock.Highlight(false);
                selectedBlock = null;
            }

            ProcessBlast(group, clicked);
            LevelManager.Instance.UseMove();
        }
        else if (selectedBlock == null)
        {
            selectedBlock = clicked;
            clicked.Highlight(true);
        }
        else
        {
            if (Mathf.Abs(selectedBlock.row - clicked.row) +
                Mathf.Abs(selectedBlock.column - clicked.column) == 1)
            {
                StartCoroutine(TrySwap(selectedBlock, clicked));
            }
            selectedBlock.Highlight(false);
            selectedBlock = null;
        }
    }


    void ExecuteAreaBlast(int row, int col)
    {
        PlaySound(bombClip, 0.7f);
        PlayBombFX(GetTargetPosition(row, col), Color.white);

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r >= 0 && r < rows && c >= 0 && c < columns && grid[r, c] != null)
                {
                    LevelManager.Instance.AddScore(25);
                    PlayExplosionEffect(grid[r, c]);
                    ReturnToPool(grid[r, c]);
                    grid[r, c] = null;
                }
            }
        }
        StartCoroutine(Collapse(row, col));
    }


    IEnumerator TrySwap(Block a, Block b)
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.isGameActive)
            yield break;

        PlaySound(swapClip, 0.35f, 0.08f);
        SwapData(a, b);

        LevelManager.Instance.UseMove(); 

        yield return new WaitForSeconds(0.22f);

        bool matchedA = GetConnectedBlocks(a).Count >= 2;
        bool matchedB = GetConnectedBlocks(b).Count >= 2;

        if (matchedA || matchedB)
        {
            if (matchedA) ProcessBlast(GetConnectedBlocks(a), a);
            if (matchedB) ProcessBlast(GetConnectedBlocks(b), b);
        }
        else
        {
            SwapData(a, b);
        }
    }

    void SwapData(Block a, Block b)
    {
        int rA = a.row; int cA = a.column;
        grid[a.row, a.column] = b;
        grid[b.row, b.column] = a;
        a.row = b.row; a.column = b.column;
        b.row = rA; b.column = cA;
        a.MoveTo(GetTargetPosition(a.row, a.column));
        b.MoveTo(GetTargetPosition(b.row, b.column));
    }

    void ProcessBlast(List<Block> group, Block origin)
    {
        if (LevelManager.Instance == null) return;
        if (!LevelManager.Instance.isGameActive) return;

        PlaySound(breakClip, 0.55f);
        LevelManager.Instance.AddScore(group.Count * 10);

        bool createBomb = group.Count >= 5;

        bool createLightning = false;
        if (group.Count >= 4 && !createBomb)
        {
            createLightning = Random.value <= 0.75f; 
        }

        foreach (var b in group)
        {
            if (b == origin)
            {
                if (createBomb)
                {
                    b.isBomb = true;
                    b.isLightning = false;
                    b.ApplySprite();
                    continue;
                }
                else if (createLightning)
                {
                    b.isLightning = true;
                    b.isBomb = false;
                    b.ApplySprite();
                    continue;
                }
            }

            PlayExplosionEffect(b);
            grid[b.row, b.column] = null;
            ReturnToPool(b);
        }

        StartCoroutine(Collapse(origin.row, origin.column));
    }


    IEnumerator Collapse(int centerRow = -1, int centerCol = -1)
    {
        yield return new WaitForSeconds(0.09f);

        for (int c = 0; c < columns; c++)
        {
            int empty = 0;
            for (int r = rows - 1; r >= 0; r--)
            {
                if (grid[r, c] == null) empty++;
                else if (empty > 0)
                {
                    Block b = grid[r, c];
                    if (b != null && !b.gameObject.activeInHierarchy)
                        b.gameObject.SetActive(true);

                    grid[r + empty, c] = b;
                    grid[r, c] = null;
                    b.row = r + empty;
                    b.MoveTo(GetTargetPosition(b.row, b.column));
                }
            }

            for (int i = 0; i < empty; i++)
            {
                Block b = GetBlockFromPool();
                Vector3 spawnPos = GetTargetPosition(0, c);
                spawnPos.y += 3f + (i * 0.5f);
                b.transform.localPosition = spawnPos;
                b.Setup(i, c, Random.Range(0, 6));
                grid[i, c] = b;
                b.MoveTo(GetTargetPosition(i, c));
            }

            if (fallClip != null && empty > 0)
                PlaySound(fallClip, 0.35f, 0.08f);
        }

        yield return new WaitForSeconds(0.18f);
        UpdateAllIcons();
        CheckDeadlock();
        StartCoroutine(CheckAutoMatches(centerRow, centerCol));
    }

    void UpdateAllIcons()
    {
        foreach (var b in grid)
            if (b != null && !b.isBomb && !b.isLightning)
                b.UpdateVisual(GetConnectedBlocks(b).Count, A, B, C);
    }

    void CheckDeadlock() { if (!HasAnyValidGroup()) Shuffle(); }

    bool HasAnyValidGroup()
    {
        foreach (var b in grid)
            if (b != null && !b.isBomb && !b.isLightning && GetConnectedBlocks(b).Count >= 2)
                return true;
        return false;
    }

    void Shuffle()
    {
        List<int> colors = new List<int>();
        foreach (var b in grid) if (b != null && !b.isBomb && !b.isLightning) colors.Add(b.colorIndex);
        for (int i = 0; i < colors.Count; i++)
        {
            int r = Random.Range(i, colors.Count);
            int tmp = colors[i]; colors[i] = colors[r]; colors[r] = tmp;
        }
        int idx = 0;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                if (grid[r, c] != null && !grid[r, c].isBomb && !grid[r, c].isLightning)
                {
                    grid[r, c].colorIndex = colors[idx++];
                    grid[r, c].ApplySprite();
                }
        if (!HasAnyValidGroup()) ForceOneValidGroup();
        UpdateAllIcons();
    }

    void ForceOneValidGroup()
    {
        int r = Random.Range(0, rows); int c = Random.Range(0, columns);
        Block baseBlock = grid[r, c];
        if (baseBlock == null || baseBlock.isBomb || baseBlock.isLightning) return;
        if (c + 1 < columns && grid[r, c + 1] != null && !grid[r, c + 1].isBomb && !grid[r, c + 1].isLightning) { grid[r, c + 1].colorIndex = baseBlock.colorIndex; grid[r, c + 1].ApplySprite(); }
        else if (r + 1 < rows && grid[r + 1, c] != null && !grid[r + 1, c].isBomb && !grid[r + 1, c].isLightning) { grid[r + 1, c].colorIndex = baseBlock.colorIndex; grid[r + 1, c].ApplySprite(); }
    }
    IEnumerator CheckAutoMatches(int centerRow, int centerCol)
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.isGameActive)
            yield break;

        yield return new WaitForSeconds(0.08f);

        if (centerRow < 0 || centerCol < 0)
            yield break;

        List<List<Block>> groups = FindLocalGroups(centerRow, centerCol, 1);

        bool anyAuto = false;
        foreach (var g in groups)
        {
            if (!LevelManager.Instance.isGameActive)
                yield break;

            if (g.Count >= 4)
            {
                anyAuto = true;
                ProcessBlast(g, g[0]);
                yield return new WaitForSeconds(0.2f);
            }
        }

        if (anyAuto)
            SpawnLightningBlock();
    }

    List<List<Block>> FindLocalGroups(int centerRow, int centerCol, int radius)
    {
        List<List<Block>> result = new List<List<Block>>();
        bool[,] visited = new bool[rows, columns];

        for (int r = Mathf.Max(0, centerRow - radius); r <= Mathf.Min(rows - 1, centerRow + radius); r++)
        {
            for (int c = Mathf.Max(0, centerCol - radius); c <= Mathf.Min(columns - 1, centerCol + radius); c++)
            {
                if (visited[r, c]) continue;
                Block start = grid[r, c];
                if (start == null || start.isBomb || start.isLightning) continue;

                List<Block> group = GetConnectedBlocks(start);
                if (group.Count >= 4)
                {
                    result.Add(group);
                    foreach (var b in group)
                        visited[b.row, b.column] = true;
                }
            }
        }

        return result;
    }

    void SpawnLightningBlock()
    {
        List<(int r, int c)> candidates = new List<(int, int)>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                if (grid[r, c] != null && !grid[r, c].isBomb && !grid[r, c].isLightning)
                    candidates.Add((r, c));

        if (candidates.Count == 0) return;

        var pick = candidates[Random.Range(0, candidates.Count)];
        Block b = grid[pick.r, pick.c];
        if (b == null) return;

        b.isLightning = true;
        b.isBomb = false;
        var sr = b.GetComponent<SpriteRenderer>();
        if (lightningSprite != null) sr.sprite = lightningSprite;
        sr.color = Color.white;
    }

    IEnumerator HandleLightningSwipe(Block lightningBlock)
    {
        if (lightningBlock == null) yield break;

        Vector2 start;

        if (Touchscreen.current != null)
            start = Touchscreen.current.primaryTouch.position.ReadValue();
        else
            start = Mouse.current.position.ReadValue();

        yield return new WaitUntil(() =>
            (Touchscreen.current != null &&
             !Touchscreen.current.primaryTouch.press.isPressed) ||
            (Mouse.current != null &&
             Mouse.current.leftButton.wasReleasedThisFrame)
        );

        Vector2 end;

        if (Touchscreen.current != null)
            end = Touchscreen.current.primaryTouch.position.ReadValue();
        else
            end = Mouse.current.position.ReadValue();

        Vector2 dir = end - start;
        if (dir.magnitude < 30f)
        {
            lightningBlock.Highlight(false);
            yield break;
        }

        bool horizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        PlaySound(lightningClip, 0.8f);

        int r = lightningBlock.row;
        int c = lightningBlock.column;

        if (horizontal)
            ClearRow(r);
        else
            ClearColumn(c);
        ReturnToPool(lightningBlock);
        grid[r, c] = null;

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Collapse());
    }

    void ClearRow(int row)
    {
        PlaySound(bombClip, 0.7f);

        for (int c = 0; c < columns; c++)
        {
            Block b = grid[row, c];
            if (b == null || b.isLightning) continue;

            LevelManager.Instance.AddScore(2);
            PlayExplosionEffect(b);
            ReturnToPool(b);
            grid[row, c] = null;
        }
    }

    void ClearColumn(int col)
    {
        PlaySound(bombClip, 0.7f);

        for (int r = 0; r < rows; r++)
        {
            Block b = grid[r, col];
            if (b == null || b.isLightning) continue;

            LevelManager.Instance.AddScore(2);
            PlayExplosionEffect(b);
            ReturnToPool(b);
            grid[r, col] = null;
        }
    }

    public List<Block> GetConnectedBlocks(Block start)
    {
        List<Block> res = new List<Block>();
        if (start == null || start.isBomb || start.isLightning) return res;
        Queue<Block> q = new Queue<Block>();
        q.Enqueue(start); res.Add(start);
        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };
        while (q.Count > 0)
        {
            Block curr = q.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nr = curr.row + dr[i];
                int nc = curr.column + dc[i];
                if (nr < 0 || nr >= rows || nc < 0 || nc >= columns) continue;
                Block next = grid[nr, nc];
                if (next != null && !next.isBomb && !next.isLightning && next.colorIndex == start.colorIndex && !res.Contains(next))
                {
                    res.Add(next);
                    q.Enqueue(next);
                }
            }
        }
        return res;
    }
}

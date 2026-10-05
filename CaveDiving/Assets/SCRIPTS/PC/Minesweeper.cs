using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class Minesweeper : MonoBehaviour
{
    [Tooltip("Existing panel to build the board in. Leave empty to auto-create one.")]
    [SerializeField] RectTransform panel;
    [Tooltip("The app icon button. If set, clicking it opens the game.")]
    [SerializeField] Button appButton;
    [SerializeField] bool openOnStart = false;
    [Tooltip("Off = reopening keeps the current board (it only resets after a win or loss).")]
    [SerializeField] bool newGameOnEveryOpen = true;
    [Tooltip("Show the mouse cursor while open and restore it on close.")]
    [SerializeField] bool manageCursor = true;
    [Tooltip("Text is drawn this many times larger and shrunk back down, which keeps tiny text sharp.")]
    [SerializeField] float textSupersample = 4f;
    public UnityEvent onClosed;

    public static int LastCloseFrame = -1;

    const int N = 10;
    const int MINES = 15;

    const float DesignW = 640f;
    const float DesignH = 720f;
    const float StatusH = 60f;
    const float Gap = 10f;
    const float GridSize = 600f;
    const float Spacing = 2f;

    static readonly Color[] NumberColors =
    {
        Color.clear, new Color(0.1f, 0.3f, 0.9f), new Color(0.1f, 0.6f, 0.1f),
        new Color(0.9f, 0.1f, 0.1f), new Color(0.1f, 0.1f, 0.5f), new Color(0.5f, 0.1f, 0.1f),
        new Color(0.1f, 0.6f, 0.6f), Color.black, Color.gray
    };

    readonly bool[,] mine = new bool[N, N];
    readonly bool[,] open = new bool[N, N];
    readonly bool[,] flagged = new bool[N, N];
    readonly int[,] count = new int[N, N];
    readonly Image[,] bg = new Image[N, N];
    readonly Text[,] label = new Text[N, N];

    Text status;
    RectTransform statusRT;
    RectTransform gridRT;
    GridLayoutGroup grid;
    Font font;
    bool minesPlaced, gameOver, hasGame;
    int openedCount;
    CursorLockMode prevLock;
    bool prevVisible;

    public bool IsOpen => panel != null && panel.gameObject.activeSelf;

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        if (appButton != null) appButton.onClick.AddListener(Open);
        if (openOnStart) Open();
        else panel.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (appButton != null) appButton.onClick.RemoveListener(Open);
    }

    void Update()
    {
        if (!IsOpen) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.escapeKey.wasPressedThisFrame) Close();
        if (kb.rKey.wasPressedThisFrame) ResetGame();
    }

    public void Open()
    {
        bool wasOpen = IsOpen;
        panel.gameObject.SetActive(true);
        panel.SetAsLastSibling();
        FitLayout();

        if (wasOpen) return;

        if (manageCursor)
        {
            prevLock = Cursor.lockState;
            prevVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (newGameOnEveryOpen || !hasGame || gameOver) ResetGame();
    }

    public void Close()
    {
        if (!IsOpen) return;
        panel.gameObject.SetActive(false);

        if (manageCursor)
        {
            Cursor.lockState = prevLock;
            Cursor.visible = prevVisible;
        }

        LastCloseFrame = Time.frameCount;
        onClosed?.Invoke();
    }

    void BuildUI()
    {
        if (panel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            var panelGO = new GameObject("Minesweeper Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvas.transform, false);
            panel = (RectTransform)panelGO.transform;
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(DesignW, DesignH);
        }

        var panelImg = panel.GetComponent<Image>();
        if (panelImg == null)
        {
            panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.18f);
        }
        panelImg.raycastTarget = true;

        var rootCanvas = panel.GetComponentInParent<Canvas>()?.rootCanvas;
        if (rootCanvas != null)
        {
            if (rootCanvas.renderMode == RenderMode.WorldSpace && rootCanvas.worldCamera == null)
                rootCanvas.worldCamera = Camera.main;
            if (rootCanvas.GetComponent<GraphicRaycaster>() == null)
                rootCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystem();

        var statusGO = new GameObject("Status", typeof(RectTransform), typeof(Text));
        statusGO.transform.SetParent(panel, false);
        statusRT = (RectTransform)statusGO.transform;
        statusRT.anchorMin = statusRT.anchorMax = statusRT.pivot = new Vector2(0.5f, 0.5f);
        status = statusGO.GetComponent<Text>();
        status.font = font;
        status.alignment = TextAnchor.MiddleCenter;
        status.color = Color.white;
        status.raycastTarget = false;

        var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGO.transform.SetParent(panel, false);
        gridRT = (RectTransform)gridGO.transform;
        gridRT.anchorMin = gridRT.anchorMax = gridRT.pivot = new Vector2(0.5f, 0.5f);
        grid = gridGO.GetComponent<GridLayoutGroup>();
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = N;

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                var cell = new GameObject($"Cell {x},{y}", typeof(RectTransform), typeof(Image), typeof(ClickRelay));
                cell.transform.SetParent(gridGO.transform, false);
                bg[x, y] = cell.GetComponent<Image>();

                var txtGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
                txtGO.transform.SetParent(cell.transform, false);
                var tRT = (RectTransform)txtGO.transform;
                tRT.anchorMin = tRT.anchorMax = tRT.pivot = new Vector2(0.5f, 0.5f);
                tRT.anchoredPosition = Vector2.zero;
                var t = txtGO.GetComponent<Text>();
                t.font = font;
                t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.raycastTarget = false;
                label[x, y] = t;

                int cx = x, cy = y;
                cell.GetComponent<ClickRelay>().OnClick = e => HandleClick(cx, cy, e.button);
            }
        }
    }

    void FitLayout()
    {
        Canvas.ForceUpdateCanvases();

        Rect r = panel.rect;
        float u = Mathf.Min(r.width / DesignW, r.height / DesignH);
        if (u <= 0f) return;

        float ss = Mathf.Max(1f, textSupersample);
        float blockH = StatusH + Gap + GridSize;
        float cellSize = (GridSize - Spacing * (N - 1)) / N;

        statusRT.anchoredPosition = new Vector2(0f, (blockH * 0.5f - StatusH * 0.5f) * u);
        statusRT.sizeDelta = new Vector2(620f * u * ss, StatusH * u * ss);
        statusRT.localScale = Vector3.one / ss;
        status.fontSize = Mathf.Max(1, Mathf.RoundToInt(24f * u * ss));

        gridRT.anchoredPosition = new Vector2(0f, (blockH * 0.5f - StatusH - Gap - GridSize * 0.5f) * u);
        gridRT.sizeDelta = new Vector2(GridSize * u, GridSize * u);
        grid.cellSize = new Vector2(cellSize * u, cellSize * u);
        grid.spacing = new Vector2(Spacing * u, Spacing * u);

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                var tRT = label[x, y].rectTransform;
                tRT.sizeDelta = new Vector2(cellSize * u * ss, cellSize * u * ss);
                tRT.localScale = Vector3.one / ss;
                label[x, y].fontSize = Mathf.Max(1, Mathf.RoundToInt(32f * u * ss));
            }
        }
    }

    void EnsureEventSystem()
    {
        var existing = FindObjectOfType<EventSystem>();
        GameObject esGO;
        if (existing == null)
        {
            esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
        }
        else
        {
            esGO = existing.gameObject;
            var old = esGO.GetComponent<StandaloneInputModule>();
            if (old != null) Destroy(old);
        }

        if (esGO.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = esGO.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    void ResetGame()
    {
        Array.Clear(mine, 0, mine.Length);
        Array.Clear(open, 0, open.Length);
        Array.Clear(flagged, 0, flagged.Length);
        Array.Clear(count, 0, count.Length);
        minesPlaced = false;
        gameOver = false;
        hasGame = true;
        openedCount = 0;
        status.text = "LMB reveal   RMB flag   R restart   Esc close";
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                Refresh(x, y);
    }

    void HandleClick(int x, int y, PointerEventData.InputButton button)
    {
        if (gameOver) return;

        if (button == PointerEventData.InputButton.Right)
        {
            if (open[x, y]) return;
            flagged[x, y] = !flagged[x, y];
            Refresh(x, y);
            return;
        }

        if (button != PointerEventData.InputButton.Left || flagged[x, y] || open[x, y]) return;

        if (!minesPlaced) PlaceMines(x, y);

        if (mine[x, y])
        {
            Lose();
            return;
        }

        Flood(x, y);
        if (openedCount == N * N - MINES) Win();
    }

    void PlaceMines(int safeX, int safeY)
    {
        minesPlaced = true;
        int placed = 0;
        while (placed < MINES)
        {
            int x = UnityEngine.Random.Range(0, N);
            int y = UnityEngine.Random.Range(0, N);
            if (mine[x, y]) continue;
            if (Mathf.Abs(x - safeX) <= 1 && Mathf.Abs(y - safeY) <= 1) continue;
            mine[x, y] = true;
            placed++;
        }

        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                int c = 0;
                ForNeighbors(x, y, (nx, ny) => { if (mine[nx, ny]) c++; });
                count[x, y] = c;
            }
    }

    void Flood(int sx, int sy)
    {
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(sx, sy));
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            if (open[p.x, p.y] || flagged[p.x, p.y]) continue;
            open[p.x, p.y] = true;
            openedCount++;
            Refresh(p.x, p.y);
            if (count[p.x, p.y] == 0)
                ForNeighbors(p.x, p.y, (nx, ny) => { if (!open[nx, ny]) queue.Enqueue(new Vector2Int(nx, ny)); });
        }
    }

    void ForNeighbors(int x, int y, Action<int, int> action)
    {
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < N && ny >= 0 && ny < N) action(nx, ny);
            }
    }

    void Win()
    {
        gameOver = true;
        status.text = "You win!   R play again   Esc close";
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                if (mine[x, y]) { flagged[x, y] = true; Refresh(x, y); }
    }

    void Lose()
    {
        gameOver = true;
        status.text = "BOOM.   R try again   Esc close";
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                if (mine[x, y]) { open[x, y] = true; Refresh(x, y); }
    }

    void Refresh(int x, int y)
    {
        var img = bg[x, y];
        var txt = label[x, y];

        if (!open[x, y])
        {
            img.color = new Color(0.55f, 0.58f, 0.65f);
            txt.text = flagged[x, y] ? "F" : "";
            txt.color = new Color(1f, 0.85f, 0.1f);
            return;
        }

        if (mine[x, y])
        {
            img.color = new Color(0.85f, 0.2f, 0.2f);
            txt.text = "*";
            txt.color = Color.black;
            return;
        }

        img.color = new Color(0.85f, 0.87f, 0.9f);
        txt.text = count[x, y] > 0 ? count[x, y].ToString() : "";
        txt.color = NumberColors[count[x, y]];
    }
}

public class ClickRelay : MonoBehaviour, IPointerClickHandler
{
    public Action<PointerEventData> OnClick;
    public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(eventData);
}
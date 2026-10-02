using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Minesweeper minigame (new Input System). Lives inside a UI panel on your existing canvas.
// Esc only closes THIS panel - it never quits the app.
//
// Setup:
//  1. Put this on an object that stays ACTIVE (NOT on the panel itself, or Esc/Open() stop working).
//  2. Drag your in-game app window panel into "Panel" (needs roughly 640x720).
//  3. Drag the app's icon Button into "App Button" - clicking it opens the game.
//     (Or leave it empty and call minesweeper.Open() / wire the Button's On Click yourself.)
// Esc closes the panel and fires onClosed.
public class Minesweeper : MonoBehaviour
{
    [Tooltip("Optional existing panel to build the board in. Leave empty to auto-create one.")]
    [SerializeField] RectTransform panel;
    [Tooltip("The app icon button. If set, clicking it opens the game.")]
    [SerializeField] Button appButton;
    [SerializeField] bool openOnStart = false;
    [Tooltip("Off = reopening keeps the current board (it only resets after a win or loss).")]
    [SerializeField] bool newGameOnEveryOpen = true;
    [Tooltip("Show the mouse cursor while open and restore it on close (for first-person games).")]
    [SerializeField] bool manageCursor = true;
    [Tooltip("Overall size multiplier. 1 = 640x720 panel, 0.1667 = 6x smaller.")]
    [SerializeField] float sizeScale = 1f / 6f;
    [Tooltip("Text is drawn this many times larger and shrunk back down, which keeps tiny text sharp. 4 is a good default.")]
    [SerializeField] float textSupersample = 4f;
    public UnityEvent onClosed;

    // Frame in which the panel was last closed. A pause menu can compare this to Time.frameCount
    // to ignore the same Esc press.
    public static int LastCloseFrame = -1;

    const int N = 10;
    const int MINES = 15;

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
    Font font;
    bool minesPlaced, gameOver, hasGame;
    int openedCount;
    CursorLockMode prevLock;
    bool prevVisible;

    float S(float v) => v * sizeScale;
    int TF(float v) => Mathf.Max(1, Mathf.RoundToInt(v * sizeScale * textSupersample)); // supersampled font size

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

    // ---------- Open / close ----------

    public void Open()
    {
        bool wasOpen = IsOpen;
        panel.gameObject.SetActive(true);
        panel.SetAsLastSibling(); // draw on top of the rest of your UI

        if (manageCursor && !wasOpen)
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

    // ---------- UI ----------

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
            panel.sizeDelta = new Vector2(S(640), S(720));
        }

        // Make sure the panel has a background that also blocks clicks to whatever is behind it
        var panelImg = panel.GetComponent<Image>();
        if (panelImg == null)
        {
            panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.18f);
        }
        panelImg.raycastTarget = true;

        // World Space canvas (e.g. a 3D monitor) needs an event camera or clicks do nothing
        var rootCanvas = panel.GetComponentInParent<Canvas>()?.rootCanvas;
        if (rootCanvas != null && rootCanvas.renderMode == RenderMode.WorldSpace && rootCanvas.worldCamera == null)
            rootCanvas.worldCamera = Camera.main;

        EnsureEventSystem();

        // Status text
        var statusGO = new GameObject("Status", typeof(RectTransform), typeof(Text));
        statusGO.transform.SetParent(panel, false);
        var sRT = (RectTransform)statusGO.transform;
        sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 1f);
        sRT.pivot = new Vector2(0.5f, 1f);
        sRT.anchoredPosition = new Vector2(0, S(-20));
        sRT.sizeDelta = new Vector2(S(620) * textSupersample, S(60) * textSupersample);
        sRT.localScale = Vector3.one / textSupersample;
        status = statusGO.GetComponent<Text>();
        status.font = font;
        status.fontSize = TF(24);
        status.alignment = TextAnchor.MiddleCenter;
        status.color = Color.white;
        status.raycastTarget = false;

        // Grid container
        var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGO.transform.SetParent(panel, false);
        var gRT = (RectTransform)gridGO.transform;
        gRT.anchorMin = gRT.anchorMax = gRT.pivot = new Vector2(0.5f, 0.5f);
        gRT.anchoredPosition = new Vector2(0, S(-30));
        gRT.sizeDelta = new Vector2(S(600), S(600));
        var grid = gridGO.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(S(58), S(58));
        grid.spacing = new Vector2(S(2), S(2));
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
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
                tRT.sizeDelta = new Vector2(S(58) * textSupersample, S(58) * textSupersample);
                tRT.localScale = Vector3.one / textSupersample;
                var t = txtGO.GetComponent<Text>();
                t.font = font;
                t.fontSize = TF(32);
                t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.raycastTarget = false;
                label[x, y] = t;

                int cx = x, cy = y;
                cell.GetComponent<ClickRelay>().OnClick = e => HandleClick(cx, cy, e.button);
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
            if (old != null) Destroy(old); // old module breaks with the new Input System
        }

        if (esGO.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = esGO.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    // ---------- Game logic ----------

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

        if (!minesPlaced) PlaceMines(x, y); // first click is always safe

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

// Tiny helper so each cell can report left/right clicks.
public class ClickRelay : MonoBehaviour, IPointerClickHandler
{
    public Action<PointerEventData> OnClick;
    public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(eventData);
}
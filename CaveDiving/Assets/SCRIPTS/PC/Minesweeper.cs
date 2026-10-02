using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Attach to an empty GameObject in an empty scene. Press Play.
// Left click = reveal, right click = flag, R = restart, Esc = quit.
public class Minesweeper : MonoBehaviour
{
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
    bool minesPlaced, gameOver;
    int openedCount;

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        ResetGame();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.escapeKey.wasPressedThisFrame) Quit();
        if (kb.rKey.wasPressedThisFrame) ResetGame();
    }

    void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- UI ----------

    void BuildUI()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

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
            // Old input module would throw errors with the new Input System
            var old = esGO.GetComponent<StandaloneInputModule>();
            if (old != null) Destroy(old);
        }
        if (esGO.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = esGO.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        var back = new GameObject("Background", typeof(RectTransform), typeof(Image));
        back.transform.SetParent(canvasGO.transform, false);
        var backRT = (RectTransform)back.transform;
        backRT.anchorMin = Vector2.zero;
        backRT.anchorMax = Vector2.one;
        backRT.offsetMin = backRT.offsetMax = Vector2.zero;
        back.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f);
        back.GetComponent<Image>().raycastTarget = false;

        var statusGO = new GameObject("Status", typeof(RectTransform), typeof(Text));
        statusGO.transform.SetParent(canvasGO.transform, false);
        var sRT = (RectTransform)statusGO.transform;
        sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 1f);
        sRT.pivot = new Vector2(0.5f, 1f);
        sRT.anchoredPosition = new Vector2(0, -40);
        sRT.sizeDelta = new Vector2(900, 60);
        status = statusGO.GetComponent<Text>();
        status.font = font;
        status.fontSize = 36;
        status.alignment = TextAnchor.MiddleCenter;
        status.color = Color.white;
        status.raycastTarget = false;

        var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGO.transform.SetParent(canvasGO.transform, false);
        var gRT = (RectTransform)gridGO.transform;
        gRT.anchorMin = gRT.anchorMax = gRT.pivot = new Vector2(0.5f, 0.5f);
        gRT.anchoredPosition = new Vector2(0, -30);
        gRT.sizeDelta = new Vector2(600, 600);
        var grid = gridGO.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(58, 58);
        grid.spacing = new Vector2(2, 2);
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
                tRT.anchorMin = Vector2.zero;
                tRT.anchorMax = Vector2.one;
                tRT.offsetMin = tRT.offsetMax = Vector2.zero;
                var t = txtGO.GetComponent<Text>();
                t.font = font;
                t.fontSize = 32;
                t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.raycastTarget = false;
                label[x, y] = t;

                int cx = x, cy = y;
                cell.GetComponent<ClickRelay>().OnClick = e => HandleClick(cx, cy, e.button);
            }
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
        openedCount = 0;
        status.text = "Minesweeper  |  LMB reveal  RMB flag  R restart  Esc quit";
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
        status.text = "You win!  Press R to play again";
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                if (mine[x, y]) { flagged[x, y] = true; Refresh(x, y); }
    }

    void Lose()
    {
        gameOver = true;
        status.text = "BOOM. Press R to try again";
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
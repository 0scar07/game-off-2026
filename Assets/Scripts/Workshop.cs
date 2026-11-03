using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Editor de niveles (el Taller): una cuadrícula donde se pintan casillas con las letras de los
    /// niveles, se prueba al momento y se comparte con un código "FASE-...". El mapa se guarda solo.
    /// </summary>
    [RequireComponent(typeof(RuntimeLevel))]
    public class Workshop : MonoBehaviour
    {
        const int Cols = 64, RowsCount = RuntimeLevel.Rows;
        const float Cell = 24f;
        const string SaveKey = "fase_taller";

        static readonly (char code, string es, string en, Color color)[] Brushes =
        {
            ('#', "Suelo", "Ground", new Color(0.3f, 0.65f, 0.35f)), ('D', "Caja", "Crate", new Color(0.8f, 0.55f, 0.3f)),
            ('d', "Tabla", "Plank", new Color(0.9f, 0.7f, 0.45f)), ('N', "Hielo", "Ice", new Color(0.55f, 0.75f, 1f)),
            ('n', "Placa", "Ice slab", new Color(0.7f, 0.85f, 1f)), ('~', "Lago", "Lake", new Color(0.3f, 0.5f, 0.95f)),
            ('^', "Pinchos", "Spikes", new Color(0.85f, 0.85f, 0.9f)), ('F', "Muelle", "Spring", new Color(1f, 0.45f, 0.6f)),
            ('M', "Sombra", "Shadow", new Color(0.45f, 0.3f, 0.8f)), ('X', "Sierra", "Saw", new Color(0.7f, 0.7f, 0.75f)),
            ('O', "Reloj", "Sundial", new Color(1f, 0.8f, 0.3f)), ('Z', "Caja mágica", "Magic box", new Color(0.6f, 0.9f, 0.9f)),
            ('*', "Luna", "Moon", new Color(1f, 0.9f, 0.4f)), ('C', "Control", "Checkpoint", new Color(0.5f, 1f, 0.6f)),
            ('S', "Salida", "Start", Color.white), ('G', "Meta", "Goal", new Color(1f, 0.75f, 0.3f)), (' ', "Borrar", "Erase", new Color(0.2f, 0.2f, 0.3f)),
        };

        char[,] map = new char[RowsCount, Cols];
        Image[,] cells;
        char brush = '#';
        Canvas editorCanvas, playCanvas;
        Text message;
        InputField codeInput;
        RuntimeLevel runtime;
        PlayerController player;
        bool editing;

        void Start()
        {
            runtime = GetComponent<RuntimeLevel>();
            player = FindAnyObjectByType<PlayerController>();
            Load(PlayerPrefs.GetString(SaveKey, ""));
            BuildEditor();
            // Si se vuelve de probar el nivel (meta → "Siguiente"), se sigue editando.
            Edit();
        }

        // ───────── Mapa ─────────

        void Load(string code)
        {
            var rows = string.IsNullOrEmpty(code) ? null : LevelCode.Decode(code);
            for (int r = 0; r < RowsCount; r++)
                for (int c = 0; c < Cols; c++)
                    map[r, c] = rows != null && r < rows.Length && c < rows[r].Length ? rows[r][c] : ' ';
            if (rows == null)
            {
                // Nivel de ejemplo: suelo, salida, un puente de día y la meta.
                for (int r = RowsCount - 3; r < RowsCount; r++) for (int c = 0; c < 40; c++) map[r, c] = c >= 14 && c < 20 ? ' ' : '#';
                for (int c = 14; c < 20; c++) map[RowsCount - 3, c] = 'd';
                map[RowsCount - 4, 2] = 'S';
                map[RowsCount - 4, 36] = 'G';
                map[RowsCount - 4, 26] = '*';
            }
        }

        string[] Rows()
        {
            var rows = new string[RowsCount];
            for (int r = 0; r < RowsCount; r++)
            {
                var chars = new char[Cols];
                for (int c = 0; c < Cols; c++) chars[c] = map[r, c];
                rows[r] = new string(chars).TrimEnd();
            }
            return rows;
        }

        void Save() => PlayerPrefs.SetString(SaveKey, LevelCode.Encode(Rows()));

        // ───────── Interfaz ─────────

        void BuildEditor()
        {
            UiKit.EnsureEvents();
            editorCanvas = UiKit.Canvas("Taller", 70);
            var root = editorCanvas.transform;
            var shade = UiKit.Fill(root, "Fondo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.05f, 0.04f, 0.14f, 0.92f);
            UiKit.Text(root, "Titulo", Loc.Pick("Taller de niveles", "Level workshop"), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1200f, 80f), 60);

            var grid = UiKit.Rect(root, "Cuadricula", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(Cols * Cell, RowsCount * Cell));
            cells = new Image[RowsCount, Cols];
            for (int r = 0; r < RowsCount; r++)
                for (int c = 0; c < Cols; c++)
                {
                    var img = UiKit.Box(grid, "c", new Vector2(0f, 1f), new Vector2(c * Cell, -r * Cell), new Vector2(Cell - 2f, Cell - 2f), Color.white, false);
                    img.rectTransform.pivot = new Vector2(0f, 1f);
                    img.raycastTarget = true;
                    int rr = r, cc = c;
                    var trigger = img.gameObject.AddComponent<EventTrigger>();
                    AddTrigger(trigger, EventTriggerType.PointerDown, () => Paint(rr, cc));
                    AddTrigger(trigger, EventTriggerType.PointerEnter, () => { if (Pressed()) Paint(rr, cc); });
                    cells[r, c] = img;
                }

            // Pinceles.
            for (int i = 0; i < Brushes.Length; i++)
            {
                var b = Brushes[i];
                float x = (i % 9 - 4f) * 190f, y = i < 9 ? -190f : -270f;
                var button = UiKit.Button(root, "Pincel", Loc.Pick(b.es, b.en), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(180f, 66f), () => brush = b.code, 24);
                button.GetComponent<Image>().color = Color.Lerp(b.color, Color.black, 0.35f);
            }

            UiKit.Button(root, "Probar", "▶ " + Loc.Pick("Probar", "Play"), new Vector2(0.5f, 0f), new Vector2(-560f, 60f), new Vector2(300f, 90f), Play, 38);
            UiKit.Button(root, "Compartir", Loc.Pick("Copiar código", "Copy code"), new Vector2(0.5f, 0f), new Vector2(-220f, 60f), new Vector2(330f, 90f), ShareCode, 32);
            codeInput = CodeInput(root);
            UiKit.Button(root, "Cargar", Loc.Pick("Cargar", "Load"), new Vector2(0.5f, 0f), new Vector2(560f, 60f), new Vector2(180f, 90f), LoadCode, 32);
            UiKit.Button(root, "Limpiar", Loc.Pick("Vaciar", "Clear"), new Vector2(1f, 1f), new Vector2(-260f, -50f), new Vector2(200f, 70f), ClearMap, 28)
                .GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            UiKit.Button(root, "Menu", Loc.Get("menu"), new Vector2(1f, 1f), new Vector2(-40f, -50f), new Vector2(200f, 70f), () => SceneFader.Load(PauseMenu.MenuScene), 28)
                .GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            message = UiKit.Text(root, "Mensaje", "", new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1400f, 50f), 30, TextAnchor.MiddleCenter, UiKit.Gold);

            playCanvas = UiKit.Canvas("TallerJugando", 70);
            UiKit.Button(playCanvas.transform, "Editar", "✎ " + Loc.Pick("Editar", "Edit"), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(260f, 80f), Edit, 34)
                .GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            Refresh();
        }

        InputField CodeInput(Transform root)
        {
            var go = DefaultControls.CreateInputField(new DefaultControls.Resources { inputField = UiKit.Rounded });
            go.transform.SetParent(root, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(200f, 60f);
            rect.sizeDelta = new Vector2(500f, 80f);
            var input = go.GetComponent<InputField>();
            foreach (var t in go.GetComponentsInChildren<Text>()) { t.font = UiKit.Font; t.fontSize = 26; }
            input.placeholder.GetComponent<Text>().text = Loc.Pick("Pega un código FASE-...", "Paste a FASE-... code");
            return input;
        }

        static void AddTrigger(EventTrigger trigger, EventTriggerType type, System.Action action)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }

        static bool Pressed() =>
            (Mouse.current != null && Mouse.current.leftButton.isPressed) || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);

        void Paint(int r, int c)
        {
            // Solo una salida y una meta.
            if (brush == 'S' || brush == 'G')
                for (int rr = 0; rr < RowsCount; rr++) for (int cc = 0; cc < Cols; cc++) if (map[rr, cc] == brush) map[rr, cc] = ' ';
            map[r, c] = brush;
            Refresh();
            Save();
        }

        void Refresh()
        {
            for (int r = 0; r < RowsCount; r++)
                for (int c = 0; c < Cols; c++)
                {
                    char code = map[r, c];
                    var color = new Color(1f, 1f, 1f, 0.05f);
                    foreach (var b in Brushes) if (b.code == code && code != ' ') color = b.color;
                    cells[r, c].color = color;
                }
        }

        void ClearMap()
        {
            for (int r = 0; r < RowsCount; r++) for (int c = 0; c < Cols; c++) map[r, c] = ' ';
            Refresh();
            Save();
        }

        void ShareCode()
        {
            Save();
            string code = LevelCode.Encode(Rows());
            message.text = Loc.Get(Share.Text(code));
        }

        void LoadCode()
        {
            var rows = LevelCode.Decode(codeInput.text);
            if (rows == null) { message.text = Loc.Pick("Ese código no vale.", "That code doesn't work."); return; }
            Load(codeInput.text);
            Refresh();
            Save();
            message.text = Loc.Pick("¡Nivel cargado!", "Level loaded!");
        }

        // ───────── Editar / jugar ─────────

        /// <summary>Las interfaces del juego (no las del Taller), buscadas cada vez: algunas se crean tarde.</summary>
        void ShowGameUi(bool show)
        {
            foreach (var c in FindObjectsByType<Canvas>())
                if (c != editorCanvas && c != playCanvas && c.name != "Transicion" && c.name != "AvisoLogro") c.enabled = show;
        }

        void Edit()
        {
            editing = true;
            editorCanvas.enabled = true;
            playCanvas.enabled = false;
            ShowGameUi(false);
            if (player != null) player.GetComponent<Rigidbody2D>().simulated = false;
            runtime.Clear();
        }

        void Play()
        {
            bool hasStart = false, hasGoal = false;
            foreach (var ch in map) { hasStart |= ch == 'S'; hasGoal |= ch == 'G'; }
            if (!hasStart || !hasGoal) { message.text = Loc.Pick("Pon una salida y una meta.", "Place a start and a goal."); return; }
            Save();
            editing = false;
            editorCanvas.enabled = false;
            playCanvas.enabled = true;
            ShowGameUi(true);
            runtime.Clear();
            runtime.Append(Rows(), 0);
            if (player != null) player.GetComponent<Rigidbody2D>().simulated = true;
            runtime.PlacePlayer();
            var line = ShiftLine.Instance;
            if (line != null) line.x = runtime.Start.x + 3f;
        }

        void Update()
        {
            if (editing && player != null) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }
}

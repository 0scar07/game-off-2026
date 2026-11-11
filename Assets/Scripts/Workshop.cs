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

        readonly System.Collections.Generic.List<(char code, Image back, Outline edge)> brushTiles = new System.Collections.Generic.List<(char, Image, Outline)>();

        void RefreshBrushes()
        {
            foreach (var (code, back, edge) in brushTiles)
            {
                bool on = code == brush;
                edge.enabled = on;
                back.color = on ? new Color(0.45f, 0.34f, 0.12f, 0.85f) : new Color(1f, 1f, 1f, 0.08f);
            }
        }

        /// <summary>Dibujo pequeño de cada pieza con cajitas (para la paleta).</summary>
        static void BrushIcon(Transform parent, char code, Color color)
        {
            var at = new Vector2(0f, 14f);
            Image Box(Vector2 pos, Vector2 size, Color c, float rot = 0f, bool round = false)
            {
                var img = UiKit.Box(parent, "Dibujo", new Vector2(0.5f, 0.5f), at + pos, size, c, round);
                img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                if (rot != 0f) img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rot);
                img.raycastTarget = false;
                return img;
            }
            void Glyph(string g, int size, Color c)
            {
                var t = UiKit.Text(parent, "Signo", g, new Vector2(0.5f, 0.5f), at, new Vector2(80f, 60f), size, TextAnchor.MiddleCenter, c);
                t.raycastTarget = false;
            }
            var dark = Color.Lerp(color, Color.black, 0.35f);
            switch (code)
            {
                case '#': Box(new Vector2(0f, -6f), new Vector2(52f, 30f), new Color(0.75f, 0.5f, 0.32f)); Box(new Vector2(0f, 12f), new Vector2(52f, 10f), color); break;
                case 'D': Box(Vector2.zero, new Vector2(40f, 40f), color); Box(Vector2.zero, new Vector2(52f, 5f), dark, 45f); Box(Vector2.zero, new Vector2(52f, 5f), dark, -45f); break;
                case 'd': Box(Vector2.zero, new Vector2(56f, 12f), color); Box(new Vector2(-14f, 0f), new Vector2(3f, 12f), dark); Box(new Vector2(14f, 0f), new Vector2(3f, 12f), dark); break;
                case 'N': Box(Vector2.zero, new Vector2(42f, 40f), color); Box(new Vector2(-8f, 10f), new Vector2(18f, 4f), Color.white); break;
                case 'n': Box(Vector2.zero, new Vector2(56f, 14f), color); Box(new Vector2(-12f, 2f), new Vector2(20f, 3f), Color.white); break;
                case '~': Box(new Vector2(0f, -4f), new Vector2(56f, 28f), color); Box(new Vector2(-10f, 4f), new Vector2(22f, 4f), new Color(0.8f, 0.92f, 1f)); break;
                case '^': for (int k = -1; k <= 1; k++) Box(new Vector2(k * 17f, -8f), new Vector2(18f, 18f), color, 45f); Box(new Vector2(0f, -20f), new Vector2(60f, 14f), new Color(0.12f, 0.1f, 0.25f)); break;
                case 'F': Box(new Vector2(0f, -14f), new Vector2(44f, 10f), dark); for (int k = 0; k < 3; k++) Box(new Vector2(0f, -4f + k * 9f), new Vector2(30f - k * 4f, 5f), color); Box(new Vector2(0f, 22f), new Vector2(44f, 8f), color); break;
                case 'M': Box(Vector2.zero, new Vector2(44f, 40f), color, 0f, true); Box(new Vector2(-9f, 4f), new Vector2(9f, 9f), Color.white); Box(new Vector2(9f, 4f), new Vector2(9f, 9f), Color.white); break;
                case 'X': Box(Vector2.zero, new Vector2(36f, 36f), color, 45f); Box(Vector2.zero, new Vector2(36f, 36f), color); Box(Vector2.zero, new Vector2(10f, 10f), dark); break;
                case 'O': Box(Vector2.zero, new Vector2(44f, 44f), color, 0f, true); Box(new Vector2(4f, 6f), new Vector2(4f, 20f), dark, -35f); break;
                case 'Z': Box(Vector2.zero, new Vector2(40f, 40f), color); Glyph("✦", 30, Color.white); break;
                case '*': Box(Vector2.zero, new Vector2(24f, 24f), color, 45f); break;
                case 'C': Box(new Vector2(-10f, 0f), new Vector2(4f, 46f), Color.white); Box(new Vector2(4f, 12f), new Vector2(26f, 16f), color); break;
                case 'S': Box(Vector2.zero, new Vector2(34f, 38f), Color.white, 0f, true); Box(new Vector2(0f, 2f), new Vector2(22f, 14f), new Color(0.12f, 0.1f, 0.25f), 0f, true); break;
                case 'G': Box(new Vector2(-10f, 0f), new Vector2(4f, 46f), Color.white); Box(new Vector2(4f, 12f), new Vector2(26f, 16f), color); Glyph("★", 18, new Color(0.3f, 0.2f, 0.05f)); break;
                default: Glyph("✖", 34, new Color(1f, 0.55f, 0.55f)); break;
            }
        }

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
            // Un nivel que llegó por enlace manda sobre el guardado.
            if (!string.IsNullOrEmpty(LinkOpener.PendingCode)) { Load(LinkOpener.PendingCode); LinkOpener.PendingCode = null; }
            else Load(PlayerPrefs.GetString(SaveKey, ""));
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

            // Pinceles: una baldosa por pieza con su dibujo y su nombre; la elegida, con borde dorado.
            brushTiles.Clear();
            for (int i = 0; i < Brushes.Length; i++)
            {
                var b = Brushes[i];
                int perRow = 9, row = i / perRow, inRow = Mathf.Min(perRow, Brushes.Length - row * perRow);
                float x = (i % perRow - (inRow - 1) * 0.5f) * 150f, y = -122f - row * 130f;
                var button = UiKit.Button(root, "Pincel", "", new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(138f, 122f), () => { brush = b.code; RefreshBrushes(); }, 22);
                button.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
                var label = button.GetComponentInChildren<Text>();
                label.text = Loc.Pick(b.es, b.en);
                label.rectTransform.anchoredPosition = new Vector2(0f, -36f);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 16;
                label.resizeTextMaxSize = 22;
                label.rectTransform.sizeDelta = new Vector2(130f, 34f);
                BrushIcon(button.transform, b.code, b.color);
                var edge = button.gameObject.AddComponent<Outline>();
                edge.effectColor = UiKit.Gold;
                edge.effectDistance = new Vector2(3f, -3f);
                brushTiles.Add((b.code, button.GetComponent<Image>(), edge));
            }
            RefreshBrushes();

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
            foreach (var t in go.GetComponentsInChildren<Text>()) { t.font = UiKit.Font; UiKit.SetSize(t, 26); }
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
            message.text = Loc.Get(Share.Text(Loc.Pick($"Juega mi nivel de FASE: {LinkOpener.Link(code)}\n(o pega este código en el Taller: {code})", $"Play my FASE level: {LinkOpener.Link(code)}\n(or paste this code in the Workshop: {code})",
                $"Jogue meu nível de FASE: {LinkOpener.Link(code)}\n(ou cole este código na Oficina: {code})", $"Joue à mon niveau de FASE : {LinkOpener.Link(code)}\n(ou colle ce code dans l'Atelier : {code})")));

        }

        void LoadCode()
        {
            // Vale pegar el texto entero que se comparte (con el enlace): se busca el código dentro.
            string text = codeInput.text;
            int at = text.LastIndexOf("FASE-", System.StringComparison.Ordinal);
            if (at >= 0) text = text.Substring(at).Split(' ', '\n', '\r', ')')[0];
            var rows = LevelCode.Decode(text);
            if (rows == null) { message.text = Loc.Pick("Ese código no vale.", "That code doesn't work."); return; }
            Load(text);

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

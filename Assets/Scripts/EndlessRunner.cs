using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Modo sin fin: el camino se va montando por delante con tramos al azar (puentes, hielo,
    /// lagos, Sombras, sierras, relojes de sol...) y cada vez más difíciles. Una caída acaba la
    /// partida; cuenta la distancia y guarda el récord.
    /// </summary>
    [RequireComponent(typeof(RuntimeLevel))]
    public class EndlessRunner : MonoBehaviour
    {
        const string BestKey = "fase_sinfin_record";

        RuntimeLevel runtime;
        PlayerController player;
        Text distanceLabel;
        float startX;
        bool over;
        System.Random random;

        public static float Best => PlayerPrefs.GetFloat(BestKey, 0f);

        void Start()
        {
            runtime = GetComponent<RuntimeLevel>();
            player = FindAnyObjectByType<PlayerController>();
            random = new System.Random();
            var first = new Segment();
            first.Flat(10);
            first.Put('S', 2, first.Top - 1);
            runtime.Append(first.Rows(), 0);
            Grow();
            runtime.PlacePlayer();
            startX = runtime.Start.x;
            var line = ShiftLine.Instance;
            if (line != null) line.x = startX + 4f;
            player.Died += GameOver;

            var canvas = UiKit.Canvas("Distancia", 6);
            canvas.transform.SetParent(transform, false);
            UiKit.Box(canvas.transform, "Fondo", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(420f, 70f), new Color(0.04f, 0.03f, 0.14f, 0.55f));
            distanceLabel = UiKit.Text(canvas.transform, "Texto", "", new Vector2(0.5f, 1f), new Vector2(0f, -75f), new Vector2(420f, 70f), 40);
        }

        void Update()
        {
            if (over || player == null) return;
            float meters = Mathf.Max(0f, player.transform.position.x - startX);
            distanceLabel.text = $"∞  {meters:0} m   ·   {Loc.Get("best")} {Mathf.Max(Best, meters):0} m";
            if (player.transform.position.x > runtime.Width - 45f) Grow();
        }

        /// <summary>Añade unos cuantos tramos más por la derecha.</summary>
        void Grow()
        {
            float difficulty = Mathf.Clamp01(runtime.Width / 900f);
            var s = new Segment();
            for (int i = 0; i < 4; i++)
            {
                int pick = random.Next(0, 10 + (int)(difficulty * 4));
                switch (pick % 12)
                {
                    case 0: s.Gap('d', 5 + random.Next(3)); break;
                    case 1: s.Gap('n', 5 + random.Next(3)); break;
                    case 2: s.Lake(6 + random.Next(4)); break;
                    case 3: s.Flat(10); s.Put('M', s.X - 5, s.Top - 1); break;
                    case 4: s.Flat(10); s.Put('X', s.X - 6, s.Top - 1); break;
                    case 5: s.Flat(8); s.Fill('^', s.X - 5, s.X - 3, s.Top - 1); break;
                    case 6: s.Steps('D'); break;
                    case 7: s.Steps('n'); break;
                    case 8: s.SunGap(); break;
                    case 9: s.Gap('Y', 6); break;
                    case 10: s.Gap('d', 4); s.Gap('n', 4); break;
                    default: s.Flat(6); s.Put('*', s.X - 3, s.Top - 4); break;
                }
                s.Flat(3);
                if (random.NextDouble() < 0.3) s.Put('*', s.X - 2, s.Top - 4);
            }
            s.Flat(2);
            s.Put('C', s.X - 1, s.Top - 1);
            runtime.Append(s.Rows(), runtime.Width);
            runtime.Fit();
        }

        void GameOver()
        {
            if (over) return;
            over = true;
            float meters = Mathf.Max(0f, player.transform.position.x - startX);
            // Die() ya devolvió a Lumi al punto de control: la distancia buena es la del momento de caer.
            meters = Mathf.Max(meters, lastMeters);
            bool record = meters > Best;
            if (record) { PlayerPrefs.SetFloat(BestKey, meters); PlayerPrefs.Save(); }
            player.GetComponent<Rigidbody2D>().simulated = false;

            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("FinSinFin", 40);
            var shade = UiKit.Fill(canvas.transform, "Fondo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.02f, 0.02f, 0.08f, 0.7f);
            UiKit.Text(canvas.transform, "Titulo", Loc.Pick("¡Fin del camino!", "End of the road!"), new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(1400f, 120f), 96);
            UiKit.Text(canvas.transform, "Distancia", $"{meters:0} m", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1000f, 160f), 150, TextAnchor.MiddleCenter, UiKit.Gold);
            UiKit.Text(canvas.transform, "Record", record ? Loc.Get("record") : $"{Loc.Get("best")}: {Best:0} m", new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1000f, 60f), 44, TextAnchor.MiddleCenter, UiKit.Soft);
            var again = UiKit.Button(canvas.transform, "Otra", Loc.Get("retry"), new Vector2(0.5f, 0.5f), new Vector2(-220f, -220f), new Vector2(380f, 110f), () => SceneFader.Load(gameObject.scene.name), 46);
            UiKit.Button(canvas.transform, "Menu", Loc.Get("menu"), new Vector2(0.5f, 0.5f), new Vector2(220f, -220f), new Vector2(380f, 110f), () => SceneFader.Load(PauseMenu.MenuScene), 46);
            string shareText = Loc.English ? $"I ran {meters:0} m in FASE's endless mode 🌗 {Share.Url}" : $"Recorrí {meters:0} m en el modo sin fin de FASE 🌗 {Share.Url}";
            var share = UiKit.Button(canvas.transform, "Compartir", Loc.Get("share"), new Vector2(0.5f, 0.5f), new Vector2(0f, -360f), new Vector2(380f, 90f), null, 38);
            var label = share.GetComponentInChildren<Text>();
            share.onClick.AddListener(() => label.text = Loc.Get(Share.Text(shareText)));
            again.Select();
        }

        float lastMeters;

        void FixedUpdate()
        {
            if (!over && player != null) lastMeters = Mathf.Max(lastMeters, player.transform.position.x - startX);
        }

        /// <summary>Un trozo de mapa de 14 filas que se va escribiendo de izquierda a derecha.</summary>
        class Segment
        {
            readonly List<char[]> columns = new List<char[]>();
            public int Top = RuntimeLevel.Rows - 3;
            public int X => columns.Count;

            char[] Column(int x)
            {
                while (columns.Count <= x) { var col = new char[RuntimeLevel.Rows]; for (int i = 0; i < col.Length; i++) col[i] = ' '; columns.Add(col); }
                return columns[x];
            }

            public void Put(char c, int x, int row) { if (row >= 0 && row < RuntimeLevel.Rows && x >= 0) Column(x)[row] = c; }
            public void Fill(char c, int from, int to, int row) { for (int x = from; x < to; x++) Put(c, x, row); }

            public void Flat(int n)
            {
                int start = X;
                for (int x = start; x < start + n; x++) for (int r = Top; r < RuntimeLevel.Rows; r++) Put('#', x, r);
            }

            public void Gap(char plank, int w)
            {
                int start = X;
                Column(start + w - 1);
                Fill(plank, start, start + w, Top);
            }

            public void Lake(int w)
            {
                int start = X;
                Fill('~', start, start + w, Top);
                for (int x = start; x < start + w; x++) for (int r = Top + 1; r < RuntimeLevel.Rows; r++) Put('#', x, r);
            }

            public void Steps(char kind)
            {
                int start = X;
                Column(start + 11);
                Fill(kind, start + 1, start + 3, Top - 1);
                Fill(kind, start + 5, start + 7, Top - 2);
                Fill(kind, start + 9, start + 11, Top - 1);
            }

            public void SunGap()
            {
                int start = X;
                Column(start + 8);
                Put('O', start + 4, Top);
            }

            public string[] Rows()
            {
                var rows = new string[RuntimeLevel.Rows];
                for (int r = 0; r < rows.Length; r++)
                {
                    var chars = new char[columns.Count];
                    for (int x = 0; x < columns.Count; x++) chars[x] = columns[x][r];
                    rows[r] = new string(chars);
                }
                return rows;
            }
        }
    }
}

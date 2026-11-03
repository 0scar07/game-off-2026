using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Logros. Se crean solos al arrancar el juego (no hace falta ponerlos en la escena), escuchan
    /// GameEvents y, al conseguir uno, lo guardan y muestran un aviso abajo a la derecha.
    /// </summary>
    public class Achievements : MonoBehaviour
    {
        public class Info
        {
            public string id, nameEs, nameEn, descEs, descEn;
            public bool secret;   // no se ve en la lista hasta conseguirlo
            public string Name => Loc.Pick(nameEs, nameEn);
            public string Description => Loc.Pick(descEs, descEn);
        }

        public static readonly Info[] All =
        {
            New("amanecer", "Primer amanecer", "First dawn", "Termina el nivel 1", "Finish level 1"),
            New("pradera", "La Pradera", "The Meadow", "Termina el capítulo I", "Finish chapter I"),
            New("bosque", "El Bosque", "The Forest", "Termina el capítulo II", "Finish chapter II"),
            New("cumbre", "La Cumbre", "The Summit", "Termina el capítulo III", "Finish chapter III"),
            New("eclipse", "Eclipse", "Eclipse", "Vence a la Gran Sombra", "Defeat the Great Shadow"),
            New("coleccionista", "Coleccionista", "Collector", "Todos los fragmentos de luna", "Every moon fragment"),
            New("cofre", "Tesoro escondido", "Hidden treasure", "Abre un cofre secreto", "Open a secret chest"),
            New("equilibrista", "Equilibrista", "Tightrope walker", "Salta de un puente mientras desaparece", "Jump off a bridge as it vanishes"),
            New("piedra", "Piedra, papel...", "Rock, paper...", "Pasa junto a 10 Sombras dormidas", "Walk past 10 sleeping Shadows"),
            New("sinmorir", "Sin mirar atrás", "No looking back", "Termina un nivel sin perder", "Finish a level without falling"),
            New("medioymedio", "Medio y medio", "Half and half", "Quédate 3 s justo encima de la línea", "Stand right on the line for 3 s"),
            New("oro", "Velocista", "Speedrunner", "Consigue 5 medallas de oro", "Earn 5 gold medals"),
            New("luna", "Luna llena", "Full moon", "Consigue una medalla de Luna", "Earn a Moon medal"),
            New("saltos", "Muelles en los pies", "Spring in your step", "Salta 500 veces", "Jump 500 times"),
            New("estilo", "Con estilo", "Looking sharp", "Ponle a Lumi una bufanda nueva", "Give Lumi a new scarf"),
            New("secreto", "Más allá del mapa", "Off the map", "Termina un nivel secreto", "Finish a secret level"),
            New("espejo", "Al otro lado", "Through the mirror", "Termina un nivel en modo espejo", "Finish a level in mirror mode"),
            New("diario", "Cada día", "Every day", "Termina un desafío diario", "Finish a daily challenge"),
            Secret("noctambulo", "Noctámbulo", "Night owl", "Pasa 60 s seguidos de noche", "Spend 60 s in a row at night"),
            Secret("saltarin", "Saltarín", "Bouncy", "Salta 15 veces en 8 segundos", "Jump 15 times in 8 seconds"),
            Secret("lector", "Lector", "Bookworm", "Encuentra 5 páginas del diario", "Find 5 diary pages"),
            Secret("fotografo", "Fotógrafo", "Photographer", "Haz una foto en el modo foto", "Take a picture in photo mode"),
        };

        static Info Secret(string id, string nameEs, string nameEn, string descEs, string descEn)
        {
            var info = New(id, nameEs, nameEn, descEs, descEn);
            info.secret = true;
            return info;
        }

        static Info New(string id, string nameEs, string nameEn, string descEs, string descEn) =>
            new Info { id = id, nameEs = nameEs, nameEn = nameEn, descEs = descEs, descEn = descEn };

        public static bool Has(string id) => PlayerPrefs.GetInt("fase_logro_" + id, 0) == 1;

        public static int Count
        {
            get { int n = 0; foreach (var a in All) if (Has(a.id)) n++; return n; }
        }

        public static void Unlock(string id)
        {
            if (Has(id)) return;
            PlayerPrefs.SetInt("fase_logro_" + id, 1);
            PlayerPrefs.Save();
            GameEvents.RaiseAchievement(id);
            if (instance != null) instance.queue.Enqueue(id);
        }

        static int Counter(string key) => PlayerPrefs.GetInt("fase_cuenta_" + key, 0);

        static int AddCounter(string key, int amount = 1)
        {
            int value = Counter(key) + amount;
            PlayerPrefs.SetInt("fase_cuenta_" + key, value);
            return value;
        }

        /// <summary>Una Sombra dormida se quedó atrás (lo llama Monster).</summary>
        public static void PassedSleepingShadow()
        {
            if (AddCounter("sombras") >= 10) Unlock("piedra");
        }

        // ───────── Seguimiento ─────────

        static Achievements instance;
        readonly Queue<string> queue = new Queue<string>();
        bool diedThisLevel;
        float onLine;
        PlayerController player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (instance != null) return;
            instance = new GameObject("Logros").AddComponent<Achievements>();
            DontDestroyOnLoad(instance.gameObject);
        }

        void OnEnable()
        {
            GameEvents.Died += OnDied;
            GameEvents.GoalReached += OnGoal;
            GameEvents.ChestOpened += OnChest;
            GameEvents.Jumped += OnJumped;
            GameEvents.PieceVanished += OnPieceVanished;
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(ShowToasts());
        }

        void OnDisable()
        {
            GameEvents.Died -= OnDied;
            GameEvents.GoalReached -= OnGoal;
            GameEvents.ChestOpened -= OnChest;
            GameEvents.Jumped -= OnJumped;
            GameEvents.PieceVanished -= OnPieceVanished;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            diedThisLevel = false;
            onLine = 0f;
            player = FindAnyObjectByType<PlayerController>();
        }

        void OnDied() => diedThisLevel = true;
        void OnChest()
        {
            Unlock("cofre");
            // El cofre trae una página del diario de Lumi.
            var hud = Hud.Instance;
            if (hud != null && !hud.Daily && !Diary.Has(hud.levelNumber))
            {
                Diary.Find(hud.levelNumber);
                if (Diary.Found >= 5) Unlock("lector");
                queue.Enqueue("__diario");
            }
        }

        readonly System.Collections.Generic.Queue<float> recentJumps = new System.Collections.Generic.Queue<float>();
        float nightTime;

        void OnJumped()
        {
            if (AddCounter("saltos") >= 500) Unlock("saltos");
            recentJumps.Enqueue(Time.time);
            while (recentJumps.Count > 0 && Time.time - recentJumps.Peek() > 8f) recentJumps.Dequeue();
            if (recentJumps.Count >= 15) Unlock("saltarin");
            // Saltar justo cuando el suelo bajo los pies va a desaparecer cuenta en OnPieceVanished.
            lastJump = Time.time;
        }

        float lastJump = -10f;

        void OnPieceVanished(Vector3 at, World world)
        {
            if (player == null) return;
            Vector3 p = player.transform.position;
            // La pieza estaba justo bajo Lumi y saltó hace muy poco: se salvó por los pelos.
            if (Time.time - lastJump < 0.35f && Mathf.Abs(p.x - at.x) < 1.5f && p.y > at.y && p.y - at.y < 3f)
            {
                Unlock("equilibrista");
                TimeControl.SlowMotion(0.45f, 0.35f);
            }
        }

        void OnGoal()
        {
            var hud = Hud.Instance;
            if (hud == null) return;
            if (hud.Mirror) Unlock("espejo");
            if (hud.sandbox) return;
            if (hud.Daily) { Unlock("diario"); return; }
            int level = hud.levelNumber;
            var catalog = LevelCatalog.Get;
            var entry = catalog.At(level);
            if (level == 1) Unlock("amanecer");
            if (!diedThisLevel) Unlock("sinmorir");
            if (entry != null && entry.secret) Unlock("secreto");
            // Progress.Complete ya guardó el nivel cuando llega este aviso.
            if (catalog.ChapterDone(1)) Unlock("pradera");
            if (catalog.ChapterDone(2)) Unlock("bosque");
            if (catalog.ChapterDone(3)) Unlock("cumbre");
            if (catalog.levels.Length > 0 && catalog.AllFragments()) Unlock("coleccionista");
        }

        /// <summary>Lo llama Hud cuando se guarda la medalla del nivel.</summary>
        public static void CheckMedals()
        {
            int levels = LevelCatalog.Get.levels.Length;
            if (Medals.CountAtLeast(Medals.Gold, levels) >= 5) Unlock("oro");
            if (Medals.CountAtLeast(Medals.Moon, levels) >= 1) Unlock("luna");
        }

        void Update()
        {
            if (player == null || ShiftLine.Instance == null || PauseMenu.Paused) return;
            onLine = Mathf.Abs(player.transform.position.x - ShiftLine.X) < 0.35f ? onLine + Time.deltaTime : 0f;
            if (onLine >= 3f) Unlock("medioymedio");
            nightTime = ShiftLine.IsDay(player.transform.position) ? 0f : nightTime + Time.deltaTime;
            if (nightTime >= 60f) Unlock("noctambulo");
        }

        // ───────── Avisos ─────────

        IEnumerator ShowToasts()
        {
            while (true)
            {
                while (queue.Count == 0) yield return null;
                string id = queue.Dequeue();
                var info = id == "__diario"
                    ? New("__diario", "Página del diario", "Diary page", "Léela en el menú → Diario", "Read it in the menu → Diary")
                    : System.Array.Find(All, a => a.id == id);
                if (info == null) continue;
                yield return Toast(info);
            }
        }

        IEnumerator Toast(Info info)
        {
            var canvas = UiKit.Canvas("AvisoLogro", 50);
            var box = UiKit.Box(canvas.transform, "Aviso", new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(640f, 130f), new Color(0.08f, 0.06f, 0.2f, 0.94f));
            box.rectTransform.pivot = new Vector2(1f, 0f);
            var glow = UiKit.Box(box.transform, "Brillo", new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(120f, 120f), new Color(1f, 0.8f, 0.4f, 0.35f));
            UiKit.Box(box.transform, "Estrella", new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(76f, 76f), new Color(1f, 0.84f, 0.35f));
            UiKit.Text(box.transform, "Icono", "★", new Vector2(0f, 0.5f), new Vector2(70f, 2f), new Vector2(76f, 76f), 46, TextAnchor.MiddleCenter, new Color(0.55f, 0.35f, 0.1f));
            UiKit.Text(box.transform, "Titulo", (info.id == "__diario" ? "" : Loc.Get("achievement") + " ") + info.Name, new Vector2(0f, 0.5f), new Vector2(130f, 22f), new Vector2(490f, 50f), 36, TextAnchor.MiddleLeft, UiKit.Gold).rectTransform.pivot = new Vector2(0f, 0.5f);
            UiKit.Text(box.transform, "Detalle", info.Description, new Vector2(0f, 0.5f), new Vector2(130f, -24f), new Vector2(490f, 40f), 26, TextAnchor.MiddleLeft, UiKit.Soft).rectTransform.pivot = new Vector2(0f, 0.5f);
            DontDestroyOnLoad(canvas.gameObject);

            var rect = box.rectTransform;
            for (float t = 0f; t < 3.6f; t += Time.unscaledDeltaTime)
            {
                float slide = t < 0.35f ? 1f - Mathf.SmoothStep(0f, 1f, t / 0.35f) : t > 3.2f ? Mathf.SmoothStep(0f, 1f, (t - 3.2f) / 0.4f) : 0f;
                rect.anchoredPosition = new Vector2(-40f + slide * 720f, 40f);
                glow.color = new Color(1f, 0.8f, 0.4f, 0.25f + 0.2f * Mathf.Sin(t * 6f));
                yield return null;
            }
            Destroy(canvas.gameObject);
        }
    }
}

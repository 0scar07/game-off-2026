using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Fantasma del récord: graba por dónde va Lumi y, si el nivel se termina en récord, lo guarda.
    /// La próxima vez una Lumi de luz recorre ese camino a la vez que tú.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class Ghost : MonoBehaviour
    {
        const float Step = 0.05f;

        readonly List<Vector2> recording = new List<Vector2>();
        Vector2[] best;
        // Segundos entre puntos del fantasma guardado (los antiguos, 0,05; los nuevos, 0,1).
        float bestStep = Step;
        Transform ghost, friendGhost;
        Vector2[] friend;
        float time, sample;
        bool finished;
        int level;

        void Start()
        {
            // La repetición es la del nivel que se está jugando (en el diario, el Taller o el sin fin no hay).
            LastRun = null;
            if (Hud.Instance == null || Hud.Instance.Daily) { enabled = false; return; }
            level = Hud.Instance.levelNumber;
            best = Load(level, out bestStep);
            if (best != null && best.Length > 1 && Settings.GhostRace) ghost = BuildGhost();
            // Fantasma de un amigo (código pegado en Extras → Con amigos), en rosa.
            friend = FriendGhost.Load(level);
            if (friend != null) friendGhost = BuildCopy(GetComponent<PlayerController>(), new Color(1f, 0.45f, 0.75f, 0.35f));

        }

        void OnEnable() => GameEvents.GoalReached += OnGoal;
        void OnDisable() => GameEvents.GoalReached -= OnGoal;

        /// <summary>Copia del modelo de Lumi pintada con luz azul (sin colisiones ni scripts).</summary>
        Transform BuildGhost() => BuildCopy(GetComponent<PlayerController>(), new Color(0.35f, 0.55f, 1f, 0.35f));

        /// <summary>Copia del modelo de Lumi hecha de luz (sin colisiones ni scripts).</summary>
        public static Transform BuildCopy(PlayerController player, Color color)
        {
            if (player.visual == null) return null;
            var copy = Instantiate(player.visual.gameObject);
            copy.name = "Fantasma";
            foreach (var b in copy.GetComponentsInChildren<MonoBehaviour>()) Destroy(b);
            foreach (var line in copy.GetComponentsInChildren<LineRenderer>()) Destroy(line.gameObject);
            var shader = Shader.Find("Fase/Particle");
            var mat = new Material(shader);
            mat.SetColor("_Color", color);
            mat.SetFloat("_Additive", 1f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_World", 2f);
            mat.renderQueue = 3100;
            foreach (var r in copy.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var anim = copy.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play("Run");
            return copy.transform;
        }

        void Update()
        {
            if (finished) return;
            time += Time.deltaTime;
            sample += Time.deltaTime;
            if (sample >= Step)
            {
                sample -= Step;
                recording.Add(transform.position);
            }
            if (friendGhost != null)
            {
                float k = time / FriendGhost.Step;
                int j = Mathf.Min((int)k, friend.Length - 2);
                Vector3 q = Vector2.Lerp(friend[j], friend[j + 1], Mathf.Clamp01(k - j));
                q.z = 0.7f;
                friendGhost.position = q + GetComponent<PlayerController>().visual.localPosition;
                float fdx = friend[j + 1].x - friend[j].x;
                if (Mathf.Abs(fdx) > 0.01f) friendGhost.rotation = Quaternion.Euler(0f, -Mathf.Sign(fdx) * 90f, 0f);
                friendGhost.gameObject.SetActive(k < friend.Length - 1);
            }
            if (ghost == null) return;
            float f = time / bestStep;

            int i = Mathf.Min((int)f, best.Length - 2);
            Vector2 a = best[i], b = best[i + 1];
            Vector3 p = Vector2.Lerp(a, b, Mathf.Clamp01(f - i));
            p.z = 0.6f;
            var offset = GetComponent<PlayerController>().visual.localPosition;
            ghost.position = p + offset;
            float dx = b.x - a.x;
            if (Mathf.Abs(dx) > 0.01f) ghost.rotation = Quaternion.Euler(0f, -Mathf.Sign(dx) * 90f, 0f);
            ghost.gameObject.SetActive(time / bestStep < best.Length - 1);
        }

        /// <summary>El recorrido del último nivel terminado (para la repetición).</summary>
        public static Vector2[] LastRun { get; private set; }

        void OnGoal()
        {
            finished = true;
            LastRun = recording.ToArray();
            // El HUD decide si es récord en este mismo evento; se mira un frame después para no
            // depender del orden en que se apuntaron los dos al evento.
            if (isActiveAndEnabled) StartCoroutine(SaveIfRecord());
        }

        System.Collections.IEnumerator SaveIfRecord()
        {
            yield return null;
            var hud = Hud.Instance;
            if (hud != null && hud.IsRecord) Save(level, recording);
        }

        // Un punto cada 0,1 s y como mucho 2 minutos: en la web todo el guardado cabe en 1 MB.
        const float SavedStep = 0.1f;
        const int MaxSavedPoints = 1200;

        static void Save(int level, List<Vector2> points)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder("v2;");
            int every = Mathf.RoundToInt(SavedStep / Step);
            for (int i = 0, n = 0; i < points.Count && n < MaxSavedPoints; i += every, n++)
                sb.Append(points[i].x.ToString("0.0", inv)).Append(',').Append(points[i].y.ToString("0.0", inv)).Append(';');
            // Si el guardado está lleno (en la web hay 1 MB), se queda sin fantasma pero el juego sigue.
            try
            {
                PlayerPrefs.SetString("fase_fantasma_" + level, sb.ToString());
                PlayerPrefs.Save();
            }
            catch (System.Exception e) { Debug.LogWarning("No se pudo guardar el fantasma: " + e.Message); }
        }

        static Vector2[] Load(int level, out float step)
        {
            string data = PlayerPrefs.GetString("fase_fantasma_" + level, "");
            step = Step;
            if (data.Length == 0) return null;
            if (data.StartsWith("v2;")) { step = SavedStep; data = data.Substring(3); }
            var parts = data.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            var points = new Vector2[parts.Length];
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            for (int i = 0; i < parts.Length; i++)
            {
                var xy = parts[i].Split(',');
                points[i] = new Vector2(float.Parse(xy[0], inv), float.Parse(xy[1], inv));
            }
            return points;
        }
    }
}

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
        Transform ghost;
        float time, sample;
        bool finished;
        int level;

        void Start()
        {
            if (Hud.Instance == null || Hud.Instance.Daily) { enabled = false; return; }
            level = Hud.Instance.levelNumber;
            best = Load(level);
            if (best != null && best.Length > 1) ghost = BuildGhost();
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
            if (ghost == null) return;
            float f = time / Step;
            int i = Mathf.Min((int)f, best.Length - 2);
            Vector2 a = best[i], b = best[i + 1];
            Vector3 p = Vector2.Lerp(a, b, Mathf.Clamp01(f - i));
            p.z = 0.6f;
            var offset = GetComponent<PlayerController>().visual.localPosition;
            ghost.position = p + offset;
            float dx = b.x - a.x;
            if (Mathf.Abs(dx) > 0.01f) ghost.rotation = Quaternion.Euler(0f, -Mathf.Sign(dx) * 90f, 0f);
            ghost.gameObject.SetActive(time / Step < best.Length - 1);
        }

        /// <summary>El recorrido del último nivel terminado (para la repetición).</summary>
        public static Vector2[] LastRun { get; private set; }

        void OnGoal()
        {
            finished = true;
            LastRun = recording.ToArray();
            var hud = Hud.Instance;
            if (hud != null && hud.IsRecord) Save(level, recording);
        }

        static void Save(int level, List<Vector2> points)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var p in points) sb.Append(p.x.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                .Append(p.y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(';');
            PlayerPrefs.SetString("fase_fantasma_" + level, sb.ToString());
            PlayerPrefs.Save();
        }

        static Vector2[] Load(int level)
        {
            string data = PlayerPrefs.GetString("fase_fantasma_" + level, "");
            if (data.Length == 0) return null;
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

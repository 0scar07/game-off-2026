using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Mensajes grabados en algunas rocas del decorado ("Lumi estuvo aquí", "L+O"...). Se leen al
    /// acercarse: aparecen poco a poco cuando Lumi pasa al lado. Dos como mucho por nivel, siempre
    /// en las mismas rocas.
    /// </summary>
    public class HiddenMessages : MonoBehaviour
    {
        static readonly (string es, string en)[] Messages =
        {
            ("Lumi estuvo aquí", "Lumi was here"),
            ("L + O", "L + O"),
            ("☾ ☀", "☾ ☀"),
            ("¿Día o noche?", "Day or night?"),
            ("0scar07 · 2026", "0scar07 · 2026"),
            ("La luna era entera", "The moon was whole"),
            ("Mira arriba", "Look up"),
            ("SHIFT", "SHIFT"),
        };

        readonly System.Collections.Generic.List<(Transform rock, CanvasGroup group)> carved = new System.Collections.Generic.List<(Transform, CanvasGroup)>();
        Transform player;
        readonly System.Collections.Generic.HashSet<Transform> read = new System.Collections.Generic.HashSet<Transform>();


        void Start()
        {
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
            var hud = Hud.Instance;
            int level = hud != null ? hud.levelNumber : 0;
            var rocks = new System.Collections.Generic.List<Transform>();
            foreach (var t in FindObjectsByType<Transform>())
                if ((t.name.StartsWith("Rock") || t.name.StartsWith("rock")) && t.parent != null && t.parent.name == "Decoracion") rocks.Add(t);
            rocks.Sort((a, b) => a.position.x.CompareTo(b.position.x));
            var random = new System.Random(level * 31 + 7);
            for (int n = 0; n < 2 && rocks.Count > 0; n++)
            {
                var rock = rocks[random.Next(rocks.Count)];
                rocks.Remove(rock);
                var message = Messages[(level + n * 3) % Messages.Length];
                Carve(rock, Loc.Pick(message.es, message.en));
            }
        }

        void Carve(Transform rock, string message)
        {
            var renderers = rock.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            var go = new GameObject("Grabado", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(400f, 80f);
            rect.position = new Vector3(b.center.x, b.center.y, b.min.z - 0.02f);
            rect.localScale = Vector3.one * 0.0045f;
            var text = UiKit.Text(go.transform, "Texto", message, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 80f), 48, TextAnchor.MiddleCenter, new Color(0.22f, 0.2f, 0.3f, 0.85f));
            Destroy(text.GetComponent<Shadow>());
            Destroy(text.GetComponent<TextScaler>());
            var group = go.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            carved.Add((rock, group));
        }

        void Update()
        {
            if (player == null) return;
            foreach (var (rock, group) in carved)
            {
                if (rock == null || group == null) continue;
                float d = Mathf.Abs(rock.position.x - player.position.x);
                group.alpha = Mathf.MoveTowards(group.alpha, d < 3.5f ? 1f : 0f, Time.deltaTime * 2f);
                // Leído: cuenta para el logro "Arqueólogo" (una vez por roca).
                if (group.alpha >= 1f && !read.Contains(rock))
                {
                    read.Add(rock);
                    string key = $"fase_grabado_{(Hud.Instance != null ? Hud.Instance.levelNumber : 0)}_{Mathf.RoundToInt(rock.position.x)}";
                    if (PlayerPrefs.GetInt(key, 0) == 0)
                    {
                        PlayerPrefs.SetInt(key, 1);
                        int total = PlayerPrefs.GetInt("fase_grabados_leidos", 0) + 1;
                        PlayerPrefs.SetInt("fase_grabados_leidos", total);
                        if (total >= 5) Achievements.Unlock("grabados");
                    }
                }
            }
        }
    }
}

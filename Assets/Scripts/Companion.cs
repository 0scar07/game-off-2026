using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Chispa, el compañero: en cinco niveles de la historia (2, 7, 10, 14 y 17) una lucecita
    /// acompaña a Lumi y le pide que encuentre su estrella perdida, escondida en el nivel. Con las
    /// cinco se gana el farol "Chispa".
    /// </summary>
    public class Companion : MonoBehaviour
    {
        public static readonly int[] Levels = { 2, 7, 10, 14, 17 };

        public static bool Found(int level) => PlayerPrefs.GetInt("fase_chispa_" + level, 0) == 1;
        public static int Count { get { int n = 0; foreach (int l in Levels) if (Found(l)) n++; return n; } }
        public static bool AllFound => Count >= Levels.Length;

        Transform player, orb, star;
        int level;
        bool done;

        void Start()
        {
            var hud = Hud.Instance;
            if (hud == null || hud.Daily || hud.sandbox || System.Array.IndexOf(Levels, hud.levelNumber) < 0) { enabled = false; return; }
            level = hud.levelNumber;
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero == null) { enabled = false; return; }
            player = hero.transform;
            orb = Shapes.Glow(transform, "Chispa", player.position + new Vector3(-1f, 1.6f, -0.6f), new Vector2(0.9f, 0.9f), new Color(1f, 0.95f, 0.7f, 0.9f)).transform;
            Shapes.Glow(orb, "Nucleo", orb.position + Vector3.back * 0.02f, new Vector2(0.3f, 0.3f), Color.white, 2f, false);
            if (Found(level)) { done = true; return; }
            PlaceStar();
            Invoke(nameof(Ask), 8f);   // cuando ya se ha ido la pista del nivel
        }

        void Ask() => LumiBubble.Say(orb, Loc.Pick("¿Me ayudas a encontrar mi estrella?", "Will you help me find my star?"), 3f, 0.8f);

        /// <summary>La estrella, a un tercio del nivel y a la altura de un salto sobre suelo firme.</summary>
        void PlaceStar()
        {
            var shift = ShiftLine.Instance;
            float lo = shift != null ? shift.minX : 0f, hi = shift != null ? shift.maxX : 100f;
            for (float x = Mathf.Lerp(lo, hi, 0.33f); x < hi; x += 1f)
            {
                var hit = Physics2D.Raycast(new Vector2(x, 60f), Vector2.down, 120f);
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.GetComponentInParent<WorldPiece>() != null) continue;
                star = Shapes.Glow(transform, "EstrellaChispa", new Vector3(x, hit.point.y + 2.4f, -0.5f), new Vector2(0.8f, 0.8f), new Color(1f, 0.85f, 1f, 0.95f)).transform;
                return;
            }
        }

        void Update()
        {
            if (player == null) return;
            // Revolotea detrás de Lumi.
            var target = player.position + new Vector3(-1.1f, 1.7f + Mathf.Sin(Time.time * 3f) * 0.25f, -0.6f);
            orb.position = Vector3.Lerp(orb.position, target, Time.deltaTime * 3f);
            if (done || star == null) return;
            star.localScale = Vector3.one * (0.8f + 0.15f * Mathf.Sin(Time.time * 5f));
            if (Vector2.Distance(player.position, star.position) > 1.1f) return;
            done = true;
            PlayerPrefs.SetInt("fase_chispa_" + level, 1);
            PlayerPrefs.Save();
            Destroy(star.gameObject);
            Sfx.Play("pluma", 0.8f, 1.2f);
            LumiBubble.Say(orb, AllFound ? Loc.Pick("¡Gracias! Toma mi luz: ya tienes el farol Chispa.", "Thank you! Take my light: the Spark lantern is yours.")
                                        : Loc.Pick($"¡Mi estrella! Gracias. ({Count}/{Levels.Length})", $"My star! Thank you. ({Count}/{Levels.Length})"), 3f, 0.8f);
        }
    }
}

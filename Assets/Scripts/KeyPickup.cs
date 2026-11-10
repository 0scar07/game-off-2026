using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Llave de un mundo (casillas 1 = de día, 2 = de noche): solo se ve y se coge en su mundo.
    /// Abre una puerta de su mismo mundo (KeyDoor). Lumi puede llevar varias.
    /// </summary>
    public class KeyPickup : Mechanic
    {
        public World world = World.Day;

        public static int DayKeys, NightKeys;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetOnLoad() => UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) => DayKeys = NightKeys = 0;

        void Start()
        {
            float w = world == World.Day ? 0f : 1f;
            var color = world == World.Day ? new Color(1f, 0.85f, 0.35f) : new Color(0.55f, 0.75f, 1.4f);
            Shapes.Glow(transform, "Brillo", transform.position + Vector3.back * 0.5f, new Vector2(1.4f, 1.4f), new Color(color.r, color.g, color.b, 0.4f), w);
            Shapes.Glow(transform, "Llave", transform.position + Vector3.back * 0.55f, new Vector2(0.7f, 0.7f), color, w, false, KeyTexture());
        }

        void Update()
        {
            transform.position += Vector3.up * Mathf.Sin(Time.time * 3f) * 0.003f;
            var player = Player;
            if (player == null || Vector2.Distance(player.transform.position, transform.position) > 1f) return;
            if (ShiftLine.IsDay(transform.position) != (world == World.Day)) return;   // solo en su mundo
            if (world == World.Day) DayKeys++; else NightKeys++;
            Sfx.Play("pluma", 0.7f, world == World.Day ? 1f : 0.8f);
            LumiBubble.Say(player.transform, world == World.Day ? Loc.Pick("¡Llave de día!", "Day key!") : Loc.Pick("¡Llave de noche!", "Night key!"), 1.4f);
            Destroy(gameObject);
        }

        static Texture2D texture;

        /// <summary>Una llave sencilla: aro y dientes.</summary>
        public static Texture2D KeyTexture()
        {
            if (texture != null) return texture;
            const int size = 64;
            texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int py = 0; py < size; py++)
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f, y = (py + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt((x + 0.45f) * (x + 0.45f) + y * y);
                    bool ring = r < 0.38f && r > 0.18f;
                    bool shaft = x > -0.1f && x < 0.85f && Mathf.Abs(y) < 0.08f;
                    bool teeth = (x > 0.5f && x < 0.6f || x > 0.72f && x < 0.82f) && y < 0f && y > -0.3f;
                    texture.SetPixel(px, py, ring || shaft || teeth ? Color.white : Color.clear);
                }
            texture.Apply();
            return texture;
        }
    }
}

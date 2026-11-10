using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Enemigo de la línea (casilla L): una sombra que corre por el suelo hacia donde esté la
    /// línea (si la tiene a menos de 12 casillas). Tocarla hace perder: usa la línea para alejarla.
    /// </summary>
    public class LineChaser : Mechanic
    {
        public float speed = 2.4f;
        Vector3 home;
        Transform art;

        void Start()
        {
            home = transform.position;
            art = Shapes.Glow(transform, "Sombra", transform.position + new Vector3(0f, 0.55f, -0.4f), new Vector2(1.2f, 1.2f), Color.white, 2f, false, ShadowTexture()).transform;
            Shapes.Glow(transform, "Aura", transform.position + new Vector3(0f, 0.55f, -0.3f), new Vector2(2.2f, 2.2f), new Color(0.6f, 0.3f, 1f, 0.35f));
            GameEvents.Died += OnDied;
        }

        void OnDestroy() => GameEvents.Died -= OnDied;
        void OnDied() => transform.position = home;

        void Update()
        {
            var line = ShiftLine.Instance;
            var player = Player;
            if (line == null || PauseMenu.Paused) return;
            float target = line.x;
            if (Mathf.Abs(target - transform.position.x) < 12f)
            {
                float x = Mathf.MoveTowards(transform.position.x, target, speed * Time.deltaTime);
                // No se cae: solo avanza si hay suelo delante.
                var hit = Physics2D.Raycast(new Vector2(x, transform.position.y + 0.3f), Vector2.down, 1f);
                if (hit.collider != null && !hit.collider.isTrigger) transform.position = new Vector3(x, transform.position.y, transform.position.z);
            }
            art.localScale = new Vector3(1f + Mathf.Sin(Time.time * 8f) * 0.05f, 1f - Mathf.Sin(Time.time * 8f) * 0.05f, 1f);
            if (player != null && Vector2.Distance(player.transform.position, transform.position + Vector3.up * 0.55f) < 0.9f) player.Die();
        }

        static Texture2D texture;

        static Texture2D ShadowTexture()
        {
            if (texture != null) return texture;
            const int size = 64;
            texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int py = 0; py < size; py++)
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f, y = (py + 0.5f) / size * 2f - 1f;
                    var c = Color.clear;
                    bool body = x * x / 0.5f + (y + 0.1f) * (y + 0.1f) / 0.75f < 1f || (y > 0.3f && y < 0.85f && Mathf.Abs(Mathf.Abs(x) - 0.4f) < 0.12f - (y - 0.3f) * 0.2f);
                    if (body) c = new Color(0.2f, 0.12f, 0.35f);
                    foreach (float ex in new[] { -0.25f, 0.25f })
                        if ((x - ex) * (x - ex) + (y - 0.15f) * (y - 0.15f) < 0.018f) c = new Color(1f, 0.4f, 0.8f);
                    texture.SetPixel(px, py, c);
                }
            texture.Apply();
            return texture;
        }
    }
}

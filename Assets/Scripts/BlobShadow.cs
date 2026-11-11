using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Sombra suave bajo Lumi, sobre el suelo que tenga debajo: se nota dónde va a caer y no se
    /// pierde sobre la nieve. Se hace más pequeña y clara cuanto más alto está.
    /// </summary>
    public class BlobShadow : MonoBehaviour
    {
        static Texture2D soft;
        Transform player, shadow;
        Material material;

        /// <summary>Punto suave (blanco con borde transparente), creado una vez.</summary>
        public static Texture2D Soft
        {
            get
            {
                if (soft != null) return soft;
                const int size = 64;
                soft = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f)) / (size * 0.5f);
                        soft.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d * d)));
                    }
                soft.Apply();
                return soft;
            }
        }

        void Start()
        {
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero == null) { enabled = false; return; }
            player = hero.transform;
            var go = Shapes.Glow(transform, "SombraLumi", player.position, new Vector2(1.3f, 0.32f), new Color(0.06f, 0.06f, 0.2f, 0.45f), 2f, false, Soft);
            shadow = go.transform;
            material = go.GetComponent<Renderer>().material;
        }

        void LateUpdate()
        {
            if (player == null) return;
            var from = (Vector2)player.position + Vector2.down * 0.5f;
            var hit = Physics2D.Raycast(from, Vector2.down, 8f, ~0);
            // Ignora los disparadores (zonas, peligros) y a la propia Lumi.
            if (hit.collider != null && (hit.collider.isTrigger || hit.collider.transform.IsChildOf(player)))
            {
                var hits = Physics2D.RaycastAll(from, Vector2.down, 8f);
                hit = default;
                foreach (var h in hits) if (!h.collider.isTrigger && !h.collider.transform.IsChildOf(player)) { hit = h; break; }
            }
            bool show = hit.collider != null;
            shadow.gameObject.SetActive(show);
            if (!show) return;
            float height = hit.distance;
            float k = Mathf.Clamp01(1f - height / 6f);
            shadow.position = new Vector3(player.position.x, hit.point.y + 0.03f, 0.35f);
            shadow.localScale = new Vector3(Mathf.Lerp(0.5f, 1.3f, k), Mathf.Lerp(0.14f, 0.32f, k), 1f);
            material.SetColor("_Color", new Color(0.06f, 0.06f, 0.2f, 0.45f * k));
        }
    }
}

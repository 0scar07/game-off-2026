using UnityEngine;

namespace Fase
{
    /// <summary>La pluma (u hoja, o copo) del nivel: flota, brilla y se coge al tocarla.</summary>
    public class Feather : MonoBehaviour
    {
        int level, chapter;
        Transform player;
        Vector3 home;

        public static void Place(Transform parent)
        {
            var hud = Hud.Instance;
            if (hud == null || hud.Daily || hud.sandbox) return;
            var entry = LevelCatalog.Get.At(hud.levelNumber);
            if (entry == null || entry.secret || Feathers.Has(hud.levelNumber)) return;
            var shift = ShiftLine.Instance;
            float lo = shift != null ? shift.minX : 0f, hi = shift != null ? shift.maxX : 100f;
            // A dos tercios del nivel, sobre suelo firme, a la altura de un salto.
            for (float x = Mathf.Lerp(lo, hi, 0.66f); x < hi; x += 1f)
            {
                var hit = Physics2D.Raycast(new Vector2(x, 60f), Vector2.down, 120f);
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.GetComponentInParent<WorldPiece>() != null) continue;
                var go = new GameObject("Coleccionable");
                go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(x, hit.point.y + 2.6f, -0.4f);
                var feather = go.AddComponent<Feather>();
                feather.level = hud.levelNumber;
                feather.chapter = entry.chapter;
                return;
            }
        }

        void Start()
        {
            home = transform.position;
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) return;
            var quad = Shapes.Create(PrimitiveType.Quad, "Dibujo");
            quad.transform.SetParent(transform, false);
            quad.transform.localScale = Vector3.one * 0.7f;
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", Feathers.Texture(chapter));
            mat.SetColor("_Color", Feathers.Tint(chapter));
            mat.SetFloat("_World", 2f);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3010;
            quad.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var glow = Shapes.Create(PrimitiveType.Quad, "Brillo");
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            glow.transform.localScale = Vector3.one * 1.6f;
            var glowMat = new Material(shader);
            glowMat.SetTexture("_MainTex", UiKit.Rounded.texture);
            glowMat.SetColor("_Color", new Color(1f, 0.95f, 0.8f, 0.3f));
            glowMat.SetFloat("_World", 2f);
            glowMat.SetFloat("_Additive", 1f);
            glowMat.renderQueue = 3005;
            glow.GetComponent<MeshRenderer>().sharedMaterial = glowMat;
        }

        void Update()
        {
            transform.position = home + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.15f;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.3f) * 15f);
            if (player == null || Vector2.Distance(player.position, transform.position) > 1.1f) return;
            Feathers.Take(level);
            Sfx.Play("pluma", 0.7f);
            LumiBubble.Say(player, $"{(chapter == 2 ? Loc.Pick("¡Una hoja!", "A leaf!", "Uma folha!", "Une feuille !") : chapter == 3 ? Loc.Pick("¡Un copo!", "A snowflake!", "Um floco!", "Un flocon !") : Loc.Pick("¡Una pluma!", "A feather!", "Uma pena!", "Une plume !"))} {Feathers.Count(chapter)}/{Feathers.Total(chapter)}", 2f);
            Destroy(gameObject);
        }
    }
}

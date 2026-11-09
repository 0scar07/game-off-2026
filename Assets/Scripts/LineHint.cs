using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Ayuda opcional: una línea discontinua y tenue enseña dónde conviene poner la línea para lo
    /// que hay delante (LineAdvisor). Se esconde cuando la línea ya está ahí.
    /// </summary>
    public class LineHint : MonoBehaviour
    {
        LineRenderer dashes;
        PlayerController player;
        float alpha, shown;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            var shader = Shader.Find("Fase/Particle");
            if (shader == null || player == null) { enabled = false; return; }
            var tex = new Texture2D(4, 16, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < 16; y++) for (int x = 0; x < 4; x++) tex.SetPixel(x, y, y < 9 ? Color.white : Color.clear);
            tex.Apply();
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", tex);
            mat.SetFloat("_World", 2f);
            mat.SetFloat("_Additive", 1f);
            mat.renderQueue = 3100;
            var go = new GameObject("PistaLinea");
            go.transform.SetParent(transform, false);
            dashes = go.AddComponent<LineRenderer>();
            dashes.sharedMaterial = mat;
            dashes.textureMode = LineTextureMode.Tile;
            dashes.positionCount = 2;
            dashes.widthMultiplier = 0.12f;
            dashes.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dashes.enabled = false;
        }

        void Update()
        {
            if (dashes == null) return;
            float? x = Settings.LineHint ? LineAdvisor.Suggest(player.transform.position, player.Facing == 0f ? 1f : player.Facing) : null;
            bool there = x.HasValue && Mathf.Abs(ShiftLine.X - x.Value) < 0.6f;
            float target = x.HasValue && !there ? 0.55f : 0f;
            alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * 2f);
            dashes.enabled = alpha > 0.01f;
            if (!dashes.enabled) return;
            if (x.HasValue) shown = Mathf.Lerp(shown == 0f ? x.Value : shown, x.Value, Time.deltaTime * 6f);
            float y = player.transform.position.y;
            dashes.SetPosition(0, new Vector3(shown, y - 8f, -1.5f));
            dashes.SetPosition(1, new Vector3(shown, y + 10f, -1.5f));
            var c = new Color(1f, 0.92f, 0.65f, alpha * (0.75f + 0.25f * Mathf.Sin(Time.time * 4f)));
            dashes.startColor = dashes.endColor = c;
        }
    }
}

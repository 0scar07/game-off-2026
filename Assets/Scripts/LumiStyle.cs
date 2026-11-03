using UnityEngine;

namespace Fase
{
    /// <summary>Pinta la bufanda y el farol de Lumi con lo elegido en Wardrobe.</summary>
    public class LumiStyle : MonoBehaviour
    {
        public LineRenderer scarf;
        public Renderer flame;
        public Renderer glow;

        static readonly int EmissionId = Shader.PropertyToID("_Emission");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        Renderer[] body;
        Color[][] original;

        void Start() => Apply();

        /// <summary>Nox: el cuerpo de Lumi en tonos de noche (morado y azul).</summary>
        void ApplyBody()
        {
            if (body == null)
            {
                var list = new System.Collections.Generic.List<Renderer>();
                foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>()) list.Add(r);
                body = list.ToArray();
                original = new Color[body.Length][];
                for (int i = 0; i < body.Length; i++)
                {
                    var mats = body[i].sharedMaterials;
                    original[i] = new Color[mats.Length];
                    for (int m = 0; m < mats.Length; m++) original[i][m] = mats[m] != null && mats[m].HasProperty(BaseColorId) ? mats[m].GetColor(BaseColorId) : Color.white;
                }
            }
            bool nox = Wardrobe.IsNox;
            var block = new MaterialPropertyBlock();
            for (int i = 0; i < body.Length; i++)
                for (int m = 0; m < original[i].Length; m++)
                {
                    body[i].GetPropertyBlock(block, m);
                    var c = original[i][m];
                    // Blancos a lila, el resto hacia azul noche.
                    var tinted = nox ? new Color(c.r * 0.55f + 0.12f, c.g * 0.45f + 0.08f, c.b * 0.6f + 0.35f, c.a) : c;
                    block.SetColor(BaseColorId, tinted);
                    body[i].SetPropertyBlock(block, m);
                }
        }

        public void Apply()
        {
            ApplyBody();
            var s = Wardrobe.Scarf;
            if (scarf != null)
            {
                scarf.startColor = s.color;
                scarf.endColor = s.color2;
            }
            var l = Wardrobe.Lantern;
            var block = new MaterialPropertyBlock();
            if (flame != null)
            {
                flame.GetPropertyBlock(block);
                block.SetColor(EmissionId, l.color * 1.8f);
                flame.SetPropertyBlock(block);
            }
            if (glow != null)
            {
                block.Clear();
                glow.GetPropertyBlock(block);
                var c = l.color;
                c.a = 0.75f;
                block.SetColor(ColorId, c);
                glow.SetPropertyBlock(block);
            }
        }
    }
}

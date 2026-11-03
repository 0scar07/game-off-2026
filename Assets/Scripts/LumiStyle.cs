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

        void Start() => Apply();

        public void Apply()
        {
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

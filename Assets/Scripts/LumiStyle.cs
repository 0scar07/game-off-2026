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

        GameObject hat;
        TrailRenderer trail;

        /// <summary>Cosas de la tienda: gorro en la cabeza, estela de estrellas y farol de cristal.</summary>
        void ApplyShop()
        {
            if (hat == null)
            {
                Transform head = null;
                foreach (var t in GetComponentsInChildren<Transform>()) if (t.name == "Head") head = t;
                if (head != null)
                {
                    hat = new GameObject("Gorro");
                    hat.transform.SetParent(head, false);
                    var cone = Shapes.Create(PrimitiveType.Cylinder);
                    cone.transform.SetParent(hat.transform, false);
                    cone.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                    cone.transform.localScale = new Vector3(0.62f, 0.22f, 0.62f);
                    var pompom = Shapes.Create(PrimitiveType.Sphere);
                    pompom.transform.SetParent(hat.transform, false);
                    pompom.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                    pompom.transform.localScale = Vector3.one * 0.22f;
                    var source = flame != null ? flame.sharedMaterial : null;
                    foreach (var r in hat.GetComponentsInChildren<Renderer>())
                    {
                        if (source != null) r.sharedMaterial = source;
                        var block = new MaterialPropertyBlock();
                        block.SetColor(BaseColorId, Shop.Items[0].color);
                        block.SetColor(EmissionId, Shop.Items[0].color * 0.25f);
                        r.SetPropertyBlock(block);
                    }
                }
            }
            if (hat != null) hat.SetActive(Shop.Worn("gorro"));

            if (trail == null) trail = GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
                if (trail == null) return;

                trail.time = 0.5f;
                trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(1f, 0f));
                trail.sharedMaterial = scarf != null ? scarf.sharedMaterial : null;
                trail.startColor = new Color(1f, 0.9f, 0.5f, 0.8f);
                trail.endColor = new Color(1f, 0.7f, 0.3f, 0f);
            }
            trail.emitting = Shop.Worn("estela");
        }

        public void Apply()
        {
            ApplyBody();
            ApplyShop();
            var s = Wardrobe.Scarf;
            if (scarf != null)
            {
                scarf.startColor = s.color;
                scarf.endColor = s.color2;
            }
            var l = Wardrobe.Lantern;
            if (Shop.Worn("cristal")) l = new Wardrobe.Item { color = new Color(0.8f, 1.3f, 1.6f) };
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

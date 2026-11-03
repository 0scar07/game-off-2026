using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Huellas de Lumi: en la hierba de día y en la nieve de noche. Cada huella pertenece al mundo
    /// donde se pisó, así que al cruzar la línea desaparece (la recorta el shader) y además se
    /// borra sola con el tiempo.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class Footprints : MonoBehaviour
    {
        public int count = 24;
        public float life = 7f;

        Transform[] prints;
        Material[] materials;
        float[] born;
        int next;
        bool leftFoot;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Start()
        {
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) { enabled = false; return; }
            prints = new Transform[count];
            materials = new Material[count];
            born = new float[count];
            var root = new GameObject("Huellas").transform;
            for (int i = 0; i < count; i++)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(quad.GetComponent<Collider>());
                quad.name = "Huella";
                quad.transform.SetParent(root);
                quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                quad.transform.localScale = new Vector3(0.22f, 0.32f, 1f);
                var mat = new Material(shader);
                mat.SetTexture("_MainTex", UiKit.Rounded.texture);
                mat.SetFloat("_Additive", 0f);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;
                quad.GetComponent<Renderer>().sharedMaterial = mat;
                quad.SetActive(false);
                prints[i] = quad.transform;
                materials[i] = mat;
                born[i] = -100f;
            }
        }

        void OnEnable() => GameEvents.Step += OnStep;
        void OnDisable() => GameEvents.Step -= OnStep;

        void OnStep(bool night)
        {
            if (prints == null) return;
            leftFoot = !leftFoot;
            var p = transform.position;
            // Los pies están en la base de la cápsula; un poco hacia el fondo para no tapar a Lumi.
            var capsule = GetComponent<CapsuleCollider2D>();
            float feet = capsule != null ? capsule.bounds.min.y : p.y - 1f;
            var print = prints[next];
            print.position = new Vector3(p.x, feet + 0.02f, leftFoot ? 0.18f : -0.12f);
            var mat = materials[next];
            mat.SetFloat("_World", night ? 1f : 0f);
            mat.SetColor(ColorId, night ? new Color(0.55f, 0.65f, 0.95f, 0.55f) : new Color(0.12f, 0.3f, 0.12f, 0.35f));
            print.gameObject.SetActive(true);
            born[next] = Time.time;
            next = (next + 1) % count;
        }

        void Update()
        {
            if (prints == null) return;
            for (int i = 0; i < count; i++)
            {
                if (!prints[i].gameObject.activeSelf) continue;
                float age = Time.time - born[i];
                if (age > life) { prints[i].gameObject.SetActive(false); continue; }
                var c = materials[i].GetColor(ColorId);
                c.a = Mathf.Min(c.a, 0.55f * (1f - age / life));
                materials[i].SetColor(ColorId, c);
            }
        }
    }
}

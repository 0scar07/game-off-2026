using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Línea curva (cabecera "curva: radio"): en vez de una recta, un arco. Por dentro es de noche
    /// y por fuera de día. Se mueve igual que la línea normal; el arco pasa por su posición a la
    /// altura media del nivel. Se dibuja con una línea de luz curva.
    /// </summary>
    public class CurveLine : MonoBehaviour
    {
        public float radius = 9f;
        LineRenderer arc;

        void Start()
        {
            var line = ShiftLine.Instance;
            if (line == null) { enabled = false; return; }
            // Dos rectas en el mismo sitio se anulan: todo de día salvo el arco.
            line.hasSecond = true;
            line.dayOnLeft = true;
            foreach (var visual in FindObjectsByType<LineVisual>()) visual.gameObject.SetActive(false);
            var shader = Shader.Find("Fase/Particle");
            arc = new GameObject("Arco").AddComponent<LineRenderer>();
            arc.transform.SetParent(transform, false);
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetTexture("_MainTex", UiKit.Rounded.texture);
                mat.SetFloat("_World", 2f);
                mat.SetFloat("_Additive", 1f);
                mat.renderQueue = 3100;
                arc.sharedMaterial = mat;
            }
            arc.positionCount = 64;
            arc.widthMultiplier = 0.18f;
            arc.startColor = arc.endColor = new Color(1f, 0.97f, 0.9f, 0.95f);
            arc.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void LateUpdate()
        {
            var line = ShiftLine.Instance;
            if (line == null) return;
            line.x2 = line.x;
            var center = new Vector2(line.x + radius, line.referenceY);
            WorldZone.Set(center, radius, true);
            for (int i = 0; i < arc.positionCount; i++)
            {
                float a = Mathf.Lerp(-Mathf.PI * 0.75f, Mathf.PI * 0.75f, i / (arc.positionCount - 1f)) + Mathf.PI;
                arc.SetPosition(i, new Vector3(center.x + Mathf.Cos(a) * radius, center.y + Mathf.Sin(a) * radius, -1.5f));
            }
        }
    }
}

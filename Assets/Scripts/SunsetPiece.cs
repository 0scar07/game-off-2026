using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Pieza de atardecer (casilla A): solo existe en la franja pegada a la línea. Para subir por
    /// ellas hay que ir llevando la línea con Lumi.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class SunsetPiece : Mechanic
    {
        public float band = 1.8f;
        BoxCollider2D box;
        Renderer[] renderers;
        float shown;

        void Start()
        {
            box = GetComponent<BoxCollider2D>();
            Shapes.Block(transform, "Caja", transform.position, new Vector3(1f, 1f, 1.2f), new Color(1f, 0.55f, 0.28f));
            Shapes.Glow(transform, "Brillo", transform.position + Vector3.back * 0.7f, new Vector2(1.8f, 1.8f), new Color(1f, 0.6f, 0.3f, 0.5f));
            renderers = GetComponentsInChildren<Renderer>();
        }

        void FixedUpdate()
        {
            bool on = DistanceToLine(transform.position) < band;
            if (box.enabled != on) box.enabled = on;
        }

        void Update()
        {
            shown = Mathf.MoveTowards(shown, box.enabled ? 1f : 0f, Time.deltaTime * 6f);
            transform.GetChild(0).localScale = Vector3.one * (0.2f + 0.8f * shown);
            foreach (var r in renderers) r.enabled = shown > 0.05f;
        }
    }
}

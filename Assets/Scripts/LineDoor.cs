using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Puerta de la línea (casilla 5): un muro de dos casillas que solo se abre mientras la línea
    /// lo atraviesa. Con la línea inclinada hay que encontrar el sitio justo.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class LineDoor : Mechanic
    {
        BoxCollider2D box;
        Transform panel;
        float open;

        void Start()
        {
            box = GetComponent<BoxCollider2D>();
            panel = Shapes.Block(transform, "Puerta", transform.position, new Vector3(0.9f, 2f, 1f), new Color(0.55f, 0.48f, 0.8f)).transform;
            Shapes.Glow(panel, "Marca", transform.position + new Vector3(0f, 0f, -0.55f), new Vector2(0.15f, 1.7f), new Color(1f, 1f, 1f, 0.8f));
        }

        void FixedUpdate()
        {
            bool through = DistanceToLine(transform.position) < 0.55f;
            box.enabled = !through;
        }

        void Update()
        {
            open = Mathf.MoveTowards(open, box.enabled ? 0f : 1f, Time.deltaTime * 5f);
            panel.localScale = new Vector3(1f, 1f - open * 0.92f, 1f);
            panel.position = transform.position + new Vector3(0f, open * 0.92f, 0f);
        }
    }
}

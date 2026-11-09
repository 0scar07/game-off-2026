using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Brote (casilla V): una planta que crece hacia arriba mientras le da el día (hasta 3 casillas)
    /// y se recoge de noche. Su copa se pisa: sirve de ascensor si la dejas al sol.
    /// </summary>
    public class Sprout : Mechanic
    {
        public float maxHeight = 3f;
        float height;
        BoxCollider2D pad;
        Transform stem, top;

        void Start()
        {
            var go = new GameObject("Copa");
            go.transform.SetParent(transform, false);
            pad = go.AddComponent<BoxCollider2D>();
            pad.size = new Vector2(1.6f, 0.3f);
            stem = Shapes.Block(transform, "Tallo", transform.position, new Vector3(0.18f, 0.1f, 0.18f), new Color(0.3f, 0.7f, 0.35f)).transform;
            top = Shapes.Block(transform, "Hoja", transform.position, new Vector3(1.6f, 0.25f, 1.2f), new Color(0.45f, 0.85f, 0.4f)).transform;
        }

        void FixedUpdate()
        {
            bool day = ShiftLine.IsDay(transform.position);
            height = Mathf.MoveTowards(height, day ? maxHeight : 0f, Time.fixedDeltaTime * (day ? 2.2f : 3.5f));
            Vector3 b = transform.position;   // base: encima de la casilla de suelo
            pad.transform.position = b + Vector3.up * (height + 0.15f);
            pad.enabled = height > 0.3f;
            stem.position = b + Vector3.up * (height * 0.5f);
            stem.localScale = new Vector3(0.18f, Mathf.Max(0.05f, height), 0.18f);
            top.position = b + Vector3.up * (height + 0.15f);
            top.localScale = new Vector3(Mathf.Lerp(0.4f, 1.6f, height / maxHeight), 0.25f, 1.2f);
        }
    }
}

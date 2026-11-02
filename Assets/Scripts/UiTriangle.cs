using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>Triángulo de interfaz que apunta a la derecha (gíralo para otras direcciones).</summary>
    public class UiTriangle : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            vh.AddVert(new Vector3(r.xMin + r.width * 0.15f, r.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin + r.width * 0.15f, r.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMax, r.center.y), color, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
        }
    }
}

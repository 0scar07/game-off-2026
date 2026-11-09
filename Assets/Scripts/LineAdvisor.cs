using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Dónde conviene poner la línea (vertical) para lo que Lumi tiene delante: mira la pieza de
    /// día o de noche más cercana por delante y propone dejarla en su lado. Lo usan la pista de la
    /// línea (LineHint) y el modo de un toque (OneTouchMode). Es una ayuda, no un solucionador.
    /// </summary>
    public static class LineAdvisor
    {
        static WorldPiece[] pieces;

        // Al cambiar de escena se vuelven a buscar las piezas.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Listen() => UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) => pieces = null;

        /// <summary>Posición propuesta para la línea, o null si no hay nada que hacer delante.</summary>
        public static float? Suggest(Vector2 lumi, float facing = 1f)
        {
            var line = ShiftLine.Instance;
            if (line == null || line.angle != 0f || line.hasSecond) return null;
            if (pieces == null) pieces = Object.FindObjectsByType<WorldPiece>();

            WorldPiece best = null;
            float bestDistance = 14f;
            float dir = facing < 0f ? -1f : 1f;
            foreach (var piece in pieces)
            {
                if (piece == null) continue;
                var box = piece.GetComponent<BoxCollider2D>();
                Vector2 center = piece.transform.TransformPoint(box != null ? box.offset : Vector2.zero);
                float ahead = (center.x - lumi.x) * dir;
                if (ahead < -0.5f || Mathf.Abs(center.y - lumi.y) > 5f) continue;
                if (ahead < bestDistance) { bestDistance = ahead; best = piece; }
            }
            if (best == null) return null;
            var b = best.GetComponent<BoxCollider2D>();
            Vector2 c = best.transform.TransformPoint(b.offset);
            float half = Mathf.Abs(best.transform.lossyScale.x) * b.size.x * 0.5f;
            // Las piezas de este mundo tienen que quedar en su lado de la línea.
            bool dayLeft = line.dayOnLeft;
            bool wantLeft = (best.world == World.Day) == dayLeft;   // ¿la pieza tiene que quedar a la izquierda?
            float target = wantLeft ? c.x + half + 0.6f : c.x - half - 0.6f;
            return Mathf.Clamp(target, line.minX, line.maxX);
        }
    }
}

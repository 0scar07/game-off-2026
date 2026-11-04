using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Lumi mira hacia lo importante: la línea mientras se mueve y, si no, el fragmento de luna
    /// más cercano. Gira el hueso de la cabeza después de la animación (LateUpdate), con un
    /// límite para que no se retuerza, y pasa por delante (hacia la cámara) si mira atrás.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public class LumiGaze : MonoBehaviour
    {
        PlayerController player;
        Transform head;
        Collectible[] fragments;
        Quaternion offset = Quaternion.identity;
        float weight, lineTimer;

        void Start()
        {
            player = GetComponent<PlayerController>();
            if (player != null && player.visual != null)
                foreach (var t in player.visual.GetComponentsInChildren<Transform>())
                {
                    string n = t.name.ToLowerInvariant();
                    if (n == "head" || n.EndsWith(":head") || n.EndsWith("_head")) { head = t; break; }
                    if (head == null && n.Contains("head") && !n.Contains("end") && !n.Contains("top")) head = t;
                }
            fragments = FindObjectsByType<Collectible>();
        }

        void LateUpdate()
        {
            if (head == null || player == null) return;
            Vector3? target = null;
            var line = ShiftLine.Instance;
            if (ShiftLine.Speed > 1.5f) lineTimer = 0.8f; else lineTimer -= Time.deltaTime;
            if (line != null && lineTimer > 0f)
            {
                Vector2 n = line.Normal, along = new Vector2(-n.y, n.x);
                Vector2 p0 = line.PointOnLine(line.x);
                target = p0 + along * Vector2.Dot((Vector2)head.position - p0, along);
            }
            else if (fragments != null)
            {
                float best = 7f;
                foreach (var f in fragments)
                {
                    if (f == null || !f.gameObject.activeInHierarchy) continue;
                    float d = Vector2.Distance(f.transform.position, head.position);
                    if (d < best) { best = d; target = f.transform.position; }
                }
            }

            float facing = player.Facing;
            Quaternion desired = Quaternion.identity;
            if (target.HasValue && facing != 0f)
            {
                Vector3 forward = new Vector3(facing, 0f, 0f);
                Vector3 dir = target.Value - head.position;
                dir.z = 0f;
                if (dir.sqrMagnitude > 0.01f)
                {
                    dir.Normalize();
                    // Si queda detrás, gira por delante de la cara (hacia la cámara).
                    if (Vector3.Dot(dir, forward) < 0f) dir = (dir + Vector3.back * 0.8f).normalized;
                    var limited = Vector3.RotateTowards(forward, dir, 55f * Mathf.Deg2Rad, 0f);
                    desired = Quaternion.FromToRotation(forward, limited);
                }
            }
            weight = Mathf.MoveTowards(weight, target.HasValue ? 1f : 0f, Time.deltaTime * 3f);
            offset = Quaternion.Slerp(offset, desired, Time.deltaTime * 8f);
            head.rotation = Quaternion.Slerp(Quaternion.identity, offset, weight) * head.rotation;
        }
    }
}

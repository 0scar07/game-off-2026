using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Mientras Lumi duerme salen "Z" que suben y se apagan encima de ella (y su farol se apaga,
    /// eso lo hace LevelLife). Se despierta sola en cuanto se mueve algo.
    /// </summary>
    public class LumiSleep : MonoBehaviour
    {
        PlayerController player;
        RectTransform canvas;
        Camera cam;
        float next;

        void Start()
        {
            player = GetComponent<PlayerController>();
            cam = Camera.main;
            var c = UiKit.Canvas("Zetas", 6);
            canvas = (RectTransform)c.transform;
        }

        void Update()
        {
            if (player == null || cam == null) return;
            if (!player.Sleeping) { next = 0.4f; return; }
            next -= Time.deltaTime;
            if (next > 0f) return;
            next = 0.9f;
            var screen = cam.WorldToScreenPoint(player.transform.position + new Vector3(0.4f, 0.9f, 0f));
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out var local)) return;
            var z = UiKit.Text(canvas, "Z", Random.value < 0.5f ? "z" : "Z", new Vector2(0.5f, 0.5f), local, new Vector2(80f, 80f), Random.Range(34, 52), TextAnchor.MiddleCenter, new Color(0.85f, 0.9f, 1f));
            z.gameObject.AddComponent<FloatAway>();
        }

        void OnDestroy() { if (canvas != null) Destroy(canvas.gameObject); }
    }
}

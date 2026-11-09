using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Palanca (casilla I): al tocarla, el día y la noche se cambian de lado durante unos
    /// segundos. Un anillo alrededor de la palanca enseña el tiempo que queda.
    /// </summary>
    public class WorldSwitch : Mechanic
    {
        public float duration = 6f;
        float left, cooldown;
        Transform handle, ring;

        void Start()
        {
            Shapes.Block(transform, "Base", transform.position + new Vector3(0f, 0.15f, 0f), new Vector3(0.6f, 0.3f, 0.6f), new Color(0.4f, 0.35f, 0.55f));
            handle = new GameObject("Palanca").transform;
            handle.SetParent(transform, false);
            handle.position = transform.position + new Vector3(0f, 0.3f, 0f);
            Shapes.Block(handle, "Barra", handle.position + new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 0.12f), new Color(1f, 0.85f, 0.4f));
            ring = Shapes.Glow(transform, "Tiempo", transform.position + new Vector3(0f, 0.7f, -0.6f), new Vector2(1.6f, 1.6f), new Color(1f, 0.85f, 0.4f, 0.4f)).transform;
            ring.gameObject.SetActive(false);
        }

        void Update()
        {
            cooldown -= Time.deltaTime;
            var line = ShiftLine.Instance;
            var player = Player;
            if (line == null || player == null) return;
            if (left <= 0f && cooldown <= 0f && Vector2.Distance(player.transform.position, transform.position + Vector3.up * 0.6f) < 1f)
            {
                left = duration;
                line.dayOnLeft = !line.dayOnLeft;
                Sfx.Play("boton", 0.8f, 0.7f);
            }
            if (left > 0f)
            {
                left -= Time.deltaTime;
                ring.gameObject.SetActive(true);
                ring.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.6f, left / duration);
                if (left <= 0f) { line.dayOnLeft = !line.dayOnLeft; cooldown = 0.8f; ring.gameObject.SetActive(false); }
            }
            handle.localRotation = Quaternion.Slerp(handle.localRotation, Quaternion.Euler(0f, 0f, left > 0f ? -35f : 35f), Time.deltaTime * 10f);
        }
    }
}

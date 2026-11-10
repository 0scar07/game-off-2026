using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Plataforma musical (casilla b): aparece y desaparece al ritmo (84 pulsos por minuto, un
    /// compás de cada dos). Las de columnas pares e impares van al revés: se cruza saltando a tiempo.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BeatPlatform : Mechanic
    {
        public float bpm = 84f;
        BoxCollider2D box;
        Transform plank;
        int phase;

        void Start()
        {
            box = GetComponent<BoxCollider2D>();
            phase = Mathf.FloorToInt(transform.position.x) % 2;
            plank = Shapes.Block(transform, "Tabla", transform.position + new Vector3(0f, 0.85f, 0f), new Vector3(1f, 0.3f, 1.2f), new Color(1f, 0.85f, 0.4f)).transform;
            Shapes.Glow(plank, "Nota", transform.position + new Vector3(0f, 0.85f, -0.65f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.5f));
        }

        void FixedUpdate()
        {
            float beat = Time.timeSinceLevelLoad * bpm / 60f;
            // Dos pulsos encendida, dos apagada; las vecinas al revés.
            bool on = ((int)(beat / 2f) + phase) % 2 == 0;
            box.enabled = on;
            // Parpadea el último cuarto antes de irse.
            float into = (beat / 2f) % 1f;
            bool warn = on && into > 0.75f;
            plank.gameObject.SetActive(on && !(warn && (int)(beat * 4f) % 2 == 0));
            plank.localScale = new Vector3(1f, 1f + (1f - Mathf.Clamp01((beat % 1f) * 4f)) * 0.4f, 1f);
        }
    }
}

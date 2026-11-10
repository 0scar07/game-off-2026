using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// Caja que se lleva (casilla m): Lumi la coge al tocarla y la lleva encima de la cabeza de un
    /// mundo a otro; con ↓ (S, o abajo en el mando) la suelta delante y sirve de escalón.
    /// </summary>
    public class Carryable : Mechanic
    {
        static Carryable carried;
        Rigidbody2D body;
        BoxCollider2D box;
        float grabWait;

        void Start()
        {
            body = gameObject.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.mass = 2f;
            box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
            Shapes.Block(transform, "Caja", transform.position, new Vector3(0.9f, 0.9f, 0.9f), new Color(0.9f, 0.7f, 0.45f));
            transform.position += Vector3.up * 0.45f;
        }

        void OnDestroy() { if (carried == this) carried = null; }

        void Update()
        {
            var player = Player;
            if (player == null) return;
            grabWait -= Time.deltaTime;
            if (carried == this)
            {
                transform.position = player.transform.position + Vector3.up * 1.45f;
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                bool drop = (kb != null && (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)) || (pad != null && pad.dpad.down.wasPressedThisFrame);
                if (drop && !UiKit.Typing)
                {
                    carried = null;
                    float side = player.Facing == 0f ? 1f : player.Facing;
                    transform.position = player.transform.position + new Vector3(side * 1.1f, 0f, 0f);
                    body.simulated = true;
                    grabWait = 0.6f;
                }
                return;
            }
            if (carried == null && grabWait <= 0f && Vector2.Distance(player.transform.position + Vector3.up * 0.4f, transform.position) < 0.9f)
            {
                carried = this;
                body.simulated = false;
                LumiBubble.Say(player.transform, Loc.Pick("↓ para soltarla", "↓ to drop it"), 1.5f);
            }
        }
    }
}

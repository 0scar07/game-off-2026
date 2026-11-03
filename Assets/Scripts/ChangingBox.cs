using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Caja que cambia: de día es una caja de madera pesada que se empuja despacio; de noche es un
    /// bloque de hielo que resbala. Se ve una u otra según el lado de la línea donde esté.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class ChangingBox : MonoBehaviour
    {
        Rigidbody2D body;
        PhysicsMaterial2D wood, ice;
        bool wasDay = true;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.mass = 4f;
            wood = new PhysicsMaterial2D("Madera") { friction = 0.9f };
            ice = new PhysicsMaterial2D("Hielo") { friction = 0.02f };
            Apply(true);
        }

        void FixedUpdate()
        {
            bool day = ShiftLine.IsDay(body.position);
            if (day != wasDay) Apply(day);
            // La madera no se desliza sola.
            if (day) body.linearVelocity = new Vector2(body.linearVelocity.x * 0.8f, body.linearVelocity.y);
        }

        void Apply(bool day)
        {
            wasDay = day;
            GetComponent<BoxCollider2D>().sharedMaterial = day ? wood : ice;
            body.mass = day ? 4f : 1.5f;
            body.linearDamping = day ? 2f : 0.05f;
        }
    }
}

using UnityEngine;

namespace Fase
{
    /// <summary>Onda de la Gran Sombra: corre por el suelo y solo existe (y hace daño) de noche.</summary>
    public class BossWave : MonoBehaviour
    {
        public float speed = 6f, direction = -1f, life = 8f;
        PlayerController player;

        void Start() => player = FindAnyObjectByType<PlayerController>();

        void Update()
        {
            transform.position += Vector3.right * direction * speed * Time.deltaTime;
            life -= Time.deltaTime;
            bool night = !ShiftLine.IsDay(transform.position.x);
            // Al entrar en el día se deshace.
            if (!night || life <= 0f) { Destroy(gameObject); return; }
            if (player != null)
            {
                Vector2 d = player.transform.position - transform.position;
                if (Mathf.Abs(d.x) < 0.6f && d.y < 1.2f && d.y > -0.5f) player.Die();
            }
        }
    }
}

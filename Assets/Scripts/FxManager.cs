using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Efectos de partículas que responden a GameEvents: polvo al aterrizar, chispas donde aparece
    /// o desaparece algo al pasar la línea, estallido de luz al coger luna, nube al caer y al
    /// rebotar. Cada efecto es un sistema de partículas plantilla (hijo de este objeto) que se
    /// mueve al sitio y emite unas cuantas.
    /// </summary>
    public class FxManager : MonoBehaviour
    {
        public ParticleSystem dust;
        public ParticleSystem appearDay;
        public ParticleSystem appearNight;
        public ParticleSystem collect;
        public ParticleSystem poof;

        Transform player;

        void Start()
        {
            var controller = FindAnyObjectByType<PlayerController>();
            if (controller != null) player = controller.transform;
        }

        void OnEnable()
        {
            GameEvents.Landed += OnLanded;
            GameEvents.Braked += OnBraked;
            GameEvents.Jumped += OnJumped;
            GameEvents.PieceAppeared += OnAppeared;
            GameEvents.PieceVanished += OnVanished;
            GameEvents.Collected += OnCollected;
            GameEvents.Died += OnDied;
            GameEvents.Bounced += OnJumped;
        }

        void OnDisable()
        {
            GameEvents.Landed -= OnLanded;
            GameEvents.Braked -= OnBraked;
            GameEvents.Jumped -= OnJumped;
            GameEvents.PieceAppeared -= OnAppeared;
            GameEvents.PieceVanished -= OnVanished;
            GameEvents.Collected -= OnCollected;
            GameEvents.Died -= OnDied;
            GameEvents.Bounced -= OnJumped;
        }

        Vector3 Feet => player != null ? player.position + Vector3.down * 0.9f : Vector3.zero;

        void OnLanded(float speed) => Emit(dust, Feet, (int)Mathf.Lerp(4, 18, speed / 20f));
        void OnJumped() => Emit(dust, Feet, 6);
        void OnBraked(Vector3 at, float speed) => Emit(dust, Feet, 10);
        void OnCollected() => Emit(collect, player != null ? player.position : Vector3.zero, 40);
        void OnDied() => Emit(poof, player != null ? player.position : Vector3.zero, 25);

        void OnAppeared(Vector3 at, World world) => Emit(world == World.Night ? appearNight : appearDay, at, 14);
        void OnVanished(Vector3 at, World world) => Emit(world == World.Night ? appearNight : appearDay, at, 6);

        static void Emit(ParticleSystem system, Vector3 at, int count)
        {
            if (system == null) return;
            system.transform.position = at;
            system.Emit(count);
        }
    }
}

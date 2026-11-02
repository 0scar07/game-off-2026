using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Efectos de sonido del juego. Escucha GameEvents y elige el sonido: pasos en hierba de día
    /// y en nieve de noche, un tintineo de cristal cuando aparece algo de noche, un "pluck" cuando
    /// aparece algo de día, clics suaves al mover la línea y una melodía al llegar a la meta.
    /// Varía un poco el tono en cada repetición para que no canse.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        [Header("Personaje")]
        public AudioClip[] stepsDay;
        public AudioClip[] stepsNight;
        public AudioClip jump;
        public AudioClip[] land;
        public AudioClip die;

        [Header("Línea y mundos")]
        public AudioClip lineGrab;
        public AudioClip lineRelease;
        public AudioClip lineTick;
        public AudioClip appearDay;
        public AudioClip appearNight;
        public AudioClip vanish;

        [Header("Meta")]
        public AudioClip goal;

        [Range(0f, 1f)] public float volume = 0.8f;

        const int Voices = 10;
        AudioSource[] sources;
        int next;
        float lastPieceSound;

        void Awake()
        {
            sources = new AudioSource[Voices];
            for (int i = 0; i < Voices; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
            }
        }

        void OnEnable()
        {
            GameEvents.Step += OnStep;
            GameEvents.Jumped += OnJumped;
            GameEvents.Landed += OnLanded;
            GameEvents.Died += OnDied;
            GameEvents.LineGrabbed += OnLineGrabbed;
            GameEvents.LineReleased += OnLineReleased;
            GameEvents.LineTick += OnLineTick;
            GameEvents.PieceAppeared += OnPieceAppeared;
            GameEvents.PieceVanished += OnPieceVanished;
            GameEvents.GoalReached += OnGoal;
        }

        void OnDisable()
        {
            GameEvents.Step -= OnStep;
            GameEvents.Jumped -= OnJumped;
            GameEvents.Landed -= OnLanded;
            GameEvents.Died -= OnDied;
            GameEvents.LineGrabbed -= OnLineGrabbed;
            GameEvents.LineReleased -= OnLineReleased;
            GameEvents.LineTick -= OnLineTick;
            GameEvents.PieceAppeared -= OnPieceAppeared;
            GameEvents.PieceVanished -= OnPieceVanished;
            GameEvents.GoalReached -= OnGoal;
        }

        void OnStep(bool night) => Play(Pick(night ? stepsNight : stepsDay), 0.35f, 0.12f);
        void OnJumped() => Play(jump, 0.45f, 0.08f, 1.15f);
        void OnLanded(float speed) => Play(Pick(land), Mathf.Clamp01(speed / 18f) * 0.8f + 0.2f, 0.1f);
        void OnDied() => Play(die, 0.8f, 0f);
        void OnLineGrabbed() => Play(lineGrab, 0.5f, 0.05f);
        void OnLineReleased() => Play(lineRelease, 0.45f, 0.05f);
        void OnLineTick() => Play(lineTick, 0.18f, 0.15f);
        void OnGoal() => Play(goal, 0.9f, 0f);

        // Muchas piezas pueden cambiar a la vez: como mucho un sonido cada 0,08 s.
        void OnPieceAppeared(Vector3 at, World world)
        {
            if (Time.unscaledTime - lastPieceSound < 0.08f) return;
            lastPieceSound = Time.unscaledTime;
            Play(world == World.Night ? appearNight : appearDay, 0.45f, 0.1f);
        }

        void OnPieceVanished(Vector3 at, World world)
        {
            if (Time.unscaledTime - lastPieceSound < 0.08f) return;
            lastPieceSound = Time.unscaledTime;
            Play(vanish, 0.25f, 0.1f, 0.9f);
        }

        static AudioClip Pick(AudioClip[] clips) =>
            clips == null || clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];

        void Play(AudioClip clip, float clipVolume, float pitchVariation, float pitch = 1f)
        {
            if (clip == null) return;
            var source = sources[next];
            next = (next + 1) % Voices;
            source.pitch = pitch * (1f + Random.Range(-pitchVariation, pitchVariation));
            source.PlayOneShot(clip, clipVolume * volume);
        }
    }
}

using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Música en dos capas: la versión de día y la de noche suenan a la vez, sincronizadas, y el
    /// volumen de cada una depende de cuánta pantalla ocupa su mundo. Al mover la línea la
    /// canción pasa de una versión a la otra sin cortes. Igual con el sonido de ambiente
    /// (pájaros de día, grillos de noche...), que va con el efectos.
    /// </summary>
    public class MusicLayers : MonoBehaviour
    {
        public AudioClip day;
        public AudioClip night;
        public AudioClip ambienceDay;
        public AudioClip ambienceNight;
        [Tooltip("Brillo de campanas que entra cuando Lumi está justo en la línea.")]
        public AudioClip lineLayer;
        [Tooltip("Tambores que suenan mientras la Gran Sombra sigue en pie.")]
        public AudioClip bossLayer;
        [Range(0f, 1f)] public float volume = 0.55f;
        [Range(0f, 1f)] public float ambienceVolume = 0.3f;
        [Tooltip("Lo rápido que sigue el volumen a la línea.")]
        public float smoothing = 6f;

        AudioSource daySource, nightSource, ambienceDaySource, ambienceNightSource, lineSource, bossSource;
        float nightAmount, onLine, bossAmount;
        Transform player;
        Boss boss;

        void Start()
        {
            daySource = CreateSource(day);
            nightSource = CreateSource(night);
            ambienceDaySource = CreateSource(ambienceDay);
            ambienceNightSource = CreateSource(ambienceNight);
            lineSource = CreateSource(lineLayer);
            bossSource = CreateSource(bossLayer);
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
            boss = FindAnyObjectByType<Boss>();
            nightAmount = NightOnScreen();
            Apply();
            // Las dos arrancan en el mismo instante para que vayan al compás.
            double start = AudioSettings.dspTime + 0.2;
            daySource.PlayScheduled(start);
            nightSource.PlayScheduled(start);
            if (ambienceDay != null) ambienceDaySource.PlayScheduled(start);
            if (ambienceNight != null) ambienceNightSource.PlayScheduled(start);
            if (lineLayer != null && player != null) lineSource.PlayScheduled(start);
            if (bossLayer != null && boss != null) bossSource.PlayScheduled(start);
        }

        AudioSource CreateSource(AudioClip clip)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        void Update()
        {
            float k = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            nightAmount = Mathf.Lerp(nightAmount, NightOnScreen(), k);
            // Cuanto más cerca de la línea está Lumi, más suena la capa de la línea.
            float near = player != null && ShiftLine.Instance != null ? 1f - Mathf.Clamp01((Mathf.Abs(player.position.x - ShiftLine.X) - 0.5f) / 2.5f) : 0f;
            onLine = Mathf.Lerp(onLine, near, k * 0.5f);
            bossAmount = Mathf.Lerp(bossAmount, boss != null && boss.isActiveAndEnabled && !boss.Defeated ? 1f : 0f, k * 0.3f);
            Apply();
        }

        /// <summary>Parte de la pantalla que es de noche (0 = toda de día, 1 = toda de noche).</summary>
        static float NightOnScreen()
        {
            var line = ShiftLine.Instance;
            if (line == null) return 0f;
            float screenX = Mathf.Clamp01(line.ScreenX());
            return line.dayOnLeft ? 1f - screenX : screenX;
        }

        // Mezcla de igual potencia: a mitad de pantalla las dos suenan igual de fuerte, sin bajón.
        void Apply()
        {
            float dayGain = Mathf.Cos(nightAmount * Mathf.PI * 0.5f), nightGain = Mathf.Sin(nightAmount * Mathf.PI * 0.5f);
            float master = volume * Progress.MusicVolume, ambience = ambienceVolume * Progress.SfxVolume;
            daySource.volume = dayGain * master;
            nightSource.volume = nightGain * master;
            ambienceDaySource.volume = dayGain * ambience;
            ambienceNightSource.volume = nightGain * ambience;
            lineSource.volume = onLine * master * 0.7f;
            bossSource.volume = bossAmount * master;
        }
    }
}

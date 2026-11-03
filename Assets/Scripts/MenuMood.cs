using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Ambiente del menú: suena la música del capítulo por el que vas y, según la hora de tu reloj,
    /// el menú es más de día (de 7 a 19 h) o más de noche.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class MenuMood : MonoBehaviour
    {
        public AudioClip[] day, night, ambienceDay, ambienceNight;
        public float dayCenter = 28f, nightCenter = 12f;

        void Awake()
        {
            var music = GetComponent<MusicLayers>();
            var entry = LevelCatalog.Get.At(Mathf.Clamp(Progress.Unlocked, 1, Mathf.Max(1, LevelCatalog.Get.MainCount)));
            int chapter = entry != null ? Mathf.Clamp(entry.chapter - 1, 0, day.Length - 1) : 0;
            if (music != null && day.Length > chapter)
            {
                music.day = day[chapter];
                music.night = night[chapter];
                music.ambienceDay = ambienceDay[chapter];
                music.ambienceNight = ambienceNight[chapter];
            }
            int hour = System.DateTime.Now.Hour;
            var line = FindAnyObjectByType<ShiftLine>();
            if (line != null) line.sweepCenter = hour >= 7 && hour < 19 ? dayCenter : nightCenter;
        }
    }
}

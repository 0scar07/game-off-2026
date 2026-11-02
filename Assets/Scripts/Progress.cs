using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Progreso y ajustes guardados en el dispositivo (PlayerPrefs; en la web, en el navegador):
    /// hasta qué nivel está desbloqueado, cuántos fragmentos de luna se cogieron en cada uno y
    /// el volumen de la música y de los efectos.
    /// </summary>
    public static class Progress
    {
        const string UnlockedKey = "fase_desbloqueado";
        const string MusicKey = "fase_musica";
        const string SfxKey = "fase_efectos";

        /// <summary>Último nivel desbloqueado (1 = solo el primero).</summary>
        public static int Unlocked => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey, 1));

        public static int Fragments(int level) => PlayerPrefs.GetInt("fase_luna_" + level, 0);

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, 0.8f);
            set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxKey, 0.9f);
            set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }

        /// <summary>Nivel terminado: desbloquea el siguiente y guarda el mejor número de fragmentos.</summary>
        public static void Complete(int level, int fragments)
        {
            if (level + 1 > Unlocked) PlayerPrefs.SetInt(UnlockedKey, level + 1);
            if (fragments > Fragments(level)) PlayerPrefs.SetInt("fase_luna_" + level, fragments);
            PlayerPrefs.Save();
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}

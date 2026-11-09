using System;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Cuánto se ha jugado hoy (sin contar la pausa). Lo enseña el reloj de la pausa; si pasa de
    /// una hora, sugiere un descanso.
    /// </summary>
    public class PlayToday : MonoBehaviour
    {
        const string DateKey = "fase_hoy_fecha", SecondsKey = "fase_hoy_segundos";

        static float seconds;
        static string date;
        float saveIn = 15f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create() => DontDestroyOnLoad(new GameObject("TiempoDeHoy").AddComponent<PlayToday>().gameObject);

        void Awake()
        {
            date = PlayerPrefs.GetString(DateKey, "");
            seconds = date == Today ? PlayerPrefs.GetFloat(SecondsKey, 0f) : 0f;
            date = Today;
        }

        static string Today => DateTime.Now.ToString("yyyy-MM-dd");

        /// <summary>Minutos jugados hoy.</summary>
        public static int Minutes => Mathf.FloorToInt(seconds / 60f);

        void Update()
        {
            if (date != Today) { date = Today; seconds = 0f; }
            if (!PauseMenu.Paused && Application.isFocused) seconds += Time.unscaledDeltaTime;
            saveIn -= Time.unscaledDeltaTime;
            if (saveIn > 0f) return;
            saveIn = 15f;
            PlayerPrefs.SetString(DateKey, date);
            PlayerPrefs.SetFloat(SecondsKey, seconds);
        }
    }
}

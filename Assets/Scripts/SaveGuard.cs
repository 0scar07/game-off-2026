using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Guardado a prueba de fallos. En la web el progreso solo llega al disco del navegador al
    /// llamar a PlayerPrefs.Save(); aquí se guarda cada pocos segundos, al cambiar de pestaña o
    /// minimizar y al cerrar. También recuerda el último punto de control de cada nivel, para
    /// seguir desde ahí si se cierra a mitad (Hud lo usa al empezar el nivel).
    /// </summary>
    public class SaveGuard : MonoBehaviour
    {
        const float Every = 10f;
        float left = Every;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create() => DontDestroyOnLoad(new GameObject("Guardado").AddComponent<SaveGuard>().gameObject);

        void Update()
        {
            left -= Time.unscaledDeltaTime;
            if (left > 0f) return;
            left = Every;
            PlayerPrefs.Save();
        }

        void OnApplicationFocus(bool focus) { if (!focus) PlayerPrefs.Save(); }
        void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
        void OnApplicationQuit() => PlayerPrefs.Save();

        // ───────── Seguir por el último punto de control ─────────

        static string Key(int level) => "fase_seguir_" + level;

        public static void SaveCheckpoint(int level, Vector3 position, float elapsed)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            PlayerPrefs.SetString(Key(level), string.Format(inv, "{0};{1};{2}", position.x, position.y, elapsed));
            PlayerPrefs.Save();
        }

        public static bool TryLoadCheckpoint(int level, out Vector3 position, out float elapsed)
        {
            position = Vector3.zero;
            elapsed = 0f;
            var parts = PlayerPrefs.GetString(Key(level), "").Split(';');
            if (parts.Length != 3) return false;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            position = new Vector3(float.Parse(parts[0], inv), float.Parse(parts[1], inv), 0f);
            elapsed = float.Parse(parts[2], inv);
            return true;
        }

        public static void ClearCheckpoint(int level)
        {
            PlayerPrefs.DeleteKey(Key(level));
            PlayerPrefs.Save();
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// Ajustes de accesibilidad y comodidad: modo asistido, alto contraste, vibración y teclas
    /// configurables. Se guardan en PlayerPrefs como el resto del progreso.
    /// </summary>
    public static class Settings
    {
        public enum Action { Left, Right, Jump, LineLeft, LineRight }

        static readonly Key[] Defaults = { Key.A, Key.D, Key.Space, Key.Q, Key.E };
        // Teclas fijas que siempre funcionan además de las elegidas.
        static readonly Key[][] Always =
        {
            new[] { Key.LeftArrow }, new[] { Key.RightArrow }, new[] { Key.UpArrow, Key.W },
            new[] { Key.LeftShift }, new[] { Key.RightShift },
        };

        // En cooperativo las flechas son de la línea (jugador 2).
        static readonly Key[][] CoopKeys =
        {
            new Key[0], new Key[0], new[] { Key.W }, new[] { Key.LeftArrow }, new[] { Key.RightArrow },
        };

        static readonly int HighContrastId = Shader.PropertyToID("_HighContrast");

        public static event System.Action Changed;

        // En cooperativo, la actividad del segundo mando no cambia quién lleva a Lumi.
        public static Gamepad PlayerGamepad => Coop && Gamepad.all.Count > 0
            ? Gamepad.all[0] : Gamepad.current;

        /// <summary>Salto más largo, más margen para saltar y el juego un poco más lento. Sin medallas.</summary>
        public static bool Assist
        {
            get => PlayerPrefs.GetInt("fase_asistido", 0) == 1;
            set => SetBool("fase_asistido", value);
        }

        /// <summary>Las piezas de noche llevan rayas para no depender del color.</summary>
        public static bool HighContrast
        {
            get => PlayerPrefs.GetInt("fase_contraste", 0) == 1;
            set { SetBool("fase_contraste", value); ApplyShader(); }
        }

        /// <summary>Dos jugadores en el mismo teclado: J1 mueve a Lumi (A/D/W/Espacio), J2 la línea (flechas o ratón).</summary>
        public static bool Coop
        {
            get => PlayerPrefs.GetInt("fase_coop", 0) == 1;
            set => SetBool("fase_coop", value);
        }

        /// <summary>Velocidad de la línea con el teclado o el mando: 0 lenta, 1 normal, 2 rápida.</summary>
        public static int LineSpeed
        {
            get => PlayerPrefs.GetInt("fase_velocidad_linea", 1);
            set { PlayerPrefs.SetInt("fase_velocidad_linea", (value % 3 + 3) % 3); PlayerPrefs.Save(); Changed?.Invoke(); }
        }

        public static float LineSpeedFactor => new[] { 0.6f, 1f, 1.6f }[LineSpeed];

        /// <summary>Menos destellos, temblores y pausas de impacto (accesibilidad).</summary>
        public static bool ReduceEffects
        {
            get => PlayerPrefs.GetInt("fase_menos_efectos", 0) == 1;
            set => SetBool("fase_menos_efectos", value);
        }

        /// <summary>Tamaño del texto: 0 normal, 1 grande, 2 muy grande.</summary>
        public static int TextSize
        {
            get => PlayerPrefs.GetInt("fase_tamano_texto", 0);
            set { PlayerPrefs.SetInt("fase_tamano_texto", (value % 3 + 3) % 3); PlayerPrefs.Save(); Changed?.Invoke(); }
        }

        public static float TextScale => new[] { 1f, 1.15f, 1.3f }[TextSize];

        /// <summary>Ayuda: una línea discontinua enseña dónde conviene poner la línea.</summary>
        public static bool LineHint
        {
            get => PlayerPrefs.GetInt("fase_pista_linea", 0) == 1;
            set => SetBool("fase_pista_linea", value);
        }

        /// <summary>Modo relajado: al caer al vacío, Lumi flota de vuelta al último suelo firme (no cuenta caída).</summary>
        public static bool Relaxed
        {
            get => PlayerPrefs.GetInt("fase_relajado", 0) == 1;
            set => SetBool("fase_relajado", value);
        }

        /// <summary>Todo el juego al 80 % de velocidad.</summary>
        public static bool SlowGame
        {
            get => PlayerPrefs.GetInt("fase_lento", 0) == 1;
            set => SetBool("fase_lento", value);
        }

        /// <summary>Modo de un toque: Lumi avanza sola, la línea se coloca sola y tocar es saltar.</summary>
        public static bool OneTouch
        {
            get => PlayerPrefs.GetInt("fase_un_toque", 0) == 1;
            set => SetBool("fase_un_toque", value);
        }

        /// <summary>Correr contra el fantasma de tu récord (se activa o quita desde El viaje).</summary>

        public static bool GhostRace
        {
            get => PlayerPrefs.GetInt("fase_fantasma", 1) == 1;
            set => SetBool("fase_fantasma", value);
        }

        /// <summary>Icono de sol o luna en las piezas (para no depender del color).</summary>

        public static bool PieceIcons
        {
            get => PlayerPrefs.GetInt("fase_iconos_piezas", 0) == 1;
            set => SetBool("fase_iconos_piezas", value);
        }

        /// <summary>Brillo en pasos: 0 = 80 %, 2 = 100 %, 5 = 130 %.</summary>
        public static int BrightnessStep
        {
            get => PlayerPrefs.GetInt("fase_brillo", 2);
            set { PlayerPrefs.SetInt("fase_brillo", (value % 6 + 6) % 6); PlayerPrefs.Save(); Changed?.Invoke(); }
        }

        public static int BrightnessPercent => 80 + BrightnessStep * 10;
        public static float BrightnessFactor => BrightnessPercent / 100f;


        public static bool Vibration
        {

            get => PlayerPrefs.GetInt("fase_vibracion", 1) == 1;
            set => SetBool("fase_vibracion", value);
        }

        static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            ApplyShader();
            // La web usa el perfil "Mobile" (80 % de resolución, sombras pequeñas). En un ordenador
            // sobra potencia: resolución completa y sombras nítidas. (En el editor no, que
            // cambiaría el archivo del perfil.)
            // En un ordenador, el perfil de calidad "PC" (antialiasing 4x, sombras suaves).
            if (!Application.isEditor && !Application.isMobilePlatform)
            {
                int pc = System.Array.IndexOf(QualitySettings.names, "PC");
                if (pc >= 0 && QualitySettings.GetQualityLevel() != pc) QualitySettings.SetQualityLevel(pc, true);
            }
            if (!Application.isEditor && !Application.isMobilePlatform
                && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
            {
                urp.renderScale = 1f;
                urp.mainLightShadowmapResolution = 2048;
            }
        }

        static void ApplyShader() => Shader.SetGlobalFloat(HighContrastId, HighContrast ? 1f : 0f);

        public static Key GetKey(Action action) => (Key)PlayerPrefs.GetInt("fase_tecla_" + action, (int)Defaults[(int)action]);

        public static void SetKey(Action action, Key key)
        {
            PlayerPrefs.SetInt("fase_tecla_" + action, (int)key);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void ResetKeys()
        {
            foreach (Action action in System.Enum.GetValues(typeof(Action))) PlayerPrefs.DeleteKey("fase_tecla_" + action);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static bool Held(Action action) => Check(action, k => k.isPressed);
        public static bool Pressed(Action action) => Check(action, k => k.wasPressedThisFrame);

        static bool Check(Action action, System.Func<UnityEngine.InputSystem.Controls.KeyControl, bool> test)
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            var key = GetKey(action);
            if (key != Key.None && test(kb[key])) return true;
            // En la carrera a dos, las flechas son del jugador 2: el 1 solo usa sus teclas.
            if (RaceMode.Active) return false;
            foreach (var extra in Coop ? CoopKeys[(int)action] : Always[(int)action]) if (test(kb[extra])) return true;

            return false;
        }

        /// <summary>Nombre corto de una tecla para los botones de Opciones.</summary>
        public static string KeyName(Key key)
        {
            if (key == Key.Space) return Loc.Pick("ESPACIO", "SPACE");
            var kb = Keyboard.current;
            string name = kb != null ? kb[key].displayName : key.ToString();
            return string.IsNullOrEmpty(name) ? key.ToString() : name.ToUpperInvariant();
        }
    }
}

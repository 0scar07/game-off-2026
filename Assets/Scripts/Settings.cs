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

        // ───────── Gráficos ─────────

        /// <summary>Ordenador de verdad (Windows o el editor): ni móvil ni navegador.</summary>
        public static bool IsDesktop => !Application.isMobilePlatform && Application.platform != RuntimePlatform.WebGLPlayer;

        /// <summary>0 = Auto (según el dispositivo), 1 = Baja, 2 = Media, 3 = Alta, 4 = Máxima.</summary>
        public static int GraphicsTier
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("fase_calidad", 0), 0, 4);
            set { PlayerPrefs.SetInt("fase_calidad", (value % 5 + 5) % 5); PlayerPrefs.Save(); ApplyGraphics(); Changed?.Invoke(); }
        }

        public static string GraphicsTierName => Loc.Pick(
            new[] { "Auto", "Baja", "Media", "Alta", "Máxima" }[GraphicsTier],
            new[] { "Auto", "Low", "Medium", "High", "Maximum" }[GraphicsTier],
            new[] { "Auto", "Baixa", "Média", "Alta", "Máxima" }[GraphicsTier],
            new[] { "Auto", "Basse", "Moyenne", "Haute", "Maximale" }[GraphicsTier]);

        /// <summary>Qué hace el nivel de calidad elegido, en una frase.</summary>
        public static string GraphicsTierInfo
        {
            get
            {
                int max = Mathf.RoundToInt(MaxRenderScale * 100f);
                switch (GraphicsTier)
                {
                    case 1: return Loc.Pick("Para equipos modestos: resolución 70 %, sin suavizado de bordes.", "For modest devices: 70 % resolution, no anti-aliasing.", "Para aparelhos modestos: resolução 70 %, sem suavização de bordas.", "Pour les appareils modestes : résolution 70 %, sans anticrénelage.");
                    case 2: return Loc.Pick("Equilibrada: resolución 85 %, suavizado 2x.", "Balanced: 85 % resolution, 2x anti-aliasing.", "Equilibrada: resolução 85 %, suavização 2x.", "Équilibrée : résolution 85 %, anticrénelage 2x.");
                    case 3: return Loc.Pick("Resolución completa, suavizado 4x y sombras nítidas.", "Full resolution, 4x anti-aliasing and sharp shadows.", "Resolução completa, suavização 4x e sombras nítidas.", "Pleine résolution, anticrénelage 4x et ombres nettes.");
                    case 4: return Loc.Pick($"Supermuestreo {max} %, suavizado 4x y sombras muy nítidas. Para equipos potentes.", $"{max} % supersampling, 4x anti-aliasing and very sharp shadows. For powerful devices.", $"Supersampling {max} %, suavização 4x e sombras muito nítidas. Para aparelhos potentes.", $"Suréchantillonnage {max} %, anticrénelage 4x et ombres très nettes. Pour les appareils puissants.");
                    default: return Loc.Pick("Se ajusta solo a tu dispositivo.", "Adjusts itself to your device.", "Ajusta-se sozinha ao seu aparelho.", "S'adapte toute seule à votre appareil.");
                }
            }
        }

        // En el navegador y el móvil, 150 % de resolución cuesta demasiado: el máximo es 125 %.
        static float MaxRenderScale => IsDesktop ? 1.5f : 1.25f;

        /// <summary>Límite de imágenes por segundo: 0 = 30, 1 = 60, 2 = 120, 3 = sin límite.</summary>
        public static int FpsStep
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("fase_fps", 1), 0, 3);
            set { PlayerPrefs.SetInt("fase_fps", (value % 4 + 4) % 4); PlayerPrefs.Save(); ApplyGraphics(); Changed?.Invoke(); }
        }

        public static string FpsName => FpsStep == 3 ? Loc.Pick("Sin límite", "Unlimited", "Sem limite", "Illimité") : new[] { "30", "60", "120" }[FpsStep];

        /// <summary>Sincronización vertical (solo en el ordenador; en el navegador la decide él).</summary>
        public static bool VSync
        {
            get => PlayerPrefs.GetInt("fase_vsync", 1) == 1;
            set { PlayerPrefs.SetInt("fase_vsync", value ? 1 : 0); PlayerPrefs.Save(); ApplyGraphics(); Changed?.Invoke(); }
        }

        static bool wantedFullscreen;
        static int wantedFrame = -10;

        /// <summary>Pantalla completa. El cambio se aplica al frame siguiente, así que se recuerda lo pedido.</summary>
        public static bool Fullscreen
        {
            get => Time.frameCount - wantedFrame < 30 ? wantedFullscreen : Screen.fullScreen;
            set
            {
                wantedFullscreen = value;
                wantedFrame = Time.frameCount;
                if (Application.platform == RuntimePlatform.WebGLPlayer) { Screen.fullScreen = value; return; }
                int w = Display.main.systemWidth, h = Display.main.systemHeight;
                if (value) Screen.SetResolution(w, h, FullScreenMode.FullScreenWindow);
                else Screen.SetResolution(Mathf.RoundToInt(w * 0.75f), Mathf.RoundToInt(h * 0.75f), FullScreenMode.Windowed);
            }
        }

        static readonly System.Collections.Generic.Dictionary<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, (float scale, int msaa, int shadow)> pipelineDefaults
            = new System.Collections.Generic.Dictionary<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, (float, int, int)>();

        /// <summary>
        /// Aplica la calidad elegida: perfil de Unity (Mobile o PC), resolución de render,
        /// suavizado de bordes, sombras, VSync y límite de FPS. En el editor solo toca VSync y FPS,
        /// para no cambiar los archivos del proyecto.
        /// </summary>
        public static void ApplyGraphics()
        {
            int tier = GraphicsTier;
            bool mobile = Application.isMobilePlatform;
            if (!Application.isEditor)
            {
                // Auto: el ordenador (también en el navegador) usa "PC"; el móvil, "Mobile".
                string profile = tier == 0 ? (mobile ? "Mobile" : "PC") : tier <= 2 ? "Mobile" : "PC";
                int index = System.Array.IndexOf(QualitySettings.names, profile);
                if (index >= 0 && QualitySettings.GetQualityLevel() != index) QualitySettings.SetQualityLevel(index, true);
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
                {
                    if (!pipelineDefaults.TryGetValue(urp, out var d))
                        pipelineDefaults[urp] = d = (urp.renderScale, urp.msaaSampleCount, urp.mainLightShadowmapResolution);
                    switch (tier)
                    {
                        case 1: urp.renderScale = 0.7f; urp.msaaSampleCount = 1; urp.mainLightShadowmapResolution = 512; break;
                        case 2: urp.renderScale = 0.85f; urp.msaaSampleCount = 2; urp.mainLightShadowmapResolution = 1024; break;
                        case 3: urp.renderScale = 1f; urp.msaaSampleCount = 4; urp.mainLightShadowmapResolution = 2048; break;
                        case 4: urp.renderScale = MaxRenderScale; urp.msaaSampleCount = 4; urp.mainLightShadowmapResolution = 4096; break;
                        default:
                            // Como antes: en un ordenador, resolución completa y sombras nítidas.
                            urp.renderScale = mobile ? d.scale : 1f;
                            urp.msaaSampleCount = d.msaa;
                            urp.mainLightShadowmapResolution = mobile ? d.shadow : 2048;
                            break;
                    }
                }
            }
            if (IsDesktop)
            {
                QualitySettings.vSyncCount = VSync ? 1 : 0;
                Application.targetFrameRate = VSync || FpsStep == 3 ? -1 : new[] { 30, 60, 120 }[FpsStep];
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // En el navegador (también el del móvil) manda su propio ritmo de pantalla: con otro
                // valor Unity temporiza por software y da tirones.
                Application.targetFrameRate = -1;
            }
            else if (mobile)
            {
                // En el móvil Unity limita a 30 si no se le dice nada.
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = FpsStep == 3 ? Mathf.Max(60, Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value)) : new[] { 30, 60, 120 }[FpsStep];
            }
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
            // Calidad gráfica elegida en Opciones → Gráficos (por defecto, Auto: en un ordenador
            // el perfil "PC" con resolución completa; en el móvil, "Mobile").
            ApplyGraphics();
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

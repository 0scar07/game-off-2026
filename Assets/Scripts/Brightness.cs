using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Ajusta el posprocesado de la escena: el brillo de Opciones (exposición sobre la que trae cada
    /// capítulo) y el gris de la pausa (sin color, salvo la línea, que la pausa vuelve a dibujar
    /// encima). Se aplica al cargar escena y al cambiar el ajuste.
    /// </summary>
    public static class Brightness
    {
        static bool gray;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            SceneManager.sceneLoaded += (_, _) => { gray = false; Apply(); };
            Settings.Changed += Apply;
            Apply();
        }

        /// <summary>Quita el color a la escena (pausa) o lo devuelve.</summary>
        public static void SetGray(bool value)
        {
            if (gray == value) return;
            gray = value;
            Apply();
        }

        static readonly Dictionary<VolumeProfile, (float exposure, float saturation)> original = new Dictionary<VolumeProfile, (float, float)>();

        static void Apply()
        {
            float stops = Mathf.Log(Settings.BrightnessFactor, 2f);
            foreach (var volume in Object.FindObjectsByType<Volume>())
            {
                if (!volume.isGlobal) continue;
                // .profile crea una copia para esta escena: el archivo del perfil no se toca.
                var profile = volume.profile;
                if (profile == null) continue;
                if (!profile.TryGet(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
                if (!original.TryGetValue(profile, out var start))
                {
                    start = (color.postExposure.overrideState ? color.postExposure.value : 0f,
                             color.saturation.overrideState ? color.saturation.value : 0f);
                    original[profile] = start;
                }
                color.postExposure.overrideState = true;
                color.postExposure.value = start.exposure + stops;
                color.saturation.overrideState = true;
                color.saturation.value = gray ? -100f : start.saturation;
                break;
            }
        }
    }
}

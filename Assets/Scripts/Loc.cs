using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Textos del juego en español e inglés. El idioma se elige en Opciones; la primera vez se
    /// toma el del sistema (español si el dispositivo está en español, inglés en otro caso).
    /// </summary>
    public static class Loc
    {
        const string Key = "fase_idioma";

        public static bool English
        {
            get => PlayerPrefs.GetInt(Key, Application.systemLanguage == SystemLanguage.Spanish ? 0 : 1) == 1;
            set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); Changed?.Invoke(); }
        }

        public static event System.Action Changed;

        /// <summary>Elige entre el texto en español y en inglés.</summary>
        public static string Pick(string es, string en) => English && !string.IsNullOrEmpty(en) ? en : es;

        public static string Get(string key) =>
            Texts.TryGetValue(key, out var t) ? (English ? t.en : t.es) : key;

        static readonly Dictionary<string, (string es, string en)> Texts = new Dictionary<string, (string, string)>
        {
            ["play"] = ("Jugar", "Play"),
            ["continue"] = ("Continuar", "Continue"),
            ["journey"] = ("El viaje", "The journey"),
            ["options"] = ("Opciones", "Options"),
            ["back"] = ("Volver", "Back"),
            ["music"] = ("Música", "Music"),
            ["sfx"] = ("Efectos", "Sound"),
            ["language"] = ("Idioma: Español", "Language: English"),
            ["reset"] = ("Borrar progreso", "Reset progress"),
            ["intro"] = ("Ver la historia", "Watch the story"),
            ["tagline"] = ("Mueve la línea. Cambia el mundo.", "Move the line. Shift the world."),
            ["credit"] = ("GitHub Game Off 2026 · tema SHIFT · por 0scar07", "GitHub Game Off 2026 · theme SHIFT · by 0scar07"),
            ["pause"] = ("Pausa", "Paused"),
            ["resume"] = ("Continuar", "Resume"),
            ["restart"] = ("Reiniciar nivel", "Restart level"),
            ["menu"] = ("Menú", "Menu"),
            ["complete"] = ("¡Nivel completado!", "Level complete!"),
            ["allmoon"] = ("Con toda la luna", "With the whole moon"),
            ["record"] = ("¡Nuevo récord!", "New record!"),
            ["moon"] = ("Luna", "Moon"),
            ["best"] = ("Récord", "Best"),
            ["locked"] = ("Completa el nivel anterior", "Finish the previous level"),
            ["chapter"] = ("Capítulo", "Chapter"),
            ["skip"] = ("Saltar ▶", "Skip ▶"),
            ["dawn"] = ("Amaneció", "Dawn"),
            ["onewhole"] = ("Las dos mitades vuelven a ser una.", "The two halves are whole again."),
            ["moontotal"] = ("Luna reunida", "Moon restored"),
            ["tomenu"] = ("Volver al menú", "Back to menu"),
            ["rotate"] = ("Gira el móvil para jugar", "Rotate your phone to play"),
            ["achievement"] = ("¡Logro!", "Achievement!"),
            ["achievements"] = ("Logros", "Achievements"),
            ["next"] = ("Siguiente", "Next"),
            ["retry"] = ("Repetir", "Retry"),
            ["share"] = ("Compartir", "Share"),
            ["shared"] = ("¡Compartido!", "Shared!"),
            ["copied"] = ("¡Copiado! Pégalo donde quieras", "Copied! Paste it anywhere"),
            ["sharefail"] = ("No se pudo compartir", "Couldn't share"),
            ["time"] = ("Tiempo", "Time"),
            ["newmedal"] = ("¡Nueva medalla!", "New medal!"),
            ["assistnomedal"] = ("Modo asistido: sin medallas", "Assist mode: no medals"),
            ["wardrobe"] = ("Lumi", "Lumi"),
            ["wardrobetitle"] = ("Bufandas y faroles", "Scarves and lanterns"),
            ["scarf"] = ("Bufanda", "Scarf"),
            ["lantern"] = ("Farol", "Lantern"),
            ["equip"] = ("Ponérsela", "Wear it"),
            ["equipped"] = ("Puesta", "Wearing"),
            ["assist"] = ("Modo asistido", "Assist mode"),
            ["contrast"] = ("Alto contraste", "High contrast"),
            ["vibration"] = ("Vibración", "Vibration"),
            ["controls"] = ("Controles", "Controls"),
            ["presskey"] = ("Pulsa una tecla...", "Press a key..."),
            ["resetkeys"] = ("Teclas por defecto", "Default keys"),
            ["on"] = ("Sí", "On"),
            ["off"] = ("No", "Off"),
            ["left"] = ("Izquierda", "Left"),
            ["right"] = ("Derecha", "Right"),
            ["jump"] = ("Saltar", "Jump"),
            ["lineleft"] = ("Línea ←", "Line ←"),
            ["lineright"] = ("Línea →", "Line →"),
            ["mirror"] = ("Modo espejo", "Mirror mode"),
            ["daily"] = ("Desafío diario", "Daily challenge"),
            ["secret"] = ("Nivel secreto", "Secret level"),
            ["tip"] = ("Consejo", "Tip"),
            ["energy"] = ("Energía de la línea", "Line energy"),
            ["unlockby"] = ("Se consigue con", "Unlocked by"),
        };
    }
}

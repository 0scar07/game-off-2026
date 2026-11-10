using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Textos del juego en español e inglés. El idioma se elige en Opciones; la primera vez se
    /// toma el del sistema (español si el dispositivo está en español, inglés en otro caso).
    /// </summary>
    public static partial class Loc
    {
        const string Key = "fase_idioma";

        /// <summary>0 español, 1 inglés, 2 portugués, 3 francés.</summary>
        public static int Language
        {
            get
            {
                var system = Application.systemLanguage;
                int guess = system == SystemLanguage.Spanish ? 0 : system == SystemLanguage.Portuguese ? 2 : system == SystemLanguage.French ? 3 : 1;
                return Mathf.Clamp(PlayerPrefs.GetInt(Key, guess), 0, 3);
            }
            set { PlayerPrefs.SetInt(Key, (value % 4 + 4) % 4); PlayerPrefs.Save(); Changed?.Invoke(); }
        }

        /// <summary>Cualquier idioma que no sea el español (portugués y francés usan el inglés si falta algo).</summary>
        public static bool English
        {
            get => Language != 0;
            set => Language = value ? 1 : 0;
        }

        public static event System.Action Changed;

        /// <summary>Elige el texto del idioma actual: español, inglés o (por el texto en español) portugués y francés.</summary>
        public static string Pick(string es, string en)
        {
            int language = Language;
            if (language == 0 || string.IsNullOrEmpty(en)) return es;
            if (language >= 2 && es != null && Translations.TryGetValue(es, out var t)) return language == 2 ? t.pt : t.fr;
            return en;
        }

        public static string Get(string key) =>
            Texts.TryGetValue(key, out var t) ? Pick(t.es, t.en) : key;


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
            ["diary"] = ("Diario", "Diary"),
            ["diarytitle"] = ("Diario de Lumi", "Lumi's diary"),
            ["coop"] = ("2 jugadores", "2 players"),
            ["linespeed"] = ("Velocidad de la línea", "Line speed"),
            ["workshop"] = ("Taller", "Workshop"),
            ["endless"] = ("Sin fin", "Endless"),
            ["shop"] = ("Tienda", "Shop"),
            ["replay"] = ("Repetición", "Replay"),
            ["character"] = ("Personaje", "Character"),
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
            ["reduceeffects"] = ("Menos destellos", "Fewer flashes"),
            ["textsize"] = ("Tamaño del texto", "Text size"),
            ["extras"] = ("Extras", "Extras"),
            ["help"] = ("Ayudas", "Assists"),
            ["tricks"] = ("Nivel de trucos", "Tricks level"),
            ["extralevels"] = ("Niveles extra", "Extra levels"),

            ["linehint"] = ("Pista de la línea", "Line hint"),
            ["relaxed"] = ("Modo relajado", "Relaxed mode"),
            ["slowgame"] = ("Velocidad del juego", "Game speed"),
            ["onetouch"] = ("Un toque", "One touch"),

            ["stats"] = ("Estadísticas", "Statistics"),
            ["soundtrack"] = ("Banda sonora", "Soundtrack"),
            ["gallery"] = ("Galería", "Gallery"),
            ["album"] = ("Álbum", "Album"),
            ["chaptermap"] = ("Mapa de capítulos", "Chapter map"),
            ["weekly"] = ("Retos de la semana", "Weekly challenges"),
            ["pieceicons"] = 
("Formas en las piezas", "Shapes on pieces"),
            ["brightness"] = ("Brillo", "Brightness"),

            ["trivia"] = ("¿Sabías qué?", "Did you know?"),

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

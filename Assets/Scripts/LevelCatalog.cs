using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Lista de niveles del juego (la escribe el generador en Resources/catalogo.json): escena,
    /// nombre, capítulo, fragmentos y tiempos de medalla de cada uno. La usan los logros, las
    /// medallas y los niveles secretos sin tener que cargar las escenas.
    /// </summary>
    [System.Serializable]
    public class LevelCatalog
    {
        [System.Serializable]
        public class Entry
        {
            public string scene, name, nameEn;
            public int chapter, fragments;
            public bool secret;
            public float[] medals = new float[4]; // bronce, plata, oro, luna
        }

        public Entry[] levels = new Entry[0];

        static LevelCatalog loaded;

        public static LevelCatalog Get
        {
            get
            {
                if (loaded != null) return loaded;
                var asset = Resources.Load<TextAsset>("catalogo");
                loaded = asset != null ? JsonUtility.FromJson<LevelCatalog>(asset.text) : new LevelCatalog();
                return loaded;
            }
        }

        /// <summary>Número de niveles de la historia (sin contar los secretos).</summary>
        public int MainCount
        {
            get { int n = 0; foreach (var e in levels) if (!e.secret) n++; return n; }
        }

        public Entry At(int level) => level >= 1 && level <= levels.Length ? levels[level - 1] : null;

        /// <summary>Todos los fragmentos de los niveles de un capítulo (0 = todos los capítulos).</summary>
        public bool AllFragments(int chapter = 0)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                var e = levels[i];
                if (e.secret || (chapter != 0 && e.chapter != chapter)) continue;
                if (Progress.Fragments(i + 1) < e.fragments) return false;
            }
            return true;
        }

        public bool ChapterDone(int chapter)
        {
            int last = 0;
            for (int i = 0; i < levels.Length; i++) if (!levels[i].secret && levels[i].chapter == chapter) last = i + 1;
            return last > 0 && Progress.Unlocked > last;
        }
    }
}

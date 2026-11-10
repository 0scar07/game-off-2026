using System;
using System.IO;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Fantasma de un amigo: tu recorrido de un nivel se convierte en un código de texto que se
    /// comparte (FANT-nivel-...); al pegarlo en Extras → Con amigos, su Lumi de luz rosa corre
    /// contigo en ese nivel. Se guarda un punto cada 0,15 s con una décima de precisión.
    /// </summary>
    public static class FriendGhost
    {
        public const float Step = 0.15f;
        const string Prefix = "FANT-";

        /// <summary>Código para compartir el recorrido (puntos cada 0,05 s, como los graba Ghost).</summary>
        public static string Encode(int level, Vector2[] run)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                int px = 0, py = 0;
                for (int i = 0; i < run.Length; i += 3)
                {
                    int x = Mathf.RoundToInt(run[i].x * 10f), y = Mathf.RoundToInt(run[i].y * 10f);
                    writer.Write((short)Mathf.Clamp(x - px, short.MinValue, short.MaxValue));
                    writer.Write((short)Mathf.Clamp(y - py, short.MinValue, short.MaxValue));
                    px = x; py = y;
                }
            }
            return $"{Prefix}{level}-{Convert.ToBase64String(stream.ToArray())}";
        }

        /// <summary>Guarda el fantasma de un código pegado. Devuelve el nivel, o 0 si no vale.</summary>
        public static int Import(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int start = text.IndexOf(Prefix, StringComparison.Ordinal);
            if (start < 0) return 0;
            string code = text.Substring(start + Prefix.Length).Trim();
            int dash = code.IndexOf('-');
            if (dash <= 0 || !int.TryParse(code.Substring(0, dash), out int level)) return 0;
            string data = code.Substring(dash + 1).Split(' ', '\n', '\r')[0];
            if (Decode(data) == null) return 0;
            PlayerPrefs.SetString("fase_amigo_" + level, data);
            PlayerPrefs.Save();
            return level;
        }

        public static Vector2[] Load(int level)
        {
            string data = PlayerPrefs.GetString("fase_amigo_" + level, "");
            return data.Length == 0 ? null : Decode(data);
        }

        static Vector2[] Decode(string data)
        {
            try
            {
                var bytes = Convert.FromBase64String(data);
                var points = new Vector2[bytes.Length / 4];
                using var reader = new BinaryReader(new MemoryStream(bytes));
                int x = 0, y = 0;
                for (int i = 0; i < points.Length; i++)
                {
                    x += reader.ReadInt16();
                    y += reader.ReadInt16();
                    points[i] = new Vector2(x / 10f, y / 10f);
                }
                return points.Length > 1 ? points : null;
            }
            catch (Exception) { return null; }
        }
    }
}

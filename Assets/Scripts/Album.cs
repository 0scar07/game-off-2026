using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Álbum: las últimas 9 fotos del modo foto, en pequeño (320×180, JPG) dentro del guardado del
    /// juego, para verlas en Extras → Álbum. La foto grande se sigue descargando como siempre.
    /// </summary>
    public static class Album
    {
        public const int Size = 9;
        const int W = 320, H = 180;

        public static void Add(Texture2D shot)
        {
            if (shot == null) return;
            var rt = RenderTexture.GetTemporary(W, H, 0);
            Graphics.Blit(shot, rt);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var small = new Texture2D(W, H, TextureFormat.RGB24, false);
            small.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            small.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            int next = PlayerPrefs.GetInt("fase_album_siguiente", 0);
            PlayerPrefs.SetString("fase_album_" + next, System.Convert.ToBase64String(small.EncodeToJPG(80)));
            PlayerPrefs.SetInt("fase_album_siguiente", (next + 1) % Size);
            PlayerPrefs.Save();
            Object.Destroy(small);
        }

        /// <summary>Foto número i (0 = la más reciente); null si no hay.</summary>
        public static Texture2D Get(int i)
        {
            int next = PlayerPrefs.GetInt("fase_album_siguiente", 0);
            int slot = ((next - 1 - i) % Size + Size) % Size;
            string data = PlayerPrefs.GetString("fase_album_" + slot, "");
            if (string.IsNullOrEmpty(data)) return null;
            var tex = new Texture2D(2, 2);
            return tex.LoadImage(System.Convert.FromBase64String(data)) ? tex : null;
        }
    }
}

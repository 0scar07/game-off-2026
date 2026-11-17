using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Fase
{
    /// <summary>
    /// Nivel de la semana: un nivel del Taller elegido a mano que el juego descarga del repositorio
    /// público (semana/nivel.txt en la rama principal). Cambiarlo es editar ese archivo en GitHub:
    /// línea 1 «nombre | name», línea 2 el autor y línea 3 el código FASE-.
    /// Los jugadores mandan los suyos con la plantilla de issue «Nivel de la comunidad».
    /// </summary>
    public static class WeeklyLevel
    {
        public const string Repo = "https://github.com/0scar07/game-off-2026";
        const string Url = "https://raw.githubusercontent.com/0scar07/game-off-2026/fase/semana/nivel.txt";

        /// <summary>Página para enviar un nivel propio (plantilla de issue).</summary>
        public const string SubmitUrl = Repo + "/issues/new?template=nivel.yml";

        public enum State { Idle, Loading, Ready, Failed }

        public static State Status { get; private set; }
        public static string NameEs { get; private set; } = "";
        public static string NameEn { get; private set; } = "";
        public static string Author { get; private set; } = "";
        public static string Code { get; private set; } = "";

        public static string Name => Loc.Pick(NameEs, NameEn);

        /// <summary>Descarga el nivel (una vez por sesión; si falló, lo vuelve a intentar).</summary>
        public static IEnumerator Fetch()
        {
            if (Status == State.Loading || Status == State.Ready) yield break;
            Status = State.Loading;
            using (var request = UnityWebRequest.Get(Url))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();
                // Un nivel de la semana ocupa unos cientos de bytes: un archivo enorme se rechaza.
                if (request.result != UnityWebRequest.Result.Success || request.downloadHandler.data == null
                    || request.downloadHandler.data.Length > 8192 || !Parse(request.downloadHandler.text))
                {
                    Status = State.Failed;
                    yield break;
                }
            }
            Status = State.Ready;
        }

        static bool Parse(string text)
        {
            var lines = text.Replace("\r", "").Split('\n');
            if (lines.Length < 3) return false;
            string code = lines[2].Trim();
            if (LevelCode.Decode(code) == null) return false;
            var names = lines[0].Split('|');
            NameEs = Online.SafeName(names[0], 40);
            NameEn = names.Length > 1 ? Online.SafeName(names[1], 40) : NameEs;
            Author = Online.SafeName(lines[1], 24);
            Code = code;
            return true;
        }

        /// <summary>Abre el Taller con el nivel de la semana y lo empieza a jugar.</summary>
        public static void Play()
        {
            if (Status != State.Ready) return;
            LinkOpener.PendingCode = Code;
            LinkOpener.PlayNow = true;
            SceneFader.Load("Taller");
        }
    }
}

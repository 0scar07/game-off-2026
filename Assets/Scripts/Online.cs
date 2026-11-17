using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Fase
{
    /// <summary>
    /// Conexión con Supabase (base de datos gratuita) para los récords en línea y la analítica
    /// anónima. Las tablas y sus reglas están en Tools/supabase.sql. Si Url o Key están vacíos,
    /// todo lo en línea queda apagado y el juego funciona igual.
    /// La clave es la pública ("anon"): las reglas de la base de datos solo dejan leer los récords y
    /// añadir filas, nunca cambiar ni borrar.
    /// </summary>
    public static class Online
    {
        public const string Url = "https://nxbhkkyesztocwsowvei.supabase.co";
        public const string Key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im54Ymhra3llc3p0b2N3c293dmVpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTQ4NTk5OTIsImV4cCI6MjExMDQzNTk5Mn0.XiZhKRsRm0MjrfdrmNmfj3Kg4u2vHWV_AyqE547crPE";

        public static bool Enabled => Url.Length > 0 && Key.Length > 0;

        static UnityWebRequest Post(string path, string json)
        {
            var request = new UnityWebRequest(Url + path, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 10,
            };
            request.SetRequestHeader("apikey", Key);
            request.SetRequestHeader("Authorization", "Bearer " + Key);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=minimal");
            return request;
        }

        /// <summary>Envía y llama a done(texto de respuesta o null si falló).</summary>
        public static IEnumerator Send(string path, string json, Action<string> done = null)
        {
            if (!Enabled) { done?.Invoke(null); yield break; }
            using (var request = Post(path, json))
            {
                yield return request.SendWebRequest();
                done?.Invoke(request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null);
            }
        }

        public static string Escape(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s ?? "")
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c < 32) sb.Append(' ');
                else sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Corredor invisible para las peticiones (sobrevive a los cambios de escena).</summary>
        public static void Run(IEnumerator routine)
        {
            if (runner == null)
            {
                // Al cerrar el juego (o salir de Play) no se crean objetos nuevos.
                if (quitting) return;
                var go = new GameObject("EnLinea") { hideFlags = HideFlags.HideInHierarchy };
                UnityEngine.Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<UiRunner>();
            }
            runner.StartCoroutine(routine);
        }

        static UiRunner runner;
        static bool quitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            quitting = false;
            Application.quitting += () => quitting = true;
        }

        /// <summary>Crea el corredor ya, para no tener que crearlo mientras se cierra una escena.</summary>
        public static void Warm() { if (Enabled) Run(Idle()); }

        static IEnumerator Idle() { yield break; }
    }

    /// <summary>Récords en línea (idea 80): los 10 mejores tiempos de cada nivel, uno por nombre.</summary>
    public static class OnlineRecords
    {
        [Serializable] class Row { public string name; public int time_ms; }
        [Serializable] class Rows { public Row[] items; }

        /// <summary>Sube el tiempo (si hay nombre) y luego trae la tabla del nivel.</summary>
        public static IEnumerator SubmitAndFetch(int level, float seconds, bool submit, Action<List<(string name, float seconds)>> done)
        {
            if (submit && PlayerName.Has)
            {
                string name = PlayerName.Value.Length > 12 ? PlayerName.Value.Substring(0, 12) : PlayerName.Value;
                int ms = Mathf.RoundToInt(seconds * 1000f);
                yield return Online.Send("/rest/v1/records", $"{{\"level\":{level},\"name\":\"{Online.Escape(name)}\",\"time_ms\":{ms}}}");
            }
            string reply = null;
            yield return Online.Send("/rest/v1/rpc/top_records", $"{{\"lvl\":{level}}}", r => reply = r);
            if (reply == null) { done(null); yield break; }
            var list = new List<(string, float)>();
            try
            {
                var rows = JsonUtility.FromJson<Rows>("{\"items\":" + reply + "}");
                if (rows?.items != null) foreach (var row in rows.items) list.Add((row.name, row.time_ms / 1000f));
            }
            catch { done(null); yield break; }
            done(list);
        }
    }

    /// <summary>
    /// Analítica anónima (idea 140): qué niveles se empiezan y terminan y dónde se muere, sin
    /// nombres ni identificadores. Se puede apagar en Opciones → Controles. Los eventos se
    /// juntan y se mandan de una vez al terminar o salir del nivel.
    /// </summary>
    public static class Analytics
    {
        const string OptKey = "fase_analitica";

        public static bool Allowed
        {
            get => PlayerPrefs.GetInt(OptKey, 1) == 1;
            set { PlayerPrefs.SetInt(OptKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // Las pruebas automáticas de la web (Tools/prueba_web) abren el juego con ?prueba=1.
        static bool AutomatedTest => Application.absoluteURL.Contains("prueba=1");

        static readonly List<string> pending = new List<string>();
        static int level;
        static float started;
        static bool finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            GameEvents.Died += () => Add("death");
            GameEvents.GoalReached += () => { Add("finish"); finished = true; Flush(); };
            Online.Warm();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => OnScene();
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += scene => { if (level > 0) { Add("quit"); Flush(); level = 0; } };
            OnScene();
        }

        static void OnScene()
        {
            var hud = Hud.Instance;
            level = hud != null && !hud.sandbox && !hud.Daily ? hud.levelNumber : 0;
            started = Time.time;
            finished = false;
            if (level > 0) Add("start");
        }

        static void Add(string kind)
        {
            // Las pruebas en el editor no cuentan: solo se analizan partidas de verdad.
            if (level <= 0 || !Allowed || !Online.Enabled || Application.isEditor || AutomatedTest) return;
            // "quit" solo cuenta si se sale sin terminar el nivel.
            if (kind == "quit" && finished) return;
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            string x = player != null ? player.transform.position.x.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) : "null";
            string secs = (Time.time - started).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            pending.Add($"{{\"level\":{level},\"kind\":\"{kind}\",\"x\":{x},\"seconds\":{secs}}}");
        }

        static void Flush()
        {
            if (pending.Count == 0) return;
            string json = "[" + string.Join(",", pending) + "]";
            pending.Clear();
            Online.Run(Online.Send("/rest/v1/events", json));
        }
    }
}

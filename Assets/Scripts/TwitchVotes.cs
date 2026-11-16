using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Twitch: el chat de un canal vota hacia dónde va la línea. Se entra al chat como invitado
    /// anónimo (solo lectura, sin cuenta ni claves). Cada pocos segundos se cuentan los votos
    /// (!izquierda / !derecha, !left / !right, &lt; / &gt;) y la línea se desliza hacia el lado que
    /// gana. Quien juega sigue moviendo a Lumi (y puede mover la línea él también).
    /// </summary>
    public class TwitchVotes : MonoBehaviour
    {
        const string ChannelKey = "fase_twitch";
        const float Round = 3f, Step = 2.5f;

        public static string Channel
        {
            get => PlayerPrefs.GetString(ChannelKey, "");
            set { PlayerPrefs.SetString(ChannelKey, (value ?? "").Trim().TrimStart('#').ToLowerInvariant()); PlayerPrefs.Save(); }
        }

        public static TwitchVotes Instance { get; private set; }
        public static bool On => Instance != null;

        /// <summary>0 apagado, 1 conectando, 2 en el canal, 3 error.</summary>
        public int Status { get; private set; }
        public int Left { get; private set; }
        public int Right { get; private set; }
        public float RoundLeft => Mathf.Max(0f, Round - timer);

        float timer, target = float.NaN;
        readonly System.Collections.Generic.HashSet<string> voted = new System.Collections.Generic.HashSet<string>();
        Text overlay;
        GameObject overlayRoot;

        public static void Begin(string channel)
        {
            End();
            Channel = channel;
            if (string.IsNullOrEmpty(Channel)) return;
            var go = new GameObject("Twitch");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<TwitchVotes>();
            Instance.Connect(Channel);
        }

        public static void End()
        {
            if (Instance == null) return;
            Instance.Disconnect();
            Destroy(Instance.gameObject);
            Instance = null;
        }

        // ───────── Conexión ─────────

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void FaseTwitchOpen(string channel);
        [DllImport("__Internal")] static extern void FaseTwitchClose();
        [DllImport("__Internal")] static extern int FaseTwitchStatus();
        [DllImport("__Internal")] static extern string FaseTwitchPoll();

        void Connect(string channel) { Status = 1; FaseTwitchOpen(channel); }
        void Disconnect() => FaseTwitchClose();

        void Pump()
        {
            Status = FaseTwitchStatus();
            string text = FaseTwitchPoll();
            if (!string.IsNullOrEmpty(text)) foreach (var line in text.Split('\n')) Handle(line);
        }
#else
        readonly ConcurrentQueue<string> lines = new ConcurrentQueue<string>();
        CancellationTokenSource cancel;
        volatile int netStatus;

        void Connect(string channel)
        {
            Status = netStatus = 1;
            cancel = new CancellationTokenSource();
            var token = cancel.Token;
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    using (var socket = new System.Net.WebSockets.ClientWebSocket())
                    {
                        await socket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), token);
                        async System.Threading.Tasks.Task Send(string message) =>
                            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), System.Net.WebSockets.WebSocketMessageType.Text, true, token);
                        await Send("PASS SCHMOOPIIE");
                        await Send("NICK justinfan" + new System.Random().Next(10000, 90000));
                        await Send("JOIN #" + channel);
                        var buffer = new byte[8192];
                        var pending = new StringBuilder();
                        while (!token.IsCancellationRequested && socket.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close) break;
                            pending.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            if (!result.EndOfMessage) continue;
                            foreach (var line in pending.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (line.StartsWith("PING")) { await Send("PONG :tmi.twitch.tv"); continue; }
                                if (line.Contains(" JOIN #")) netStatus = 2;
                                if (lines.Count < 500) lines.Enqueue(line);
                            }
                            pending.Clear();
                        }
                    }
                    if (!token.IsCancellationRequested) netStatus = 3;
                }
                catch
                {
                    if (!token.IsCancellationRequested) netStatus = 3;
                }
            });
        }

        void Disconnect() => cancel?.Cancel();

        void Pump()
        {
            Status = netStatus;
            while (lines.TryDequeue(out var line)) Handle(line);
        }
#endif

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Disconnect();
        }

        // ───────── Votos ─────────

        /// <summary>":nombre!nombre@... PRIVMSG #canal :mensaje" → un voto por persona y ronda.</summary>
        void Handle(string line)
        {
            int at = line.IndexOf(" PRIVMSG #", StringComparison.Ordinal);
            if (at < 0 || !line.StartsWith(":")) return;
            int colon = line.IndexOf(" :", at + 10, StringComparison.Ordinal);
            if (colon < 0) return;
            string user = line.Substring(1, Math.Max(0, line.IndexOf('!') - 1));
            string message = line.Substring(colon + 2).Trim().ToLowerInvariant();
            int vote = Vote(message);
            if (vote == 0 || !voted.Add(user)) return;
            if (vote < 0) Left++; else Right++;
        }

        static int Vote(string m)
        {
            if (m == "<" || m == "◀" || m.StartsWith("!izq") || m.StartsWith("!left") || m == "!l" || m.StartsWith("!esq") || m.StartsWith("!gauche")) return -1;
            if (m == ">" || m == "▶" || m.StartsWith("!der") || m.StartsWith("!right") || m == "!r" || m.StartsWith("!dir") || m.StartsWith("!droite")) return 1;
            return 0;
        }

        void Update()
        {
            Pump();
            var line = ShiftLine.Instance;
            // Solo en los niveles (en el menú también hay línea y una Lumi que pasea).
            bool inLevel = line != null && !line.locked && Hud.Instance != null;
            ShowOverlay(inLevel);
            if (!inLevel) { timer = 0f; Left = Right = 0; voted.Clear(); target = float.NaN; return; }

            timer += Time.unscaledDeltaTime;
            if (timer >= Round)
            {
                timer = 0f;
                if (Left != Right)
                {
                    float from = float.IsNaN(target) ? line.x : target;
                    target = Mathf.Clamp(from + (Right > Left ? Step : -Step), line.minX, line.maxX);
                }
                Left = Right = 0;
                voted.Clear();
            }
            if (!float.IsNaN(target))
            {
                line.x = Mathf.MoveTowards(line.x, target, Time.deltaTime * 6f);
                if (Mathf.Approximately(line.x, target)) target = float.NaN;
            }
            if (overlay != null) overlay.text = Label();
        }

        string Label()
        {
            if (Status == 3) return Loc.Pick("Twitch: no se pudo conectar", "Twitch: couldn't connect", "Twitch: não foi possível conectar", "Twitch : connexion impossible");
            if (Status != 2) return Loc.Pick("Twitch: conectando…", "Twitch: connecting…", "Twitch: conectando…", "Twitch : connexion…");
            return $"#{Channel}   ◀ {Left}   ·   {Right} ▶   ({Mathf.CeilToInt(RoundLeft)})";
        }

        void ShowOverlay(bool show)
        {
            if (show && overlayRoot == null)
            {
                var canvas = UiKit.Canvas("TwitchVotos", 45);
                canvas.transform.SetParent(transform, false);
                var box = UiKit.Box(canvas.transform, "Caja", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(720f, 64f), new Color(0.39f, 0.25f, 0.65f, 0.85f));
                box.rectTransform.pivot = new Vector2(0.5f, 0f);
                overlay = UiKit.Text(box.transform, "Votos", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 60f), 30);
                overlayRoot = canvas.gameObject;
            }
            if (overlayRoot != null && overlayRoot.activeSelf != show) overlayRoot.SetActive(show);
        }
    }
}

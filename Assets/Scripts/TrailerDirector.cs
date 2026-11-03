using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Graba los planos del tráiler desde dentro del juego: carga cada nivel, quita la interfaz,
    /// mueve la cámara y la línea como en una película y guarda cada fotograma (1920×1080, 60 fps,
    /// tiempo fijo: sin tirones aunque el ordenador vaya lento) en Trailer/planos/NN/.
    /// Se lanza desde el menú Fase → Grabar tráiler (solo en el editor).
    /// </summary>
    public class TrailerDirector : MonoBehaviour
    {
        public enum LineMode { Follow, Sweep, Static }

        [System.Serializable]
        public class Shot
        {
            public string scene;
            public float seconds = 4f;
            public float lumiX = 2.5f;
            public bool run;
            [Tooltip("Cámara: posición relativa a Lumi al empezar y al acabar (si followCamera) o absoluta.")]
            public Vector3 camFrom = new Vector3(0f, 1.6f, -14f), camTo = new Vector3(0f, 1.6f, -12f);
            public bool followCamera = true;
            public LineMode line = LineMode.Follow;
            public float lineFrom = 3f, lineTo = 3f;   // Follow: separación respecto a Lumi; Sweep: x absolutas
            public float line2 = -1f;   // < 0 = sin segunda línea
            public float fov = 35f;
        }

        public Shot[] shots;
        public string folder = "Trailer/planos";
        public int width = 1920, height = 1080, fps = 60;

        public static bool Recording { get; private set; }

        [System.Serializable]
        class Plan { public Shot[] shots; }

        /// <summary>Si existe Trailer/grabar.json (lo escribe el menú del editor), graba esos planos.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            const string path = "Trailer/grabar.json";
            if (!Application.isEditor || !File.Exists(path)) return;
            var plan = JsonUtility.FromJson<Plan>(File.ReadAllText(path));
            File.Delete(path);
            Run(plan.shots);
        }

        public static void Run(Shot[] shots)
        {
            var go = new GameObject("DirectorTrailer");
            DontDestroyOnLoad(go);
            var director = go.AddComponent<TrailerDirector>();
            director.shots = shots;
            director.StartCoroutine(director.Record());
        }

        IEnumerator Record()
        {
            Recording = true;
            Time.captureFramerate = fps;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 4 };
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            for (int s = 0; s < shots.Length; s++)
            {
                var shot = shots[s];
                SceneManager.LoadScene(shot.scene);
                yield return null;
                yield return null;
                var player = FindAnyObjectByType<PlayerController>();
                var line = ShiftLine.Instance;
                var cam = Camera.main;
                Prepare(cam, player);
                if (player != null)
                {
                    var hit = Physics2D.Raycast(new Vector2(shot.lumiX, 30f), Vector2.down, 60f);
                    var p = new Vector3(shot.lumiX, hit.collider != null ? hit.point.y + 1f : player.transform.position.y, 0f);
                    player.transform.position = p;
                    player.SetCheckpoint(p);
                    player.GetComponent<Rigidbody2D>().position = p;
                }
                if (line != null)
                {
                    line.autoSweep = false;
                    if (shot.line2 >= 0f) { line.hasSecond = true; line.x2 = shot.line2; }
                    line.minX = -1000f;
                    line.maxX = 1000f;
                }
                cam.fieldOfView = shot.fov;
                cam.targetTexture = rt;
                MobileInput.Move = shot.run ? 1f : 0f;
                // Medio segundo para que todo se asiente (física, partículas) sin grabar.
                for (int i = 0; i < fps / 2; i++) { Place(shot, 0f, cam, player, line); yield return null; }

                string dir = Path.Combine(folder, (s + 1).ToString("00"));
                Directory.CreateDirectory(dir);
                int frames = Mathf.RoundToInt(shot.seconds * fps);
                for (int f = 0; f < frames; f++)
                {
                    Place(shot, f / (float)frames, cam, player, line);
                    yield return new WaitForEndOfFrame();
                    var previous = RenderTexture.active;
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    RenderTexture.active = previous;
                    File.WriteAllBytes(Path.Combine(dir, $"{f:00000}.jpg"), tex.EncodeToJPG(94));
                }
                MobileInput.Move = 0f;
                cam.targetTexture = null;
            }
            Time.captureFramerate = 0;
            Recording = false;
            File.WriteAllText(Path.Combine(folder, "listo.txt"), "ok");
            Debug.Log("Tráiler grabado en " + folder);
            Destroy(gameObject);
        }

        /// <summary>Quita interfaz, pausa automática y seguimiento de cámara.</summary>
        static void Prepare(Camera cam, PlayerController player)
        {
            foreach (var canvas in FindObjectsByType<Canvas>()) canvas.enabled = false;
            foreach (var pause in FindObjectsByType<PauseMenu>()) pause.enabled = false;
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
            if (player != null) player.killHeight = -1000f;
        }

        static void Place(Shot shot, float t, Camera cam, PlayerController player, ShiftLine line)
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            Vector3 anchor = shot.followCamera && player != null ? player.transform.position : Vector3.zero;
            var pos = anchor + Vector3.Lerp(shot.camFrom, shot.camTo, k);
            cam.transform.position = pos;
            // Mira un poco hacia donde va la acción: ligera inclinación, como una grúa.
            cam.transform.rotation = Quaternion.Euler(4f, 0f, 0f);
            if (line == null) return;
            float px = player != null ? player.transform.position.x : 0f;
            switch (shot.line)
            {
                case LineMode.Follow: line.x = px + Mathf.Lerp(shot.lineFrom, shot.lineTo, k); break;
                case LineMode.Sweep: line.x = Mathf.Lerp(shot.lineFrom, shot.lineTo, k); break;
            }
        }
    }
}

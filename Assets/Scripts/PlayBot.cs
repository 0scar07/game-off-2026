using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Jugador automático para las pruebas (Fase → Probar niveles jugando): corre hacia la meta,
    /// salta paredes y huecos y coloca la línea para que exista lo que pisa y lo que viene. No
    /// resuelve puzles difíciles: sirve para ver hasta dónde llega cada nivel sin manos y cazar
    /// niveles rotos, bloqueos y errores. Fuera de esa prueba no hace nada.
    /// </summary>
    public class PlayBot : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public static float Move { get; private set; }
        public static bool JumpPressed { get; private set; }
        public static bool JumpHeld { get; private set; }
        public static bool Debug;

        public static void Enable(bool on)
        {
            Active = on;
            Move = 0f;
            JumpPressed = JumpHeld = false;
        }

        PlayerController player;
        Rigidbody2D body;
        ShiftLine line;
        float jumpHold, jumpPulse, stuckTime, lastProgressX, cooldown;
        Vector3 lastPos;
        Vector2 lastVel;

        void OnEnable() => GameEvents.Died += LogDeath;
        void OnDisable() => GameEvents.Died -= LogDeath;

        void LogDeath()
        {
            if (!Debug || player == null) return;
            var p = lastPos;
            string near = "";
            foreach (var m in FindObjectsByType<Monster>()) if (Vector2.Distance(m.transform.position, p) < 3f) near += $" sombra@{m.transform.position.x:0.0}";
            foreach (var h in FindObjectsByType<Hazard>()) if (Vector2.Distance(h.transform.position, p) < 3f) near += $" {h.name}@{h.transform.position.x:0.0}";
            UnityEngine.Debug.LogWarning($"bot muere en {p} vel={lastVel}{near} dia={ShiftLine.IsDay(p)}");
        }

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            body = player != null ? player.GetComponent<Rigidbody2D>() : null;
            line = ShiftLine.Instance;
            if (player != null) lastProgressX = player.transform.position.x;
        }

        void Update()
        {
            if (!Active) return;
            // Se busca cada vez que falta (el bot puede crearse antes de que cargue el nivel).
            if (player == null) { player = FindAnyObjectByType<PlayerController>(); if (player != null) lastProgressX = player.transform.position.x; }
            if (line == null) line = ShiftLine.Instance;
            if (player == null) return;
            if (Debug && Time.frameCount % 90 == 0) UnityEngine.Debug.LogWarning($"bot vivo linea={(line != null)} bloqueada={(line != null && line.locked)}");
            var p = (Vector2)player.transform.position;
            lastPos = player.transform.position;
            if (body != null) lastVel = body.linearVelocity; else body = player.GetComponent<Rigidbody2D>();
            PlaceLine(p);
            Move = 1f;

            // Saltar: pared delante, hueco delante o atasco.
            bool grounded = Ground(p, 0f);
            bool wall = Solid(Physics2D.Raycast(p + Vector2.down * 0.3f, Vector2.right, 0.9f));
            bool gap = grounded && !Ground(p, 1.3f);
            bool danger = false;
            foreach (var h in FindObjectsByType<Hazard>())
            {
                var hp = (Vector2)h.transform.position;
                float dx = hp.x - p.x;
                if (dx < 0.2f || dx > 2.4f || Mathf.Abs(hp.y - p.y) > 1.6f) continue;
                bool day = ShiftLine.IsDay(hp);
                if (h.active == Hazard.When.Always || (h.active == Hazard.When.DayOnly) == day) { danger = true; break; }
            }
            if (player.transform.position.x > lastProgressX + 0.5f) { lastProgressX = player.transform.position.x; stuckTime = 0f; }
            else stuckTime += Time.deltaTime;
            cooldown -= Time.deltaTime;
            // El salto se mantiene pulsado unos instantes: Lumi lo lee en el paso de física.
            jumpPulse -= Time.deltaTime;
            JumpPressed = jumpPulse > 0f;
            if (grounded && cooldown <= 0f && (wall || gap || danger || stuckTime > 0.8f))
            {
                jumpPulse = 0.08f;
                JumpPressed = true;
                jumpHold = 0.45f;
                cooldown = 0.35f;
                // Si lleva mucho atascado, prueba a retroceder un poco para coger carrerilla.
                if (stuckTime > 3f) { Move = -1f; stuckTime = 1f; }
            }
            jumpHold -= Time.deltaTime;
            JumpHeld = jumpHold > 0f;
        }

        static bool Solid(RaycastHit2D hit) => hit.collider != null && !hit.collider.isTrigger && hit.collider.GetComponentInParent<PlayerController>() == null;

        static bool Ground(Vector2 p, float ahead) => Solid(Physics2D.Raycast(p + new Vector2(ahead, -0.6f), Vector2.down, 3.2f));

        /// <summary>
        /// Busca la posición de la línea más cercana a la actual que deja en su mundo la pieza bajo
        /// los pies, las piezas de delante y las Sombras dormidas (de día).
        /// </summary>
        void PlaceLine(Vector2 p)
        {
            if (line == null || line.locked) return;
            var wants = new List<(Vector2 at, bool day)>();
            foreach (var piece in FindObjectsByType<WorldPiece>())
            {
                // Se mide desde el borde más cercano de la pieza entera (las largas tienen el centro lejos).
                var r = piece.GetComponentInChildren<Renderer>();
                var b = r != null ? r.bounds : new Bounds(piece.transform.position, Vector3.one);
                if (b.max.x < p.x - 1.2f || b.min.x > p.x + 5f || Mathf.Abs(b.center.y - p.y) > 5f) continue;
                var at = new Vector2(Mathf.Clamp(p.x + 1.5f, b.min.x + 0.3f, b.max.x - 0.3f), b.center.y);
                wants.Add((at, piece.world == World.Day));
            }
            void Want(Component c, bool day, float ahead = 6f)
            {
                float dx = c.transform.position.x - p.x;
                if (dx > -1f && dx < ahead && Mathf.Abs(c.transform.position.y - p.y) < 6f) wants.Add((c.transform.position, day));
            }
            // Sombras dormidas, flores y corrientes que solo funcionan de día, caminos de luciérnagas
            // de noche y el agua del lago (peligrosa de día).
            foreach (var m in FindObjectsByType<Monster>()) Want(m, true);
            foreach (var f in FindObjectsByType<Spring>()) Want(f, true, 3f);
            foreach (var a in FindObjectsByType<AirCurrent>()) Want(a, true, 3f);
            foreach (var g in FindObjectsByType<GlowPath>()) Want(g, false, 4f);
            foreach (var h in FindObjectsByType<Hazard>()) if (h.active == Hazard.When.DayOnly) Want(h, false, 3f);
            if (Debug && Time.frameCount % 90 == 0) UnityEngine.Debug.LogWarning($"bot paso p={p} piezas={FindObjectsByType<WorldPiece>().Length} quiere={wants.Count} linea={line.x:0.0} [{line.minX:0},{line.maxX:0}]");
            if (wants.Count == 0) return;
            float start = line.x, best = start, bestCost = float.MaxValue;
            for (float candidate = line.minX; candidate <= line.maxX; candidate += 0.5f)
            {
                line.x = candidate;
                int wrong = 0;
                foreach (var (at, day) in wants) if (ShiftLine.IsDay(at) != day) wrong++;
                // Nunca dejar la línea encima de Lumi (ahí cambia de mundo al pisar).
                float cost = wrong * 100f + Mathf.Abs(candidate - start) * 0.1f + (Mathf.Abs(candidate - p.x) < 0.6f ? 5f : 0f);
                if (cost < bestCost) { bestCost = cost; best = candidate; }
            }
            line.x = Mathf.MoveTowards(start, best, Time.deltaTime * 30f);
            if (Debug && Time.frameCount % 60 == 0)
                UnityEngine.Debug.LogWarning($"bot p={p} wants={wants.Count} [{string.Join(",", wants.ConvertAll(w => (w.day ? "D" : "N") + w.at.x.ToString("0")))}] linea {start:0.0}->{best:0.0} coste {bestCost} min {line.minX} max {line.maxX}");
        }
    }
}

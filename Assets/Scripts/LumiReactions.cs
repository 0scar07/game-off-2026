using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Pequeñas reacciones de Lumi y efectos de movimiento:
    /// tiembla de frío quieta en la nieve de noche, se sacude el agua al pararse tras la lluvia,
    /// se ríe si cae al lago de día, deja un rastro de luz corriendo a toda velocidad, un anillo
    /// de luz al saltar y, si aterriza en una pieza que acaba de aparecer, cámara lenta y bandas
    /// de cine: "¡justo a tiempo!".
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class LumiReactions : MonoBehaviour
    {
        PlayerController player;
        Rigidbody2D body;
        int chapter;
        TrailRenderer trail;
        ParticleSystem drops;
        float fastTime, stillTime, wetness, shiverPhase;
        Vector3 visualBase;
        bool visualKnown;

        void Start()
        {
            player = GetComponent<PlayerController>();
            body = GetComponent<Rigidbody2D>();
            var hud = Hud.Instance;
            var entry = hud != null ? LevelCatalog.Get.At(hud.levelNumber) : null;
            chapter = entry != null ? entry.chapter : 1;
            var shader = Shader.Find("Fase/Particle");
            if (shader != null)
            {
                trail = BuildTrail(shader);
                drops = BuildDrops(shader);
                ringMaterial = new Material(shader);
                ringMaterial.SetFloat("_World", 2f);
                ringMaterial.SetFloat("_Additive", 1f);
                ringMaterial.renderQueue = 3100;
            }
            GameEvents.Jumped += OnJumped;
            GameEvents.Landed += OnLanded;
            GameEvents.PieceAppeared += OnPieceAppeared;
            GameEvents.Died += OnDied;
        }

        void OnDestroy()
        {
            GameEvents.Jumped -= OnJumped;
            GameEvents.Landed -= OnLanded;
            GameEvents.PieceAppeared -= OnPieceAppeared;
            GameEvents.Died -= OnDied;
        }

        // ───────── Rastro de luz ─────────

        TrailRenderer BuildTrail(Shader shader)
        {
            var go = new GameObject("Rastro");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            var t = go.AddComponent<TrailRenderer>();
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", 2f);
            mat.SetFloat("_Additive", 1f);
            mat.renderQueue = 3050;
            t.sharedMaterial = mat;
            t.time = 0.25f;
            t.widthCurve = new AnimationCurve(new Keyframe(0f, 0.9f), new Keyframe(1f, 0f));
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.9f, 0.65f), 0f), new GradientColorKey(new Color(1f, 0.7f, 0.4f), 1f) },
                new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0f, 1f) });
            t.colorGradient = gradient;
            t.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            t.emitting = false;
            return t;
        }

        // ───────── Gotas al sacudirse ─────────

        ParticleSystem BuildDrops(Shader shader)
        {
            var go = new GameObject("Gotas");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startLifetime = 0.6f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
            main.startColor = new Color(0.6f, 0.85f, 1f, 0.9f);
            main.gravityModifier = 0.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", 2f);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
            ps.Play();
            return ps;
        }

        // ───────── Anillo al saltar ─────────

        Material ringMaterial;
        readonly List<(LineRenderer line, float born)> rings = new List<(LineRenderer, float)>();

        void OnJumped()
        {
            if (ringMaterial == null || Settings.ReduceEffects) return;
            var go = new GameObject("Anillo");
            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = ringMaterial;
            lr.loop = true;
            lr.positionCount = 24;
            lr.useWorldSpace = false;
            lr.widthMultiplier = 0.06f;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.position = transform.position + Vector3.down * 0.88f;
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a) * 0.2f, Mathf.Sin(a) * 0.2f));
            }
            rings.Add((lr, Time.time));
        }

        // ───────── ¡Justo a tiempo! ─────────

        float lastAppear = -9f, cooldown;
        Vector3 lastAppearAt;
        RectTransform bars;

        void OnPieceAppeared(Vector3 at, World world) { lastAppear = Time.unscaledTime; lastAppearAt = at; }

        void OnLanded(float speed)
        {
            // Aterriza en una pieza que acaba de aparecer bajo sus pies (la línea llegó en el último momento).
            if (cooldown > 0f || Settings.ReduceEffects || Time.unscaledTime - lastAppear > 0.35f) return;
            if (Mathf.Abs(lastAppearAt.x - transform.position.x) > 2.5f || Mathf.Abs(lastAppearAt.y - (transform.position.y - 1f)) > 1.5f) return;
            cooldown = 10f;
            TimeControl.SlowMotion(0.45f, 0.35f);
            ShowBars();
            LumiBubble.Say(transform, Loc.Pick("¡Justo a tiempo!", "Just in time!"), 1.2f);
        }

        void ShowBars()
        {
            if (bars == null)
            {
                var canvas = UiKit.Canvas("BandasCine", 24);
                canvas.transform.SetParent(transform.parent, false);
                bars = (RectTransform)canvas.transform;
                foreach (var top in new[] { true, false })
                {
                    var bar = UiKit.Box(bars, top ? "Arriba" : "Abajo", new Vector2(0.5f, top ? 1f : 0f), Vector2.zero, new Vector2(4000f, 90f), Color.black, false);
                    bar.rectTransform.pivot = new Vector2(0.5f, top ? 1f : 0f);
                }
            }
            barsLeft = 0.9f;
        }

        float barsLeft;

        // ───────── Agua ─────────

        void OnDied()
        {
            if (Time.time - Hazard.LastWaterDeath < 0.2f && Random.value < 0.6f)
                Invoke(nameof(Laugh), 0.4f);
        }

        void Laugh()
        {
            var lines = new[] { ("¡Jaja, qué fría!", "Haha, so cold!"), ("¡Splash! Otra vez...", "Splash! Again..."), ("¡Brrr! Mejor de noche.", "Brrr! Better at night.") };
            var line = lines[Random.Range(0, lines.Length)];
            LumiBubble.Say(transform, Loc.Pick(line.Item1, line.Item2), 1.8f);
        }

        // ───────── Cada frame ─────────

        void Update()
        {
            cooldown -= Time.unscaledDeltaTime;
            if (player == null || body == null) return;
            var v = body.linearVelocity;
            bool day = ShiftLine.IsDay(transform.position);

            // Rastro: tras medio segundo a toda velocidad.
            fastTime = Mathf.Abs(v.x) > player.speed * 0.9f ? fastTime + Time.deltaTime : 0f;
            if (trail != null) trail.emitting = fastTime > 0.5f && !Settings.ReduceEffects;

            // Mojada si llueve sobre ella de día; al pararse, se sacude.
            if (day && LevelLife.RainLevel > 0.3f) wetness = Mathf.Min(1f, wetness + Time.deltaTime * 0.2f);
            bool still = Mathf.Abs(v.x) < 0.3f && Mathf.Abs(v.y) < 0.3f;
            stillTime = still ? stillTime + Time.deltaTime : 0f;
            if (stillTime > 0.5f && wetness > 0.6f && (LevelLife.RainLevel < 0.2f || !day))
            {
                wetness = 0f;
                if (drops != null) { drops.transform.position = transform.position; drops.Emit(18); }
                shake = 0.5f;
            }

            // Temblor: quieta en la nieve de noche (capítulo III) o mientras se sacude.
            shake -= Time.deltaTime;
            bool cold = chapter == 3 && !day && stillTime > 1.5f && !player.Sleeping;
            var visual = player.visual;
            if (visual != null)
            {
                if (!visualKnown) { visualBase = visual.localPosition; visualKnown = true; }
                float amount = shake > 0f ? 0.06f : cold ? 0.025f : 0f;
                shiverPhase += Time.deltaTime * (shake > 0f ? 60f : 38f);
                visual.localPosition = visualBase + new Vector3(Mathf.Sin(shiverPhase) * amount, 0f, 0f);
            }

            // Anillos: crecen y se apagan.
            for (int i = rings.Count - 1; i >= 0; i--)
            {
                var (line, born) = rings[i];
                float k = (Time.time - born) / 0.45f;
                if (line == null || k >= 1f) { if (line != null) Destroy(line.gameObject); rings.RemoveAt(i); continue; }
                line.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.1f, k);
                var c = new Color(1f, 0.92f, 0.7f, 1f - k);
                line.startColor = line.endColor = c;
            }

            // Bandas de cine.
            if (bars != null)
            {
                barsLeft -= Time.unscaledDeltaTime;
                bars.gameObject.SetActive(barsLeft > 0f);
            }
        }

        float shake;
    }
}

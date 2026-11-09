using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Detalles vivos del nivel: el farol brilla más cerca de un fragmento, la tarjeta de capítulo
    /// al empezar uno nuevo, la luciérnaga guía que da pistas si te atascas y el clima que va y
    /// viene (lluvia fina en el lado de día, nieve en el de noche: el shader recorta cada una).
    /// </summary>
    public class LevelLife : MonoBehaviour
    {
        PlayerController player;
        GlowSprite lantern;
        float lanternSize;
        Collectible[] fragments;
        float bestX, stuck, hintCooldown;
        int deathsHere;
        Transform guide;
        Text guideText;
        ParticleSystem rain, snow;
        float weatherClock, sleep;


        IEnumerator Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            if (player == null) yield break;
            bestX = player.transform.position.x;
            foreach (var g in player.GetComponentsInChildren<GlowSprite>()) if (g.name == "LuzFarol") lantern = g;
            if (lantern != null) lanternSize = lantern.size;
            fragments = FindObjectsByType<Collectible>();
            BuildWeather();
            // El mundo reacciona a la línea, Lumi mira lo importante y se marcan las piezas al alcance.
            gameObject.AddComponent<LineReactor>();
            gameObject.AddComponent<PieceHighlight>();
            if (player.GetComponent<LumiGaze>() == null) player.gameObject.AddComponent<LumiGaze>();
            // Se duerme si no la mueves, ánimo tras muchas caídas, mantener R, estrellas fugaces e iconos de las piezas.
            if (player.GetComponent<LumiSleep>() == null) player.gameObject.AddComponent<LumiSleep>();
            gameObject.AddComponent<Encouragement>();
            gameObject.AddComponent<HoldToReturn>();
            gameObject.AddComponent<ShootingStars>();
            gameObject.AddComponent<PieceIcons>();

            var hud = Hud.Instance;
            var here = hud != null ? LevelCatalog.Get.At(hud.levelNumber) : null;
            chapter = here != null ? here.chapter : 1;
            wasDay = ShiftLine.IsDay(player.transform.position);

            GameEvents.Died += OnDied;
            GameEvents.CheckpointReached += OnCheckpoint;
            GameEvents.Step += OnStep;
            yield return ChapterCard();
        }

        void OnDestroy()
        {
            GameEvents.Died -= OnDied;
            GameEvents.CheckpointReached -= OnCheckpoint;
            GameEvents.Step -= OnStep;
        }

        // ───────── Farol y estornudos ─────────

        int chapter, steps;
        bool wasDay;
        float sneezeWait = 8f;

        /// <summary>El farol tintinea un paso sí y otro no.</summary>
        void OnStep(bool night)
        {
            if (++steps % 2 == 0) Sfx.Play("farol", 0.18f, Random.Range(0.94f, 1.1f));
        }

        /// <summary>En la Cumbre Helada, al pasar del día a la nieve de noche, Lumi a veces estornuda.</summary>
        void CheckSneeze()
        {
            sneezeWait -= Time.deltaTime;
            bool day = ShiftLine.IsDay(player.transform.position);
            if (chapter == 3 && wasDay && !day && sneezeWait <= 0f && Random.value < 0.45f)
            {
                sneezeWait = 45f;
                Sfx.Play("estornudo", 0.7f, Random.Range(0.95f, 1.08f));
                LumiBubble.Say(player.transform, Loc.Pick("¡Achís!", "Achoo!"), 1.6f);
                player.Squash(new Vector2(1.25f, 0.78f));
            }
            wasDay = day;
        }


        void OnDied() => deathsHere++;
        void OnCheckpoint() { deathsHere = 0; stuck = 0f; }

        // ───────── Tarjeta de capítulo ─────────

        IEnumerator ChapterCard()
        {
            var hud = Hud.Instance;
            if (hud == null || hud.Daily) yield break;
            var catalog = LevelCatalog.Get;
            var entry = catalog.At(hud.levelNumber);
            var previous = catalog.At(hud.levelNumber - 1);
            if (entry == null || entry.secret || (previous != null && previous.chapter == entry.chapter)) yield break;
            string[] names = { Loc.Pick("La Pradera", "The Meadow"), Loc.Pick("El Bosque Otoñal", "The Autumn Forest"), Loc.Pick("La Cumbre Helada", "The Frozen Summit") };
            string[] numbers = { "I", "II", "III" };
            int c = Mathf.Clamp(entry.chapter - 1, 0, 2);
            var canvas = UiKit.Canvas("Capitulo", 45);
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            canvas.transform.SetParent(transform, false);
            group.blocksRaycasts = false;
            // Una cabecera sobre el título del nivel deja libres la pista y el salto inicial.
            var strip = UiKit.Box(canvas.transform, "Cabecera", new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1200f, 90f), UiKit.Panel);
            var title = UiKit.Text(strip.transform, "Nombre", $"{Loc.Get("chapter")} {numbers[c]} · {names[c]}", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 80f), 42, TextAnchor.MiddleCenter, UiKit.Gold);
            for (float t = 0f; t < 3f; t += Time.unscaledDeltaTime)
            {
                group.alpha = t < 0.4f ? t / 0.4f : t > 2.4f ? 1f - (t - 2.4f) / 0.6f : 1f;
                if (PauseMenu.Paused || PhotoMode.Active) group.alpha = 0f;
                yield return null;
            }
            Destroy(canvas.gameObject);
        }

        // ───────── Clima ─────────

        void BuildWeather()
        {
            var cam = Camera.main;
            var shader = Shader.Find("Fase/Particle");
            if (cam == null || shader == null) return;
            rain = Weather(cam.transform, shader, 0f, new Color(0.75f, 0.85f, 1f, 0.45f), new Vector3(0.02f, 0.5f, 1f), 14f);
            snow = Weather(cam.transform, shader, 1f, new Color(1f, 1f, 1f, 0.85f), new Vector3(0.12f, 0.12f, 1f), 1.6f);
        }

        static ParticleSystem Weather(Transform parent, Shader shader, float world, Color color, Vector3 size, float speed)
        {
            var go = new GameObject(world < 0.5f ? "LluviaDia" : "NieveNoche");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 9f, 12f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startSpeed = speed;
            main.startLifetime = 3f;
            main.startSize3D = true;
            main.startSizeX = size.x; main.startSizeY = size.y; main.startSizeZ = 1f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(44f, 0.5f, 8f);
            shape.rotation = new Vector3(90f, 0f, 0f);
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", world);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            r.sharedMaterial = mat;
            if (world < 0.5f) { r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = 0.06f; }
            ps.Play();
            return ps;
        }

        void Update()
        {
            if (player == null) return;
            CheckSneeze();


            // El clima sube y baja en ciclos de unos 40 s.
            weatherClock += Time.deltaTime;
            float intensity = Mathf.Clamp01(Mathf.Sin(weatherClock * 0.15f) * 1.4f - 0.2f);
            if (rain != null) { var e = rain.emission; e.rateOverTime = intensity * 220f; }
            if (snow != null) { var e = snow.emission; e.rateOverTime = intensity * 90f; }

            // Farol: brilla y late más cerca de un fragmento sin coger.
            if (lantern != null && fragments != null)
            {
                float nearest = 99f;
                foreach (var f in fragments) if (f != null) nearest = Mathf.Min(nearest, Vector2.Distance(f.transform.position, player.transform.position));
                float k = 1f - Mathf.Clamp01((nearest - 2f) / 8f);
                // Dormida, el farol casi se apaga.
                sleep = Mathf.MoveTowards(sleep, player.Sleeping ? 1f : 0f, Time.deltaTime * (player.Sleeping ? 0.5f : 3f));
                lantern.size = lanternSize * (1f + k * 0.6f) * Mathf.Lerp(1f, 0.2f, sleep);
                lantern.pulse = 0.08f + k * 0.2f;
                lantern.pulseSpeed = 3f + k * 6f;
            }

            // Luciérnaga guía: si llevas rato sin avanzar o te caes mucho en el mismo sitio.
            float x = player.transform.position.x;
            if (x > bestX + 2f) { bestX = x; stuck = 0f; }
            else stuck += Time.deltaTime;
            hintCooldown -= Time.deltaTime;
            if (hintCooldown <= 0f && (stuck > 25f || deathsHere >= 4)) { ShowHint(); hintCooldown = 30f; stuck = 0f; deathsHere = 0; }
            if (guide != null)
            {
                var target = player.transform.position + new Vector3(1.2f, 2.4f + Mathf.Sin(Time.time * 3f) * 0.2f, -0.5f);
                guide.position = Vector3.Lerp(guide.position, target, Time.deltaTime * 3f);
            }
        }

        void ShowHint()
        {
            if (guide == null)
            {
                guide = new GameObject("Guia").transform;
                guide.position = player.transform.position + new Vector3(6f, 5f, -0.5f);
                if (lantern != null)
                {
                    var glow = Instantiate(lantern.gameObject, guide);
                    glow.transform.localPosition = Vector3.zero;
                    var g = glow.GetComponent<GlowSprite>();
                    if (g != null) { g.size = 1.2f; g.dayFactor = 1f; }
                    foreach (var r in glow.GetComponentsInChildren<Renderer>())
                    {
                        var block = new MaterialPropertyBlock();
                        block.SetColor("_Color", new Color(0.7f, 1.5f, 0.5f, 0.9f));
                        r.SetPropertyBlock(block);
                    }
                }
                var canvas = UiKit.Canvas("PistaGuia", 8);
                canvas.transform.SetParent(transform, false);
                var box = UiKit.Box(canvas.transform, "Bocadillo", new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(1000f, 80f), new Color(0.05f, 0.15f, 0.05f, 0.8f));
                guideText = UiKit.Text(box.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 80f), 32, TextAnchor.MiddleCenter, new Color(0.85f, 1f, 0.8f));
            }
            guideText.transform.parent.gameObject.SetActive(true);
            guideText.text = "✦ " + HintFor(player.transform.position);
            CancelInvoke(nameof(HideHint));
            Invoke(nameof(HideHint), 6f);
        }

        void HideHint() { if (guideText != null) guideText.transform.parent.gameObject.SetActive(false); }

        /// <summary>Pista según lo que hay justo delante.</summary>
        static string HintFor(Vector3 at)
        {
            WorldPiece nearest = null;
            float best = 12f;
            foreach (var piece in FindObjectsByType<WorldPiece>())
            {
                float dx = piece.transform.position.x - at.x;
                if (dx > -1f && dx < best) { best = dx; nearest = piece; }
            }
            foreach (var m in FindObjectsByType<Monster>())
                if (m.transform.position.x - at.x > -1f && m.transform.position.x - at.x < 8f)
                    return Loc.Pick("Las Sombras duermen de día: lleva la línea hasta ellas.", "Shadows sleep by day: bring the line over them.");
            if (nearest == null) return Loc.Pick("Prueba a mover la línea a la otra punta.", "Try moving the line all the way across.");
            return nearest.world == World.Night
                ? Loc.Pick("Lo azul solo existe de noche: arrastra la línea detrás de ti.", "Blue things only exist at night: drag the line behind you.")
                : Loc.Pick("La madera solo existe de día: lleva la línea por delante.", "Wood only exists by day: move the line ahead of you.");
        }
    }
}

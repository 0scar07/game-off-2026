using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Modo foto (tecla F o el botón de la cámara): congela el juego, quita la interfaz y deja
    /// mover la cámara (flechas/WASD), acercarla (rueda o +/-), inclinarla (Q/E), cambiar el
    /// filtro (1-4) y ocultar a Lumi (H). Espacio o el botón captura la imagen; F o Esc sale.
    /// </summary>
    public class PhotoMode : MonoBehaviour
    {
        public static bool Active { get; private set; }

        static readonly (string es, string en, Color filter, float saturation)[] Filters =
        {
            ("Natural", "Natural", Color.white, 0f),
            ("Atardecer", "Sunset", new Color(1f, 0.85f, 0.7f), 10f),
            ("Luna", "Moon", new Color(0.75f, 0.85f, 1f), -25f),
            ("Recuerdo", "Memory", new Color(1f, 0.93f, 0.8f), -70f),
        };

        Camera cam;
        Vector3 startPos;
        Quaternion startRot;
        float startFov, tilt;
        int filter;
        Canvas ui;
        Text filterLabel;
        Canvas[] hidden;
        ColorAdjustments adjust;
        Color baseFilter;
        float baseSaturation;
        GameObject lumi;
        bool lumiHidden;

        static PhotoMode instance;
        void Awake() => instance = this;

        // Si la escena cambia con el modo foto abierto, que no se quede marcado para la siguiente.
        void OnDestroy()
        {
            if (instance == this) instance = null;
            Active = false;
        }

        /// <summary>Desde el botón de la pausa (sirve también en el móvil).</summary>
        public static void Open()
        {
            if (instance == null || Active) return;
            FindAnyObjectByType<PauseMenu>()?.Resume();
            instance.Enter();
        }

        void Update()
        {
            var pad = Gamepad.current;
            if (Active && pad != null && pad.buttonEast.wasPressedThisFrame)
            {
                Exit();
                return;
            }
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!Active && kb.fKey.wasPressedThisFrame && !PauseMenu.Paused && !UiKit.Typing && FindAnyObjectByType<ResultsScreen>() == null) Enter();
            else if (Active && (kb.fKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)) Exit();
            if (!Active) return;

            float dt = Time.unscaledDeltaTime;
            Vector3 move = Vector3.zero;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1f;
            cam.transform.position += move * 6f * dt;
            float zoom = (kb.equalsKey.isPressed || kb.numpadPlusKey.isPressed ? -1f : 0f) + (kb.minusKey.isPressed || kb.numpadMinusKey.isPressed ? 1f : 0f);
            if (Mouse.current != null) zoom -= Mouse.current.scroll.ReadValue().y * 0.05f;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + zoom * 20f * dt, 15f, 60f);
            if (kb.qKey.isPressed) tilt += 20f * dt;
            if (kb.eKey.isPressed) tilt -= 20f * dt;
            cam.transform.rotation = startRot * Quaternion.Euler(0f, 0f, tilt);
            for (int i = 0; i < Filters.Length; i++) if (kb[Key.Digit1 + i].wasPressedThisFrame) SetFilter(i);
            if (kb.hKey.wasPressedThisFrame) ToggleLumi();
            if (kb.spaceKey.wasPressedThisFrame) StartCoroutine(Capture());
        }

        void Enter()
        {
            Active = true;
            Time.timeScale = 0f;
            cam = Camera.main;
            startPos = cam.transform.position;
            startRot = cam.transform.rotation;
            startFov = cam.fieldOfView;
            tilt = 0f;
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
            hidden = FindObjectsByType<Canvas>();
            foreach (var c in hidden) c.enabled = false;
            var volume = FindAnyObjectByType<Volume>();
            if (volume != null && volume.profile.TryGet(out adjust))
            {
                adjust.colorFilter.overrideState = adjust.saturation.overrideState = true;
                baseFilter = adjust.colorFilter.value;
                baseSaturation = adjust.saturation.value;
            }
            var player = FindAnyObjectByType<PlayerController>();
            lumi = player != null && player.visual != null ? player.visual.gameObject : null;
            BuildUi();
        }

        void Exit()
        {
            if (!Active) return;
            Active = false;
            SetFilter(0);
            if (lumiHidden) ToggleLumi();
            cam.transform.SetPositionAndRotation(startPos, startRot);
            cam.fieldOfView = startFov;
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = true;
            foreach (var c in hidden) if (c != null) c.enabled = true;
            if (ui != null) Destroy(ui.gameObject);
            TimeControl.Normal();
        }

        void BuildUi()
        {
            UiKit.EnsureEvents();
            ui = UiKit.Canvas("ModoFoto", 60);
            var box = UiKit.Box(ui.transform, "Panel", new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(560f, 330f), new Color(0.04f, 0.03f, 0.14f, 0.75f));
            UiKit.Text(box.transform, "Titulo", Loc.Pick("Modo foto", "Photo mode"), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(500f, 50f), 38, TextAnchor.MiddleCenter, UiKit.Gold);
            UiKit.Text(box.transform, "Ayuda", Loc.Pick("Mover: WASD · Zoom: rueda o +/-\nInclinar: Q/E · Filtros: 1-4\nOcultar a Lumi: H · Salir: F",
                "Move: WASD · Zoom: wheel or +/-\nTilt: Q/E · Filters: 1-4\nHide Lumi: H · Exit: F"), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(520f, 140f), 24, TextAnchor.MiddleCenter, UiKit.Soft);
            filterLabel = UiKit.Text(box.transform, "Filtro", "", new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(500f, 40f), 28);
            UiKit.Button(ui.transform, "Salir", Loc.Pick("Volver al juego", "Back to game"), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(440f, 100f), Exit, 36);
            UiKit.Button(ui.transform, "Capturar", Loc.Pick("Capturar (Espacio)", "Capture (Space)"), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(460f, 100f), () => StartCoroutine(Capture()), 40);
            SetFilter(0);
        }

        void SetFilter(int index)
        {
            filter = index;
            if (filterLabel != null) filterLabel.text = Loc.Pick("Filtro: ", "Filter: ") + Loc.Pick(Filters[index].es, Filters[index].en);
            if (adjust == null) return;
            adjust.colorFilter.value = index == 0 ? baseFilter : baseFilter * Filters[index].filter;
            adjust.saturation.value = baseSaturation + Filters[index].saturation;
        }

        void ToggleLumi()
        {
            lumiHidden = !lumiHidden;
            if (lumi != null) lumi.SetActive(!lumiHidden);
        }

        System.Collections.IEnumerator Capture()
        {
            ui.enabled = false;
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            ui.enabled = true;
            Share.SavePicture(shot);
            Achievements.Unlock("fotografo");
            Destroy(shot);
            if (filterLabel != null) filterLabel.text = Loc.Pick("¡Foto guardada!", "Photo saved!");
        }
    }
}

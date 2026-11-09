using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Galería de Extras: los modelos del juego (Lumi, una Sombra, la abeja, el fragmento...) en un
    /// rincón lejano del menú, vistos por su propia cámara y pintados en el panel. Giran solos
    /// despacio y se pueden girar arrastrando.
    /// </summary>
    public class Gallery : MonoBehaviour
    {
        static readonly Vector3 Spot = new Vector3(3000f, 0f, 0f);
        static readonly (string piece, string es, string en)[] Entries =
        {
            ("", "Lumi", "Lumi"),
            ("sombra", "Sombra", "Shadow"),
            ("abeja", "Abeja de la noche", "Night bee"),
            ("fragmento", "Fragmento de luna", "Moon fragment"),
            ("cofre", "Cofre secreto", "Secret chest"),
            ("relojsol", "Reloj de sol", "Sundial"),
            ("muelle", "Flor muelle", "Spring flower"),
        };

        RawImage view;
        MenuWalker walker;
        Camera cam;
        RenderTexture target;
        Transform turntable, current;
        int index;
        float yaw;
        bool dragging, visible;
        readonly Dictionary<int, Transform> built = new Dictionary<int, Transform>();

        public string CurrentName => Loc.Pick(Entries[index].es, Entries[index].en);

        public void Init(RawImage image, MenuWalker lumi)
        {
            view = image;
            walker = lumi;
            target = new RenderTexture(640, 640, 16);
            view.texture = target;
            turntable = new GameObject("Galeria").transform;
            turntable.position = Spot;
            cam = new GameObject("CamaraGaleria").AddComponent<Camera>();
            cam.transform.position = Spot + new Vector3(0f, 0.1f, -6.5f);
            cam.fieldOfView = 26f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.18f);
            cam.targetTexture = target;
            cam.enabled = false;
            var light = new GameObject("LuzGaleria").AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 20f;
            light.intensity = 1.5f;
            light.transform.position = Spot + new Vector3(-2f, 3f, -4f);
            Show(0);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            if (cam != null) cam.enabled = value;
        }

        public void Step(int direction) => Show(((index + direction) % Entries.Length + Entries.Length) % Entries.Length);

        void Show(int i)
        {
            index = i;
            if (current != null) current.gameObject.SetActive(false);
            if (!built.TryGetValue(i, out current) || current == null)
            {
                current = Build(Entries[i].piece);
                built[i] = current;
            }
            if (current != null) current.gameObject.SetActive(true);
            yaw = 200f;
        }

        /// <summary>Copia el modelo sin sus scripts ni colisiones, centrado y a 2 unidades de alto.</summary>
        Transform Build(string piece)
        {
            GameObject source = string.IsNullOrEmpty(piece) ? (walker != null ? walker.gameObject : null) : Resources.Load<GameObject>("Piezas/" + piece);
            if (source == null) return null;
            // Bajo un padre apagado: así no se ejecuta el Awake de sus scripts antes de quitarlos.
            var holder = new GameObject("Modelo").transform;
            holder.gameObject.SetActive(false);
            holder.SetParent(turntable, false);
            var copy = Instantiate(source, holder);
            foreach (var mb in copy.GetComponentsInChildren<MonoBehaviour>(true))
                if (!(mb is LumiStyle) && !(mb is Scarf)) DestroyImmediate(mb);
            foreach (var c in copy.GetComponentsInChildren<Collider2D>(true)) DestroyImmediate(c);
            foreach (var c in copy.GetComponentsInChildren<Collider>(true)) DestroyImmediate(c);
            foreach (var r in copy.GetComponentsInChildren<Rigidbody2D>(true)) DestroyImmediate(r);
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            holder.gameObject.SetActive(true);
            var renderers = copy.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                float scale = 2f / Mathf.Max(0.01f, Mathf.Max(b.size.y, b.size.x * 0.8f));
                copy.transform.localScale *= scale;
                copy.transform.position += Spot - (Spot + (b.center - Spot) * scale);
            }
            var animator = copy.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null) animator.Play("Idle", 0, 0f);
            return holder;
        }

        void Update()
        {
            if (!visible || current == null) return;
            var pointer = Pointer.current;
            if (pointer != null)
            {
                bool over = RectTransformUtility.RectangleContainsScreenPoint(view.rectTransform, pointer.position.ReadValue(), null);
                if (pointer.press.wasPressedThisFrame && over) dragging = true;
                if (!pointer.press.isPressed) dragging = false;
                if (dragging) yaw -= pointer.delta.ReadValue().x * 0.5f;
            }
            if (!dragging) yaw += Time.unscaledDeltaTime * 25f;
            current.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void OnDestroy()
        {
            if (target != null) target.Release();
            if (turntable != null) Destroy(turntable.gameObject);
            if (cam != null) Destroy(cam.gameObject);
        }
    }
}

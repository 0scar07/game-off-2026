using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Viento de día (cabecera "viento: velocidad"; negativo sopla hacia la izquierda): empuja a
    /// Lumi solo en el lado de día. De noche hay calma. Se ven rachas en el lado de día.
    /// </summary>
    public class Wind : MonoBehaviour
    {
        public float strength = -3f;
        static Wind instance;

        /// <summary>Velocidad horizontal que suma el viento en ese punto (0 de noche o sin viento).</summary>
        public static float Velocity(Vector2 at) => instance != null && ShiftLine.IsDay(at) ? instance.strength : 0f;

        void Awake() => instance = this;
        void OnDestroy() { if (instance == this) instance = null; }

        void Start()
        {
            var cam = Camera.main;
            var shader = Shader.Find("Fase/Particle");
            if (cam == null || shader == null) return;
            var go = new GameObject("Rachas");
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(strength > 0f ? -14f : 14f, 0f, 14f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startLifetime = 2.5f;
            main.startSpeed = Mathf.Abs(strength) * 4f;
            main.startSize3D = true;
            main.startSizeX = 1.2f; main.startSizeY = 0.04f; main.startSizeZ = 1f;
            main.startColor = new Color(1f, 1f, 1f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 14f, 4f);
            shape.rotation = new Vector3(0f, strength > 0f ? 90f : -90f, 0f);
            var emission = ps.emission;
            emission.rateOverTime = 14f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", 0f);
            mat.SetFloat("_Additive", 1f);
            mat.renderQueue = 3000;
            r.sharedMaterial = mat;
            ps.Play();
        }
    }
}

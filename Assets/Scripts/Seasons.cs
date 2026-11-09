using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Estaciones (cabecera "estaciones: si"): en el lado de día caen hojas de otoño; en el de
    /// noche ya nieva (el clima normal). La línea separa también el otoño del invierno.
    /// </summary>
    public class Seasons : MonoBehaviour
    {
        void Start()
        {
            var cam = Camera.main;
            var shader = Shader.Find("Fase/Particle");
            if (cam == null || shader == null) return;
            var go = new GameObject("HojasOtono");
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(0f, 9f, 12f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startLifetime = 6f;
            main.startSpeed = 1.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.2f), new Color(0.95f, 0.8f, 0.3f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 0.5f, 6f);
            shape.rotation = new Vector3(90f, 0f, 0f);
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.6f;
            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
            var emission = ps.emission;
            emission.rateOverTime = 18f;
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", 0f);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
            ps.Play();
        }
    }
}

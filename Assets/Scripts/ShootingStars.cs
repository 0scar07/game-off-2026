using UnityEngine;

namespace Fase
{
    /// <summary>
    /// De vez en cuando cruza una estrella fugaz por el cielo. Solo se ve en el lado de noche (el
    /// shader de partículas la recorta con la línea).
    /// </summary>
    public class ShootingStars : MonoBehaviour
    {
        ParticleSystem stars;
        Camera cam;
        float next = 4f;

        void Start()
        {
            cam = Camera.main;
            var shader = Shader.Find("Fase/Particle");
            if (cam == null || shader == null) return;
            var go = new GameObject("EstrellasFugaces");
            go.transform.SetParent(transform, false);
            stars = go.AddComponent<ParticleSystem>();
            stars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = stars.main;
            main.startLifetime = 0.9f;
            main.startSize = 0.18f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 8;
            main.playOnAwake = false;
            var emission = stars.emission;
            emission.rateOverTime = 0f;
            var fade = stars.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.7f, 0.8f, 1f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.12f;
            r.lengthScale = 2f;
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", 1f);
            mat.SetFloat("_Additive", 1f);
            mat.SetColor("_Color", new Color(2f, 2f, 2.4f, 1f));
            mat.renderQueue = 3000;
            r.sharedMaterial = mat;
            stars.Play();
        }

        void Update()
        {
            if (stars == null || cam == null) return;
            next -= Time.deltaTime;
            if (next > 0f) return;
            next = Random.Range(6f, 14f);
            // En el cielo, por detrás del nivel, en la parte de arriba de la pantalla.
            var at = cam.transform.position + new Vector3(Random.Range(-6f, 12f), Random.Range(5f, 8f), 22f);
            var emit = new ParticleSystem.EmitParams
            {
                position = at,
                velocity = new Vector3(-Random.Range(14f, 20f), -Random.Range(5f, 8f), 0f),
                startLifetime = 0.9f,
            };
            stars.Emit(emit, 1);
        }
    }
}

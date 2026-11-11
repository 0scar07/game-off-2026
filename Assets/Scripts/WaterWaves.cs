using UnityEngine;

namespace Fase
{
    /// <summary>Hace correr las olas del lago (desplaza la textura) sin crear materiales nuevos.</summary>
    public class WaterWaves : MonoBehaviour
    {
        public float tiles = 4f, speed = 0.25f;

        static readonly int TexST = Shader.PropertyToID("_MainTex_ST");
        Renderer target;
        MaterialPropertyBlock block;

        void Start()
        {
            target = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (target == null) return;
            float offset = Time.time * speed;
            block.SetVector(TexST, new Vector4(tiles, 1f, offset, 0f));
            target.SetPropertyBlock(block);
        }
    }
}

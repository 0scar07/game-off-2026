// Partículas, halos y cintas de FASE (luciérnagas, polen, chispas, bufanda, brillo del farol...).
// Sin iluminación, con color de vértice, y recortadas por la línea igual que el mundo:
//   _World = 0 solo de día, 1 solo de noche, 2 siempre.
// _Additive = 1 suma luz (brillos); 0 mezcla normal con transparencia (hojas, nieve, bufanda).
Shader "Fase/Particle"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        [Enum(Dia, 0, Noche, 1, Ambos, 2)] _World ("Mundo", Float) = 2
        [Toggle] _Additive ("Suma luz", Float) = 1
        [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 5
        [HideInInspector] _DstBlend ("_DstBlend", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _World;
                float _Additive;
            CBUFFER_END
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _LineCount;
            float _DayOnLeft;
            float4 _LineP, _LineN, _Bubble;

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                if (_World < 1.5)
                {
                    // Igual que en Fase/World: cada línea cruzada cambia de mundo; la burbuja del farol es noche.
                    float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);
                    float2 q = float2(uv.x * _ScreenParams.x / _ScreenParams.y, uv.y);
                    float crossed = step(0.0, dot(q - _LineP.xy, _LineN.xy)) + (_LineCount > 1.5 ? step(0.0, dot(q - _LineP.zw, _LineN.xy)) : 0.0);
                    bool odd = fmod(crossed, 2.0) > 0.5;
                    bool night = _DayOnLeft > 0.5 ? odd : !odd;
                    if (_Bubble.w > 0.5 && distance(q, _Bubble.xy) < _Bubble.z) night = true;
                    clip((_World < 0.5) != night ? 1 : -1);
                }
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                // En modo suma, el alfa oscurece el color (así se apagan las partículas al desaparecer).
                if (_Additive > 0.5) color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}

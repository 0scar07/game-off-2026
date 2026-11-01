// Material de Fase: iluminación simple de URP + recorte por la línea de cambio.
//   _World = 0 (Día)   solo se dibuja en el lado de día
//   _World = 1 (Noche) solo se dibuja en el lado de noche
//   _World = 2 (Ambos) siempre se dibuja, pero se tiñe de noche en el lado de noche
// La línea llega en las variables globales _ShiftScreenX (posición en pantalla, 0..1),
// _ShiftX (posición en el mundo, para las sombras) y _DayOnLeft (ver ShiftLine.cs).
Shader "Fase/World"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Textura", 2D) = "white" {}
        [HDR] _Emission ("Brillo propio", Color) = (0, 0, 0, 0)
        [Enum(Dia, 0, Noche, 1, Ambos, 2)] _World ("Mundo", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _BaseMap_ST;
            float4 _Emission;
            float _World;
        CBUFFER_END

        float _ShiftX;
        float _ShiftScreenX;
        float _DayOnLeft;

        // true si `position` queda en el lado de noche de `lineX`.
        bool IsNight(float position, float lineX)
        {
            bool left = position < lineX;
            return _DayOnLeft > 0.5 ? !left : left;
        }

        // Descarta el píxel si no pertenece al mundo de este material.
        void ClipWorld(bool night)
        {
            if (_World > 1.5) return;
            clip((_World < 0.5) != night ? 1 : -1);
        }

        // Corte en pantalla: vertical y en el mismo sitio para lo cercano y para el fondo.
        bool IsNightOnScreen(float4 positionCS)
        {
            return IsNight(GetNormalizedScreenSpaceUV(positionCS).x, _ShiftScreenX);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fog : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fog = ComputeFogFactor(position.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                bool night = IsNightOnScreen(input.positionCS);
                ClipWorld(night);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normal = normalize(input.normalWS);
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = saturate(dot(normal, light.direction)) * light.shadowAttenuation;
                half3 ambient = SampleSH(normal);

                half3 color = albedo.rgb * (light.color * (diffuse * 0.8 + 0.2) + ambient * 0.5);

                // Lado de noche: luz de luna fría y más oscura; el brillo propio destaca más.
                if (night) color *= half3(0.32, 0.38, 0.75);
                color += _Emission.rgb;

                // Borde brillante junto a la línea.
                float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);
                half edge = saturate(1.0 - abs(uv.x - _ShiftScreenX) * _ScreenParams.x / 40.0);
                color += edge * half3(1.0, 0.85, 0.6) * 0.35;

                return half4(MixFog(color, input.fog), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS, normalWS, _LightDirection));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                ClipWorld(IsNight(input.positionWS.x, _ShiftX));
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                ClipWorld(IsNightOnScreen(input.positionCS));
                return 0;
            }
            ENDHLSL
        }
    }
}

// Material de Fase: iluminación simple de URP + recorte por la línea de cambio.
//   _World = 0 (Día)   solo se dibuja en el lado de día
//   _World = 1 (Noche) solo se dibuja en el lado de noche
//   _World = 2 (Ambos) siempre se dibuja, pero se tiñe de noche en el lado de noche
// La línea llega en las variables globales _ShiftScreenX (posición en pantalla, 0..1),
// la línea proyectada en pantalla (_LineP, _LineN), su posición en el mundo para las sombras
// (_ShiftX, _ShiftX2), la burbuja de noche del farol (_Bubble) y _HighContrast (ver ShiftLine.cs
// y Settings.cs): con alto contraste las piezas que solo existen de noche llevan rayas.
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

        float _ShiftX, _ShiftX2;
        float _LineCount;
        float _DayOnLeft;
        float _HighContrast;
        float4 _LineP;    // punto de la línea 1 (xy) y de la 2 (zw) en pantalla, x multiplicada por el ancho
        float4 _LineN;    // normal en pantalla (xy) y en el mundo (zw)
        float4 _Bubble;   // burbuja de noche del farol en pantalla: centro (xy), radio (z), activa (w)
        float4 _BubbleW;  // la misma burbuja en el mundo
        float4 _Zone;     // zona de mundo (espejos, línea curva) en pantalla: centro (xy), radio (z), 0 apagada / 1 día / 2 noche (w)
        float4 _ZoneW;    // la misma zona en el mundo

        // Cada línea cruzada cambia de mundo (con dos líneas: día | noche | día).
        bool NightFromCrossings(float crossed)
        {
            bool odd = fmod(crossed, 2.0) > 0.5;
            return _DayOnLeft > 0.5 ? odd : !odd;
        }

        // Descarta el píxel si no pertenece al mundo de este material.
        void ClipWorld(bool night)
        {
            if (_World > 1.5) return;
            clip((_World < 0.5) != night ? 1 : -1);
        }

        float2 ScreenPoint(float4 positionCS)
        {
            float2 uv = GetNormalizedScreenSpaceUV(positionCS);
            return float2(uv.x * _ScreenParams.x / _ScreenParams.y, uv.y);
        }

        // Corte en pantalla: igual para lo cercano y para el fondo.
        bool IsNightOnScreen(float4 positionCS)
        {
            float2 q = ScreenPoint(positionCS);
            if (_Bubble.w > 0.5 && distance(q, _Bubble.xy) < _Bubble.z) return true;
            if (_Zone.w > 0.5 && distance(q, _Zone.xy) < _Zone.z) return _Zone.w > 1.5;
            float crossed = step(0.0, dot(q - _LineP.xy, _LineN.xy)) + (_LineCount > 1.5 ? step(0.0, dot(q - _LineP.zw, _LineN.xy)) : 0.0);
            return NightFromCrossings(crossed);
        }

        // Para las sombras: la línea en el mundo.
        bool IsNightInWorld(float3 positionWS)
        {
            if (_BubbleW.w > 0.5 && distance(positionWS.xy, _BubbleW.xy) < _BubbleW.z) return true;
            if (_ZoneW.w > 0.5 && distance(positionWS.xy, _ZoneW.xy) < _ZoneW.z) return _ZoneW.w > 1.5;
            float d = dot(positionWS.xy, _LineN.zw);
            float crossed = step(_ShiftX, d) + (_LineCount > 1.5 ? step(_ShiftX2, d) : 0.0);
            return NightFromCrossings(crossed);
        }

        // Distancia en píxeles a la línea más cercana (para el borde brillante).
        float LineDistancePixels(float4 positionCS)
        {
            float2 q = ScreenPoint(positionCS);
            float d = abs(dot(q - _LineP.xy, _LineN.xy));
            if (_LineCount > 1.5) d = min(d, abs(dot(q - _LineP.zw, _LineN.xy)));
            if (_Bubble.w > 0.5) d = min(d, abs(distance(q, _Bubble.xy) - _Bubble.z));
            if (_Zone.w > 0.5) d = min(d, abs(distance(q, _Zone.xy) - _Zone.z));
            return d * _ScreenParams.y;
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
                half edge = pow(saturate(1.0 - LineDistancePixels(input.positionCS) / 30.0), 3.0);
                color += edge * (night ? half3(0.45, 0.55, 1.0) : half3(1.0, 0.9, 0.7)) * 0.45;

                // Alto contraste: rayas diagonales en lo que solo existe de noche, para distinguirlo sin color.
                if (_HighContrast > 0.5 && _World > 0.5 && _World < 1.5)
                {
                    float2 pixel = uv * _ScreenParams.xy;
                    half stripe = step(0.5, frac((pixel.x + pixel.y) / 22.0));
                    color = lerp(color, half3(0.85, 0.95, 1.0), stripe * 0.35);
                }

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
                ClipWorld(IsNightInWorld(input.positionWS));
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

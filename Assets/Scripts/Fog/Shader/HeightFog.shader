Shader "Hidden/HeightFogURP"
{
    Properties { _BlueNoiseTex ("Blue Noise", 2D) = "gray" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "HeightFog"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // Source color (from Blitter)
            TEXTURE2D_X(_BlitTexture); SAMPLER(sampler_BlitTexture);

            // Depth comes from DeclareDepthTexture.hlsl
            // TEXTURE2D_X(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

            TEXTURE2D(_BlueNoiseTex); SAMPLER(sampler_BlueNoiseTex);

            // ---- Globals / uniforms ----
            float4 _FogColor;
            float  _Density;          // base density (0.0–0.1 typical)
            float  _Height;           // world-space fog ceiling
            float  _HeightFalloff;    // how fast density changes with height
            float  _NoiseAmp;         // 0–1
            float  _NoiseScale;       // ~0.02–0.08
            float2 _NoiseScroll;      // tiny motion
            float  _NearVeilStart;    // meters
            float  _NearVeilEnd;      // meters
            float  _SanityBoost;      // 0–1 scalar from gameplay
            float  _DitherStrength;   // 0–1

            // Enemy SDF shroud uniforms (set via Shader.SetGlobal*)
            float3 _SDFCenter; 
            float  _SDFRadius; 
            float  _SDFDelta;  // + densify, - thin

            struct VOut { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };
            VOut Vert(uint id : SV_VertexID)
            {
                VOut o; o.uv = GetFullScreenTriangleTexCoord(id);
                o.posCS = GetFullScreenTriangleVertexPosition(id);
                return o;
            }

            // SDF helpers (global scope)
            float sdfSphere(float3 p, float3 c, float r) { return length(p - c) - r; }

            float FogFactor(float3 worldPos, float linEyeDepth, float2 uv)
            {
                // Distance term (exp2 feels foggier)
                float dist = max(linEyeDepth, 0.0);
                float baseF = 1.0 - exp2(-_Density * dist);

                // Height term (denser below _Height)
                float hTerm = saturate((_Height - worldPos.y) * _HeightFalloff);

                // Low-frequency 3D-ish noise via world XZ (fast cheat)
                float2 nUV = worldPos.xz * _NoiseScale + _NoiseScroll * _Time.y;
                float n0 = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, nUV).r;
                float n  = lerp(1.0, n0, _NoiseAmp);

                // Near-veil (claustrophobia)
                float nearV = smoothstep(_NearVeilStart, _NearVeilEnd, linEyeDepth);

                // Combine baseline
                float f = baseF * hTerm * n;
                f = saturate(f + nearV * 0.08);
                f = saturate(lerp(f, f * 1.5, _SanityBoost)); // intensify under stress

                // Enemy shroud: densify inside SDF sphere (soft edge)
                float d = sdfSphere(worldPos, _SDFCenter, _SDFRadius);
                float inSphere = saturate(1.0 - smoothstep(0.0, 1.0, d)); // 1 inside, 0 outside
                f = saturate(f + inSphere * _SDFDelta);

                return f;
            }

            float4 Frag(VOut i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);

                // Depth sampling via URP helper (device depth 0..1)
                float deviceDepth = SampleSceneDepth(i.uv);

                // Reconstruct world position from depth
                float4x4 invVP  = UNITY_MATRIX_I_VP;
                float3   worldPos = ComputeWorldSpacePosition(i.uv, deviceDepth, invVP);

                // Camera + linear eye depth
                float3 camPos = GetCameraPositionWS();
                float  linEye = distance(worldPos, camPos);

                // Blue-noise dither to fight banding
                float2 bnUV = frac(i.uv * _ScreenParams.xy / 64); // tile
                float  dither = (SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, bnUV).r - 0.5) * _DitherStrength;

                float fog = saturate(FogFactor(worldPos, linEye, i.uv) + dither);
                col.rgb = lerp(col.rgb, _FogColor.rgb, fog);
                return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
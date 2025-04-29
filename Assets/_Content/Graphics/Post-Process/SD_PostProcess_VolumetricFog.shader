Shader "Hidden/VolumetricFog"
{
    Properties
    {
        _MaxDistance("Max sample distance", float) = 100
        _StepSize("Sample step size", Range(0.1, 20)) = 1
        _StepOffset("Random sample offset", float) = 0
        _BaseDensity("Fog base density", float) = 0
        _CloudDensity("Fog cloud density", Range(0, 10)) = 1
        _FogColor("Fog color", Color) = (1,1,1,1)
        _FogNoise("Fog cloud noise", 3D) = "white" {}
        _FogNoiseTiling("Noise tiling", float) = 1
        _FogNoiseOffset("Noise offset", Vector) = (0,0,0,0)
        _FogNoiseThreshold("Noise threshold", float) = 0
        _FogDirection("Noise direction", Vector) = (1,0,0,0)
        _FogSpeed("Noise speed", float) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "POSTPROCESS_VOLUMETRIC_FOG"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            float _MaxDistance;
            float _StepSize;
            float _BaseDensity;
            float _CloudDensity;
            float4 _FogColor;
            float _StepOffset;
            TEXTURE3D(_FogNoise);
            float _FogNoiseTiling;
            float3 _FogNoiseOffset;
            float _FogNoiseThreshold;
            float3 _FogDirection;
            float _FogSpeed;

            float getDensity(float3 samplePos, float invertNoiseThreshold)
            {
    float noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, samplePos * float3(_FogNoiseTiling*0.6, _FogNoiseTiling, _FogNoiseTiling*0.6) + saturate(_FogDirection) * _FogSpeed * _Time.y, 0).x;
                float density = _BaseDensity + _CloudDensity * saturate(noise - _FogNoiseThreshold) * invertNoiseThreshold;
                return density;
            }
            
            half4 frag(Varyings IN) : SV_TARGET
            {
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength, _MaxDistance);
                float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _StepOffset;
                float transmittance = 1;

                float invertNoiseThreshold = rcp(1-_FogNoiseThreshold);
                
                while(distTravelled < distLimit)
                {
                    float3 samplePos = _WorldSpaceCameraPos + rayDir * distTravelled;
                    float density = getDensity(samplePos, invertNoiseThreshold);

                    Light mainLight = GetMainLight(TransformWorldToShadowCoord(samplePos));
                    transmittance *= exp(-density * _StepSize);
                    _FogColor.rgb += mainLight.color.rgb * density * mainLight.shadowAttenuation * _StepSize;
                    distTravelled += _StepSize;
                }
                
                return lerp(sceneColor, _FogColor, 1.0 - saturate(transmittance));
            }
            ENDHLSL
        }
    }
}
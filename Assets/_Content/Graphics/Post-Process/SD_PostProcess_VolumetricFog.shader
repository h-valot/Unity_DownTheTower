Shader "Hidden/VolumetricFog"
{
    Properties
    {
        _MaxDistance("Max Distance", float) = 100
        _StepSize("Distance between each sample along rays", Range(0.1, 20)) = 1
        _StepNoiseOffset("Scale random step offset", float) = 0
        _DensityMultiplier("Fog Density", Range(0.0001, 10)) = 1
        _FogColor("Color of the fog", Color) = (1,1,1,1)

        _FogNoise("Fog noise", 3D) = "white" {}
        _FogNoiseTiling("Fog noise tiling", float) = 1
        _FogNoiseOffset("Fog noise offset", Vector) = (0,0,0,0)
        _FogNoiseThreshold("Fog noise threshold", float) = 0
        _FogDirection("Fog Direction", Vector) = (1,0,0,0)
        _FogSpeed("Fog Speed", float) = 0.1
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
            float _DensityMultiplier;
            float4 _FogColor;
            float _StepNoiseOffset;
            TEXTURE3D(_FogNoise);
            float _FogNoiseTiling;
            float3 _FogNoiseOffset;
            float _FogNoiseThreshold;
            float3 _FogDirection;
            float _FogSpeed;

            float getDensity(float3 samplePos)
            {
                float noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, samplePos * _FogNoiseTiling + saturate(_FogDirection) * _FogSpeed * _Time.y, 0).x;
                float density = _DensityMultiplier - noise * _FogNoiseThreshold;
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
                float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _StepNoiseOffset;
                float transmittance = 1;
                
                while(distTravelled < distLimit)
                {
                    float3 samplePos = _WorldSpaceCameraPos + rayDir * distTravelled;
                    float density = getDensity(samplePos);

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
Shader "Hidden/VolumetricFog"
{
    Properties
    {
        _MaxDistance("Max Distance", float) = 100
        _StepSize("Distance between each sample along rays", Range(0.1, 20)) = 1
        _DensityMultiplier("Fog Density", Range(0.0001, 0.1)) = 1
        _FogColor("Color of the fog", Color) = (1,1,1,1)
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag

            float _MaxDistance;
            float _StepSize;
            float _DensityMultiplier;
            float4 _FogColor;

            float getDensity()
            {
                return _DensityMultiplier;
            }
            
            half4 frag(Varyings IN) : SV_TARGET
            {
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float distLimit = min(viewLength, _MaxDistance);
                float sampleNumber = int(distLimit / _StepSize);
                float transmittance = 1;
                
                for (int i=0; i < sampleNumber; i++)
                {
                    float density = getDensity();

                    transmittance *= exp(-density * _StepSize);
                }
                
                return lerp(sceneColor, _FogColor, 1.0 - saturate(transmittance));
            }
            ENDHLSL
        }
    }
}
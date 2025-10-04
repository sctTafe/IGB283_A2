Shader "Universal Render Pipeline/IGB283 Shader - Vertex Displacement + PBR"
{
    Properties
    {
        // --- Displacement ---
        _MainTex("Displacement Texture", 2D) = "white" {}
        _DisplacementStrength("Displacement Strength", Range(0, 1)) = 0.1
        _DisplacementSpeed("Displacement Speed", Range(0, 10)) = 1.0

        // --- PBR Textures ---
        _BaseColor("Base Color (Albedo)", 2D) = "white" {}
        _MetallicMap("Metallic Map", 2D) = "black" {}
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0, 2)) = 1.0

        // --- Tint ---
        _ColorTint("Tint", Color) = (1,1,1,1)
        
        // --- PBR Properties ---
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            // URP Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            // URP Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };

            // --- Displacement Inputs ---
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _DisplacementStrength;
            float _DisplacementSpeed;

            // --- PBR Inputs ---
            TEXTURE2D(_BaseColor);
            SAMPLER(sampler_BaseColor);
            TEXTURE2D(_MetallicMap);
            SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            float4 _ColorTint;
            float _NormalScale;
            float _Smoothness;
            float _Metallic;

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Animate UVs for displacement
                float2 displacedUV = input.uv + _Time.y * _DisplacementSpeed;

                // Sample displacement height using tex2Dlod
                float displacement = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, displacedUV, 0).r;
                
                // Displace vertex along normal
                float3 displacedVertex = input.positionOS.xyz + input.normalOS * (displacement * _DisplacementStrength);

                // Transform to world space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(displacedVertex);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                // Output
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                
                // TBN vectors
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // --- Sample Textures ---
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseColor, sampler_BaseColor, input.uv) * _ColorTint;
                half metallic = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv).r * _Metallic;
                half smoothness = (1.0 - SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r) * _Smoothness;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalScale);

                // --- Build TBN for normal mapping ---
                half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                half3 normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
                normalWS = normalize(normalWS);

                // --- Setup for PBR Lighting ---
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = normalize(GetCameraPositionWS() - input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.bakedGI = SAMPLE_GI(input.uv, 0, normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.uv);

                // --- Setup Surface Data ---
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.alpha = baseColor.a;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = 0;
                surfaceData.specular = 0;

                // --- URP PBR Lighting ---
                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                return color;
            }
            ENDHLSL
        }


    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
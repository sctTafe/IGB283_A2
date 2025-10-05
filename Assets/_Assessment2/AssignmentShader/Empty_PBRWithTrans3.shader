Shader "Unlit/Custom PBR Transparent 2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _MetallicGlossMap ("Metallic Map", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _Alpha ("Transparency", Range(0,1)) = 1.0



    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 200
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            

            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MetallicGlossMap);
            SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            
            // CBUFFER
            /*
            To use any property in a shader, we must introduce it in the shader code, since the
            properties section is separated from the subshader internally. We will need to define
            all variables (excepting textures) in a section contained by CBUFFER_START and CBUFFER_END
            */
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                half _Glossiness;
                half _Metallic;
                half _BumpScale;
                half _Alpha;
                float4 _Color;
            CBUFFER_END
            
            // ATTRIBUTES
              
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvBump : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 binormalWS : TEXCOORD5;
                float4 shadowCoord : TEXCOORD6;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvBump = TRANSFORM_TEX(input.uv, _BumpMap);
                
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.binormalWS = normalInput.bitangentWS;
                
                output.shadowCoord = GetShadowCoord(vertexInput);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uvBump));
                normalTS.xy *= _BumpScale;
                
                // Build TBN matrix
                float3x3 TBN = float3x3(input.tangentWS, input.binormalWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, TBN));
                
                // Get main light
                Light mainLight = GetMainLight(input.shadowCoord);
                
                // Material properties
                half metallic = metallicGloss.r * _Metallic;
                half smoothness = metallicGloss.a * _Glossiness;
                half perceptualRoughness = 1.0 - smoothness;
                half roughness = perceptualRoughness * perceptualRoughness;
                
                // Lighting vectors
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
                float3 lightDirWS = mainLight.direction;
                float3 halfDir = normalize(lightDirWS + viewDirWS);
                
                // Dot products
                half NdotL = saturate(dot(normalWS, lightDirWS));
                half NdotH = saturate(dot(normalWS, halfDir));
                half NdotV = saturate(dot(normalWS, viewDirWS));
                half LdotH = saturate(dot(lightDirWS, halfDir));
                
                // Fresnel (Schlick)
                half3 F0 = lerp(half3(0.04, 0.04, 0.04), albedo.rgb, metallic);
                half3 F = F0 + (1.0 - F0) * pow(1.0 - NdotV, 5.0);
                
                // GGX Distribution
                half a2 = roughness * roughness;
                half d = NdotH * NdotH * (a2 - 1.0) + 1.0;
                half D = a2 / (PI * d * d);
                
                // Geometry
                half k = roughness * 0.5;
                half G1 = NdotV / (NdotV * (1.0 - k) + k);
                half G2 = NdotL / (NdotL * (1.0 - k) + k);
                half G = G1 * G2;
                
                // Specular
                half3 specular = D * F * G / max(4.0 * NdotL * NdotV, 0.001);
                
                // Diffuse
                half3 kD = (1.0 - F) * (1.0 - metallic);
                half3 diffuse = kD * albedo.rgb / PI;
                
                // Combine with light
                half3 radiance = mainLight.color * mainLight.shadowAttenuation * NdotL;
                half3 color = (diffuse + specular) * radiance;
                
                // Add ambient
                half3 ambient = SampleSH(normalWS) * albedo.rgb * (1.0 - metallic);
                color += ambient;
                
                return half4(color, albedo.a * _Alpha);
            }
            ENDHLSL
        }
 
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

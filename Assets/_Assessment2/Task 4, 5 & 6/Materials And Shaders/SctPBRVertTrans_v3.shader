/*
Author: Scott Barley
Date 05/10/25
IS: Combination of simple shaders writen for Unity URP pipline for learning peruposes
DOSE: 
- Allows PBR texture input (like a really simple version)
- Allows for Transparance 
- Allows for Vertext Displacement on the XY plain
*/
Shader "Unlit/SctPBRVertTrans_v3"
{
    Properties
    {
        // PBR Mat Inputs
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _MetallicGlossMap ("Metallic Map", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0

        // Transparency
        _Alpha ("Transparency", Range(0,1)) = 1.0

        // Colour Tint
        _Color ("Color", Color) = (1,1,1,1)

        // Vertex Displacement
        _Amplitude ("Wave Size", Range(0,1)) = 0.4
        _Frequency ("Wave Freqency", Range(1, 8)) = 2
        _AnimationSpeed ("Animation Speed", Range(0,5)) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            /*
            --- TAGS ---

            -- Render Type --
            ("RenderType"="Transparent")
            DOSE:
                RenderType is a classification tag that labels what type of shader this is. It's primarily used for:
                    - Replacement Shaders
                    - Scene View Rendering Modes (Wireframe, Overdraw, etc.)
                    - Shader Categorization - understand the shader's intended behavior,

            OTHERS
                "Opaque" - Solid objects
                "Transparent" - See-through objects
                "TransparentCutout" - Textures with alpha cutoff (like foliage)
                "Background" - Skyboxes
                "Overlay" - UI elements

            -- Queue tags --
            ("Queue"="Transparent")
            DOSE:
                Queue tags control the renders order during the frame. Unity renders objects in multiple passes in a specific order:
            OTHERS:             
                Background (skybox) - Queue index 1000
                Geometry (opaque objects) - Queue index 2000
                AlphaTest (cutout shaders) - Queue index 2450
                Transparent (transparent objects) - Queue index 3000
                Overlay (UI, effects) - Queue index 4000
            */

            "RenderType"="Transparent" 
            //"Queue"="Geometry" // Opaque objects (Geometry queue) render front-to-back with ZWrite On. They write to the depth buffer, so objects behind them get culled early for performance.
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        /*
        -- LOD --
        (LOD 500)
            IS: complexity threshold for the shader
            DOSE: 
                - can set a maximum shader LOD globally in the project or per-camera:
                - Unity will only use SubShaders with LOD values less than or equal to this maximum.
                - If a shader's LOD is too high, Unity falls back to the next SubShader with a lower LOD, or eventually to the FallBack shader.
            OTHERS:
                Common LOD Values (Unity Standard)
                    LOD 600 - Complex shaders (multiple lights, reflections, advanced effects)
                    LOD 400 - Standard PBR shaders (diffuse + specular + normal maps)
                    LOD 300 - Simplified PBR (fewer texture samples)
                    LOD 200 - Basic diffuse + normal mapping
                    LOD 100 - Very simple (diffuse only, minimal lighting)

        */
        LOD 500
        
        Pass
        {
            /*
            IS: Pass Name
            DOSE:
                -  identifier in the Frame Debugger
                - Other shaders can reuse this pass using this name tag
            */
            Name "ForwardLit"


            /*
            IS:  rendering pass tag
            DOSE: 
                - tells URP which rendering pass this belongs to.
            OTRHERS:
                UniversalForward - Main rendering pass with lighting
                ShadowCaster - Renders shadows
                DepthOnly - Depth pre-pass
                Meta - Lightmap baking
                Universal2D - 2D lighting
            */
            Tags { "LightMode"="UniversalForward" }
            
            /*
            (Alpha Blending)
            DOSE:
                controls alpha blending, how transparent pixels mix with what's already rendered.
            CURRENT:
                SrcAlpha - Multiply the new pixel by its alpha value
                OneMinusSrcAlpha - Multiply the background by (1 - alpha)
                => FinalColor = (SourceColor × SourceAlpha) + (DestinationColor × (1 - SourceAlpha))
             */
            Blend SrcAlpha OneMinusSrcAlpha
            
            /*
            (ZWrite Off)
             Disbaled Write to 'Depth buffer' so that:
             -  Transparent object renders but doesn't block depth
             -  Objects behind it can still render and blend
             NOTE: Disabiliing it has risks; artifacts, order issues, occlusion issses
             */
            ZWrite Off
            
            /*
            (face culling)
            OTHERS:
                Cull Back (default) - Don't render back-facing triangles
                Cull Front - Don't render front-facing triangles
                Cull Off - Render both sides
             */
            Cull Back
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            /*
            PARAMA's
            DOSE:
                Assgins function to be called
                vert -               
                    (tells the GPU "The function named vert is my vertex shader")
                frag -  
                    (tells the GPU "The function named frag is my fragment (pixel) shader")
            HOW:
                vert -
                    Links 'Varyings' vert(Attributes input) function to the vertex stage
                    The vertex shader runs once per vertex in the mesh
                    Transforms vertices from object space to screen space
                frag - 

            OTHER:
                They link to Core Pipleine Statges
                - #pragma vertex vert          // Vertex shader (required)
                - #pragma fragment frag        // Fragment/Pixel shader (required)
                - #pragma geometry geom        // Geometry shader (optional)
                - #pragma hull hull            // Hull shader for tessellation (optional)
                - #pragma domain domain        // Domain shader for tessellation (optional)

                Other Additiona types:  (NOTE: Lots more to lookup and learn, the are some cool URP summary tables)
                - Lighting Variants - "#pragma multi_compile _ _MAIN_LIGHT_SHADOWS"
                - Fog Variants - "#pragma multi_compile_fog"
                - Instancing - "#pragma multi_compile_instancing"
                - LightMapping -"#pragma multi_compile _ LIGHTMAP_ON"
            */
            #pragma vertex vert
            #pragma fragment frag
         
            /*
            Texture & sampeling Declarations
            DOSE: 
                Declares texture types and sampling type and links them to the 'Properties block'
            */
            TEXTURE2D(_MainTex);        // Declare texture type data
            SAMPLER(sampler_MainTex);   // Declare how to sample it

            TEXTURE2D(_MetallicGlossMap);
            SAMPLER(sampler_MetallicGlossMap);

            TEXTURE2D(_BumpMap);        // NOTE: Normal Map
            SAMPLER(sampler_BumpMap);
            
            /*
            ATTRIBUTES           
            DOSE: 
                defines what data comes FROM the mesh into the vertex shader.
            HOW:
                tells Unity which mesh data to bind to each variable:
            */
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            


            struct Varyings
            {
                float4 positionCS : SV_POSITION;  // Screen position (required!)
                float2 uv : TEXCOORD0;            // Main texture UVs
                float2 uvBump : TEXCOORD1;        // Normal map UVs
                float3 positionWS : TEXCOORD2;    // World space position
                float3 normalWS : TEXCOORD3;      // World space normal
                float3 tangentWS : TEXCOORD4;     // World space tangent
                float3 binormalWS : TEXCOORD5;    // World space binormal
                float4 shadowCoord : TEXCOORD6;   // Shadow map coordinates
            };

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
                // Vertex Displacement
                float _Amplitude;
                float _Frequency;
                float _AnimationSpeed;
            CBUFFER_END

            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // --- Vertex displacement ---
                float3 pos = input.positionOS.xyz;
                float time = _Time.y * _AnimationSpeed;

                float3 displacedPos = pos;
                displacedPos.y += sin(pos.x * _Frequency + time) * _Amplitude;

                // --- Normal recalculation (finite difference method) ---
                float3 bitangent = cross(input.normalOS, input.tangentOS.xyz);
                
                float3 posPlusTangent = pos + input.tangentOS.xyz * 0.01;
                posPlusTangent.y += sin(posPlusTangent.x * _Frequency + time) * _Amplitude;

                float3 posPlusBitangent = pos + bitangent * 0.01;
                posPlusBitangent.y += sin(posPlusBitangent.x * _Frequency + time) * _Amplitude;

                float3 modifiedTangent   = posPlusTangent - displacedPos;
                float3 modifiedBitangent = posPlusBitangent - displacedPos;
                float3 modifiedNormal    = normalize(cross(modifiedTangent, modifiedBitangent));

                // Get vertex positions using displaced position
                VertexPositionInputs vertexInput = GetVertexPositionInputs(displacedPos);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvBump = TRANSFORM_TEX(input.uv, _BumpMap);
                
                // Use modified normal for lighting calculations
                output.normalWS = TransformObjectToWorldNormal(modifiedNormal);
                output.tangentWS = TransformObjectToWorldDir(modifiedTangent);
                output.binormalWS = TransformObjectToWorldDir(modifiedBitangent);
                         
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

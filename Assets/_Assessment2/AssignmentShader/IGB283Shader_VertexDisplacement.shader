Shader "Unlit/IGB283 Shader - Vertex Displacement"
{
    //values shown in inspector
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MainColour("Colour", Color) = (1, 1, 1, 1)

        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR] _Emission ("Emission", color) = (0,0,0)

        _Amplitude ("Wave Size", Range(0,1)) = 0.4
        _Frequency ("Wave Freqency", Range(1, 8)) = 2
        _AnimationSpeed ("Animation Speed", Range(0,5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            HLSLPROGRAM         
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  // necessary functions for all shaders
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"// contains lighting functions

            // PARAM
            #pragma vertex vert
            #pragma fragment frag

            // STRUCTS
            /* 
            Two data structures that will be passed
            between functions in the render pipeline, which you might see called “Attributes” or
            “App Data”, and “Varyings”, “v2f”, or “Interpolators”. The Attributes data structure
            receives base mesh data per vertex while the Varyings struct stores the interpolated
            mesh data going from vertex to fragment (through the rasterizer)
            */

            // ATTRIBUTES
            struct Attributes
            {
            /*
            The positionOS and positionHCS variables represent the position of a mesh’s
            vertex (or pixel) in object space (hence OS) and homogeneous clip space (HCS)
            respectively
            */
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
            };

            struct Varyings
            {
                 float4 positionHCS : SV_POSITION;
                 float3 normalWS : NORMAL;
            };

            // CBUFFER
            /*
            To use any property in a shader, we must introduce it in the shader code, since the
            properties section is separated from the subshader internally. We will need to define
            all variables (excepting textures) in a section contained by CBUFFER_START and CBUFFER_END
            */
            CBUFFER_START(UnityPerMaterial)
                half4 _MainColour;
                float _Amplitude;
                float _Frequency;
                float _AnimationSpeed;
            CBUFFER_END



            // Vertex function with sinisoidal displacement + recomputed normal
            /*
            The vertex shader takes our Attributes struct as an input and returns the Varyings
            struct (we don’t have to worry about calling the vertex function ourselves). In the vertex
            shader, we will transform the mesh data into the appropriate spaces (clip and world),
            which will eventually be used in the fragment shader
            */
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // --- Vertex displacement ---
                float3 pos = IN.positionOS.xyz;
                float time = _Time.y * _AnimationSpeed;

                float3 displacedPos = pos;
                displacedPos.y += sin(pos.x * _Frequency + time) * _Amplitude;

                // --- Normal recalculation (finite difference method) ---
                float3 posPlusTangent = pos + IN.tangentOS.xyz * 0.01;
                posPlusTangent.y += sin(posPlusTangent.x * _Frequency + time) * _Amplitude;

                float3 bitangent = cross(IN.normalOS, IN.tangentOS.xyz);
                float3 posPlusBitangent = pos + bitangent * 0.01;
                posPlusBitangent.y += sin(posPlusBitangent.x * _Frequency + time) * _Amplitude;

                float3 modifiedTangent   = posPlusTangent - displacedPos;
                float3 modifiedBitangent = posPlusBitangent - displacedPos;
                float3 modifiedNormal    = normalize(cross(modifiedTangent, modifiedBitangent));

                // Output clip position & world normal
                OUT.positionHCS = TransformObjectToHClip(float4(displacedPos, 1.0));
                OUT.normalWS    = TransformObjectToWorldNormal(modifiedNormal);

                return OUT;
            }



            /*
            use the dot product to find the difference in angle between the light
            direction and surface normal. We want a smaller difference to make a brighter light,
            and a larger difference to make a dimmer light. Essentially, the more the light and
            surface point towards each other, the brighter the light should be
            */
            // Simple lambert lighting
            half4 frag(Varyings IN) : SV_TARGET
            {
                // Get the light attributes
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal   = normalize(IN.normalWS);

                // Normalize vectors
                // NOTE: Using saturate on the dot product result will clamp the value between 0 and 1, which prevents unusual lighting where the value is negative.
                half NdotL = saturate(dot(normal, lightDir));
                half3 lighting = NdotL * mainLight.color.rgb;

                // Apply the lighting to the colour
                half4 colour = _MainColour;
                colour.rgb *= lighting;

                return colour;
            }

            ENDHLSL
        }
    }
}

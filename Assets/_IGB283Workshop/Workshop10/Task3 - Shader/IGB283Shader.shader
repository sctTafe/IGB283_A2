Shader "Unlit/IGB283 Shader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MainColour("Colour", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            struct Attributes
            {
            /*
            The positionOS and positionHCS variables represent the position of a mesh’s
            vertex (or pixel) in object space (hence OS) and homogeneous clip space (HCS)
            respectively
            */
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
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
            CBUFFER_END


            /*
            The vertex shader takes our Attributes struct as an input and returns the Varyings
            struct (we don’t have to worry about calling the vertex function ourselves). In the vertex
            shader, we will transform the mesh data into the appropriate spaces (clip and world),
            which will eventually be used in the fragment shader
            */
            Varyings vert(Attributes IN)
            {
                /*
                 convert the position from object space to homogeneous clip space, and the normal from object 
                 space to world space
                */
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }


            /*
Displacement            use the dot product to find the difference in angle between the light
            direction and surface normal. We want a smaller difference to make a brighter light,
            and a larger difference to make a dimmer light. Essentially, the more the light and
            surface point towards each other, the brighter the light should be
            */
            half4 frag(Varyings IN) : SV_TARGET
            {
                // Get the light attributes
                Light mainLight = GetMainLight();
                float3 lightDirection = mainLight.direction;
                half3 lightColour = mainLight.color;


                // Normalize vectors
                float3 normal = normalize(IN.normalWS);
                lightDirection = normalize(lightDirection);
 
                // Calculate the lighting
                // NOTE: Using saturate on the dot product result will clamp the value between 0 and 1, which prevents unusual lighting where the value is negative.
                half3 lighting = saturate(dot(normal, lightDirection));


                // Apply the lighting to the colour
                half4 colour = _MainColour;
                colour.rgb *= lighting;

                return colour;
            }




            ENDHLSL
        }
    }
}

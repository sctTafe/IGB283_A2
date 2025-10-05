Shader "Unlit/Custom PBR Transparent"
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
       Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            //Tags { "LightMode"="ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            ColorMask RGB

            HLSLPROGRAM         
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // PARAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MetallicGlossMap;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
            
            half _Glossiness;
            half _Metallic;
            half _BumpScale;
            half _Alpha;
            fixed4 _Color;

             // ATTRIBUTES
             struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };


                        struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvBump : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 worldTangent : TEXCOORD4;
                float3 worldBinormal : TEXCOORD5;
                SHADOW_COORDS(6)
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvBump = TRANSFORM_TEX(v.uv, _BumpMap);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample textures
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
                fixed4 metallicGloss = tex2D(_MetallicGlossMap, i.uv);
                fixed3 normalTex = UnpackNormal(tex2D(_BumpMap, i.uvBump));
                normalTex.xy *= _BumpScale;
                
                // Build TBN matrix for normal mapping
                float3x3 TBN = float3x3(i.worldTangent, i.worldBinormal, i.worldNormal);
                float3 worldNormal = normalize(mul(normalTex, TBN));
                
                // Lighting calculations
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + viewDir);
                
                float metallic = metallicGloss.r * _Metallic;
                float smoothness = metallicGloss.a * _Glossiness;
                float roughness = 1.0 - smoothness;
                float perceptualRoughness = roughness * roughness;
                
                // Simplified PBR
                float NdotL = max(0.0, dot(worldNormal, lightDir));
                float NdotH = max(0.0, dot(worldNormal, halfDir));
                float NdotV = max(0.0, dot(worldNormal, viewDir));
                
                // Fresnel (Schlick approximation)
                float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo.rgb, metallic);
                float3 F = F0 + (1.0 - F0) * pow(1.0 - NdotV, 5.0);
                
                // Specular (GGX)
                float alpha = perceptualRoughness * perceptualRoughness;
                float denom = NdotH * NdotH * (alpha - 1.0) + 1.0;
                float D = alpha / (3.14159265 * denom * denom);
                
                // Diffuse
                float3 kD = (1.0 - F) * (1.0 - metallic);
                float3 diffuse = kD * albedo.rgb / 3.14159265;
                
                // Combine
                float3 specular = D * F / max(4.0 * NdotL * NdotV, 0.001);
                
                // Shadows
                float shadow = SHADOW_ATTENUATION(i);
                
                // Final color
                float3 color = (diffuse + specular) * _LightColor0.rgb * NdotL * shadow;
                color += albedo.rgb * unity_AmbientSky.rgb * 0.3;
                
                return fixed4(color, albedo.a * _Alpha);
            }










            ENDHLSL
        }
    }
}

Shader "Unlit/IGB283 Shader - PBR Mat"
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

        // --- Tint ---
        _ColorTint("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
                Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
       Tags { "LightMode"="ForwardBase" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"  // Use alias-style includes instead of full path
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBinormal : TEXCOORD4;
            };

            // --- Displacement Inputs ---
            sampler2D _MainTex;
            float _DisplacementStrength;
            float _DisplacementSpeed;

            // --- PBR Inputs ---
            sampler2D _BaseColor;
            sampler2D _MetallicMap;
            sampler2D _RoughnessMap;
            sampler2D _NormalMap;
            float4 _ColorTint;

            v2f vert(appdata v)
            {
                v2f o;

                // Animate UVs for displacement
                float2 displacedUV = v.uv + _Time.y * _DisplacementSpeed;

                // Sample displacement height
                float displacement = tex2D(_MainTex, displacedUV).r;
                float3 displacedVertex = v.vertex.xyz + v.normal * (displacement * _DisplacementStrength);

                // Output position
                o.pos = UnityObjectToClipPos(float4(displacedVertex, 1.0));
                o.uv = v.uv;

                // World-space data
                o.worldPos = mul(unity_ObjectToWorld, float4(displacedVertex, 1.0)).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;

                return o;
            }




            float4 frag(v2f i) : SV_Target
            {
                // --- Sample Textures ---
                float4 baseCol = tex2D(_BaseColor, i.uv) * _ColorTint;
                float metallic = tex2D(_MetallicMap, i.uv).r;
                float roughness = tex2D(_RoughnessMap, i.uv).r;
                float3 normalTex = UnpackNormal(tex2D(_NormalMap, i.uv));

                // --- Build TBN for normal mapping ---
                float3x3 TBN = float3x3(i.worldTangent, i.worldBinormal, i.worldNormal);
                float3 worldNormal = normalize(mul(TBN, normalTex));

                // --- Lighting ---
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);

                // Diffuse
                float NdotL = saturate(dot(worldNormal, lightDir));
                float3 diffuse = baseCol.rgb * _LightColor0.rgb * NdotL;

                // Specular (basic)
                float NdotH = saturate(dot(worldNormal, halfDir));
                float specPower = pow(1.0 - roughness, 4.0) * 128;
                float3 specular = pow(NdotH, specPower) * _LightColor0.rgb * metallic;

                // Combine
                float3 finalColor = diffuse + specular;

                return float4(finalColor, baseCol.a);
            }



            ENDHLSL
        }
    }
}

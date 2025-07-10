Shader "Custom/LowPolyFlatShading"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma target 4.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            fixed4 _Color;

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.normal = v.normal;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float3 faceNormal = normalize(IN[0].normal + IN[1].normal + IN[2].normal);

                g2f o;
                for (int i = 0; i < 3; i++)
                {
                    o.vertex = UnityObjectToClipPos(IN[i].vertex);
                    o.worldNormal = UnityObjectToWorldNormal(faceNormal);
                    triStream.Append(o);
                }
                triStream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float diff = max(0, dot(normal, lightDir));
                fixed4 col = _Color * (diff * _LightColor0 + unity_AmbientSky);
                return col;
            }
            ENDCG
        }
    }
}
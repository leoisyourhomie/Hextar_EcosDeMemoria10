Shader "Custom/LuminanceShader"
{
    Properties
    {
        _OutlineColor ("Color del Glow", Color) = (0, 1, 0, 1)
        _BaseThickness ("Grosor Base", Range (0.0, 1.0)) = 0.2
        _AnimSpeed ("Velocidad del Pulso", Range (0.0, 10.0)) = 2.0
        _GlowFalloff ("Difuminado (Fuerza)", Range (1.0, 10.0)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            Name "GLOW_ANIMADO"
            Cull Front
            ZWrite Off
            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float _BaseThickness;
            float _AnimSpeed;
            float4 _OutlineColor;
            float _GlowFalloff;

            v2f vert (appdata v)
            {
                v2f o;
                
                // _Time.g contiene el tiempo de Unity escalado de forma normal.
                // Usamos la función sin() para crear una onda matemática que oscila entre -1 y 1 de forma infinita.
                float onda = sin(_Time.g * _AnimSpeed);
                
                // Convertimos la onda de (-1 a 1) a un rango de (0 a 1) para que el stroke nunca se haga negativo o se invierta.
                float factorPulso = (onda * 0.5) + 0.5;
                
                // El grosor final varía dinámicamente según el pulso del tiempo
                float grosorAnimado = _BaseThickness * factorPulso;
                
                // Inflamos los vértices con el nuevo grosor animado
                float3 norm = normalize(v.normal);
                v.vertex.xyz += norm * grosorAnimado;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = norm;
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float dotProduct = 1.0 - saturate(dot(normalize(i.viewDir), normalize(i.normal)));
                float glow = pow(dotProduct, _GlowFalloff);
                
                half4 finalColor = _OutlineColor;
                finalColor.a = glow;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
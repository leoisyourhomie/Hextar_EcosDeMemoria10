Shader "Custom/LuminanceObject"
{
    Properties
    {
        [Header(Glow Sutil)]
        _GlowColor ("Color del Brillo", Color) = (0, 1, 0, 1)
        _MaxGlowIntensity ("Intensidad Maxima", Range(0.0, 5.0)) = 1.5
        _GlowFalloff ("Difuminado", Range(0.1, 5.0)) = 2.0
        
        [Header(Animacion Fade)]
        _FadeSpeed ("Velocidad Base", Range(0.01, 3.0)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend One One
        ZWrite Off
        Cull Off
        ZTest LEqual 

        CGPROGRAM
        #pragma surface surf Unlit vertex:vert noforwardadd noambient nolightmap nodirlightmap
        #pragma multi_compile_instancing

        fixed4 _GlowColor;
        half _MaxGlowIntensity;
        half _GlowFalloff;
        half _FadeSpeed;

        // Variables únicas por objeto inyectadas desde C#
        half _ObjetoDelay;
        half _ObjetoSpeedMod;

        struct Input
        {
            half3 viewDir;
        };

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            UNITY_SETUP_INSTANCE_ID(v);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            half rim = 1.0h - saturate(dot(normalize(IN.viewDir), o.Normal));
            half finalRim = pow(rim, _GlowFalloff);

            // Sumamos el modificador único a la velocidad base para romper el ciclo armónico
            half velocidadUnica = _FadeSpeed + _ObjetoSpeedMod;

            half ondaTime = sin((_Time.y * velocidadUnica) + _ObjetoDelay);
            half factorFade = (ondaTime * 0.5h) + 0.5h;

            o.Emission = _GlowColor.rgb * finalRim * (_MaxGlowIntensity * factorFade);
            o.Albedo = fixed3(0, 0, 0);
            o.Alpha = 1.0h;
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}
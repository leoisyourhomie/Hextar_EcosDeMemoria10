Shader "Expiria3D/ChromaKey" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MaskCol ("Mask Color", Color)  = (0, 255, 0.0, 1.0)
        _Sensitivity ("Threshold Sensitivity", Range(0,1)) = 0.15
        _Smooth ("Smoothing", Range(0,1)) = 0.01
        [Enum(Back, 2, Front, 1, Off, 0)] _CullMode ("Cull Mode", Float) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" } // Cambiado a Transparent-10
        LOD 100

        Cull [_CullMode]
        ZWrite Off
        // No es necesario especificar ZTest LEqual ya que es el valor predeterminado
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }

        CGPROGRAM
        #pragma surface surf Lambert alpha

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        float4 _MaskCol;
        float _Sensitivity;
        float _Smooth;

        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);

            float maskY = 0.2989 * _MaskCol.r + 0.5866 * _MaskCol.g + 0.1145 * _MaskCol.b;
            float maskCr = 0.7132 * (_MaskCol.r - maskY);
            float maskCb = 0.5647 * (_MaskCol.b - maskY);

            float Y = 0.2989 * c.r + 0.5866 * c.g + 0.1145 * c.b;
            float Cr = 0.7132 * (c.r - Y);
            float Cb = 0.5647 * (c.b - Y);

            float blendValue = smoothstep(_Sensitivity, _Sensitivity + _Smooth, distance(float2(Cr, Cb), float2(maskCr, maskCb)));
            o.Albedo = c.rgb;
            o.Alpha = blendValue;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
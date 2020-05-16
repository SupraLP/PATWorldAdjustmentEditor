Shader "Custom/MyIntersectionShader" {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,0)
        _GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _FadeLength("Fade Length", Range(0, 2)) = 0.15
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
 
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
 
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 screenuv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _FadeLength;
 
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenuv = ComputeScreenPos(o.vertex);
                COMPUTE_EYEDEPTH(o.screenuv.z);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
 
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenuv)));
                float partZ = i.screenuv.z;
                float diff = sceneZ - partZ;
                float intersect = 0;
 
                intersect = saturate(diff / _FadeLength);
 
                return fixed4(lerp(tex2D(_MainTex, i.uv) * _Color, _GlowColor, intersect));
            }
            ENDCG
        }
    }
}

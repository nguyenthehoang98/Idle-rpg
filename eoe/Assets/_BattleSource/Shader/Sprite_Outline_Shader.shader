Shader "Game/Sprite-Outline"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineSize;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float2 offset = _MainTex_TexelSize.xy * _OutlineSize;

                float a1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(offset.x,0)).a;
                float a2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(offset.x,0)).a;
                float a3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0,offset.y)).a;
                float a4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(0,offset.y)).a;

                float outline = max(max(a1,a2), max(a3,a4));

                if (c.a <= 0.01 && outline > 0.01)
                    return _OutlineColor;

                return c;
            }

            ENDHLSL
        }
    }
}
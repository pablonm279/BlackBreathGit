Shader "GDD/Campania/PasoVientoHeladoAurora"
{
    Properties { _RuidoTex ("Ruido", 2D) = "gray" {} _Intensidad ("Intensidad", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Background+15" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off ZTest LEqual Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _RuidoTex;
            float _Intensidad;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float3 color=0;
                float t=_Time.y;
                for (int k=0;k<3;k++)
                {
                    float fase=k*2.71;
                    float centro=.61+k*.075+sin(i.uv.x*(4.1+k*.7)+fase+t*.035)*.052
                        +sin(i.uv.x*10.3-fase+t*.021)*.018;
                    float dy=i.uv.y-centro;
                    float raiz=exp2(-abs(dy)*100);
                    float velo=smoothstep(-.008,.012,dy)*exp2(-max(0,dy)*19);
                    float hilo=tex2D(_RuidoTex,float2(i.uv.x*(5.7+k*.4)+fase,t*.003+dy*.17)).r;
                    float variacion=tex2D(_RuidoTex,float2(i.uv.x*.10+fase,t*.0007)).r;
                    float forma=(raiz*.54+velo*.25)*(.22+hilo*hilo*.78)*lerp(.45,1,variacion);
                    float3 tono=lerp(float3(.10,.72,.42),float3(.10,.44,.73),saturate(dy*6+variacion*.45));
                    tono=lerp(tono,float3(.43,.19,.57),smoothstep(.06,.18,dy)*.45);
                    color+=tono*forma;
                }
                float borde=smoothstep(.02,.16,i.uv.x)*(1-smoothstep(.84,.98,i.uv.x))
                    *smoothstep(.50,.60,i.uv.y)*(1-smoothstep(.94,1,i.uv.y));
                return fixed4(color,_Intensidad*borde);
            }
            ENDCG
        }
    }
    Fallback Off
}

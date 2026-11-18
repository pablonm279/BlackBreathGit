Shader "GDD/Campania/PasoVientoHeladoClimaParticulas"
{
    Properties
    {
        _Color ("Color", Color) = (0.8,0.9,1,0.4)
        _Tinte ("Tinte horario", Color) = (1,1,1,1)
        _Luz ("Luz", Float) = 1
        _Modo ("0 copo, 1 lluvia, 2 impacto, 3 niebla, 4 cristal", Float) = 0
        _RuidoTex ("Ruido", 2D) = "gray" {}
        _SuavidadProfundidad ("Contacto suave", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _RuidoTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            fixed4 _Color, _Tinte;
            float _Luz, _Modo, _SuavidadProfundidad;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; float4 pantalla:TEXCOORD1; float3 world:TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color;
                o.pantalla=ComputeScreenPos(o.pos); COMPUTE_EYEDEPTH(o.pantalla.z);
                o.world=mul(unity_ObjectToWorld,v.vertex).xyz; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float2 q=i.uv*2-1;
                float r=length(q);
                float alpha=1-smoothstep(.08,1,r);
                if (_Modo>.5 && _Modo<1.5)
                    alpha=(1-smoothstep(.04,.38,abs(q.x)))*(1-smoothstep(.35,1,abs(q.y)));
                else if (_Modo>1.5 && _Modo<2.5)
                    alpha=(1-smoothstep(.07,.23,abs(r-.55)))*(1-smoothstep(.8,1,r));
                else if (_Modo>2.5 && _Modo<3.5)
                {
                    float n=tex2D(_RuidoTex,i.uv*.060+i.world.xz*.006+float2(_Time.y*.001,0)).r;
                    float detalle=tex2D(_RuidoTex,i.uv*.16+float2(17,-_Time.y*.0007)).r;
                    alpha=pow(saturate(1-r*r),2)*smoothstep(.16,.74,n*.68+detalle*.32);
                }
                else if (_Modo>3.5)
                {
                    float cruz=exp2(-abs(q.x)*22)*exp2(-abs(q.y)*4)
                        +exp2(-abs(q.y)*22)*exp2(-abs(q.x)*4);
                    alpha=saturate(cruz*.65)*(1-smoothstep(.7,1,r));
                }
                float escena=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.pantalla)));
                float contacto=saturate((escena-i.pantalla.z)/max(.01,_SuavidadProfundidad));
                return fixed4(_Color.rgb*_Tinte.rgb*i.color.rgb*_Luz,_Color.a*i.color.a*alpha*contacto);
            }
            ENDCG
        }
    }
    Fallback Off
}

Shader "GDD/Campania/PasoVientoHeladoBrumaFondo"
{
    SubShader
    {
        Tags { "Queue"="Transparent-50" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _PasoLuzCielo;
            fixed4 _PasoTinteCielo;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};
            v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float ruido(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
            fixed4 frag(v2f i):SV_Target
            {
                float borde=sin(saturate(i.uv.x)*3.14159)*sin(saturate(i.uv.y)*3.14159);
                float n=ruido(float2(i.world.x*.055-_Time.y*.015,i.world.y*.15+i.world.z*.03));
                float alpha=pow(saturate(borde),1.8)*smoothstep(.18,.78,n)*.32;
                return fixed4(fixed3(.50,.59,.68)*_PasoTinteCielo.rgb*_PasoLuzCielo,alpha);
            }
            ENDCG
        }
    }
}

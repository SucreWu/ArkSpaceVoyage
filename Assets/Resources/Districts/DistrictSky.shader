Shader "ArkWalk/星野穹顶"
{
    Properties { _MainTex("星野", 2D) = "black" {} }
    SubShader
    {
        Tags { "Queue"="Background+5" "RenderType"="Background" }
        Pass
        {
            Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            struct A { float4 pos:POSITION; };
            struct V { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
            V vert(A i){V o; o.pos=UnityObjectToClipPos(i.pos); o.dir=i.pos.xyz; return o;}
            fixed4 frag(V i):SV_Target {float3 dir=normalize(i.dir);float2 uv=float2(atan2(dir.z,dir.x)/6.2831853+.5,asin(dir.y)/3.14159265+.5);float3 b=tex2D(_MainTex,uv).rgb;return float4(b*.85+float3(.007,.012,.028),1);}
            ENDCG
        }
    }
}

Shader "ArkSpace/OrionApertureMatte" {
 Properties {_MainTex ("Existing M42 angular observation", 2D) = "black" {}}
 SubShader {
  Tags { "RenderType"="Transparent" "Queue"="Transparent" } Cull Off ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct input { float4 p:POSITION; float2 uv:TEXCOORD0; };
   struct vary {float4 p:SV_POSITION; float2 uv:TEXCOORD0;};
   sampler2D _MainTex;
   vary vert(input v){vary o;o.p=UnityObjectToClipPos(v.p);o.uv=v.uv;return o;}
   fixed4 frag(vary i):SV_Target{
    // Matte the recorded field edge only. Interior observation RGB is unmodified.
    float2 d=min(i.uv,1-i.uv);
    return fixed4(tex2D(_MainTex,i.uv).rgb,smoothstep(.002,.11,min(d.x,d.y)));
   }
   ENDCG
  }
 }
}

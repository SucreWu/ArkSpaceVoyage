Shader "ArkSpace/OfficialObservationPlate" {
 Properties {_MainTex ("Angular observation", 2D) = "black" {}}
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" } Cull Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct input { float4 p:POSITION; float2 uv:TEXCOORD0; };
   struct vary {float4 p:SV_POSITION; float2 uv:TEXCOORD0;};
   sampler2D _MainTex;
   vary vert(input v){vary o;o.p=UnityObjectToClipPos(v.p);o.uv=v.uv;return o;}
   fixed4 frag(vary i):SV_Target{return tex2D(_MainTex,i.uv);}
   ENDCG
  }
 }
}

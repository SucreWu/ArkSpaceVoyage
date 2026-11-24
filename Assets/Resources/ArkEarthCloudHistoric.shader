Shader "Ark/Space/Earth Historic Cloud Layer" {
Properties {
  _MainTex("NOAA CPC opacity",2D)="black"{}
  _Body("Globe center and radius",Vector)=(0,0,0,1)
  _Sun("Direction to sun",Vector)=(-.9,.34,-.46,0)
}
SubShader {
  Tags { "Queue"="Transparent+2" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Cull Back ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
  Pass {
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
    sampler2D _MainTex;
    float4 _Body,_Sun;
    struct a { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
    struct v { float4 pos:SV_POSITION;float3 w:TEXCOORD0;float2 uv:TEXCOORD1; };
    v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.w=mul(unity_ObjectToWorld,i.vertex).xyz;o.uv=i.uv;return o;}
    fixed4 frag(v i):SV_Target {
      float3 n=normalize(i.w-_Body.xyz);
      float3 view=normalize(_WorldSpaceCameraPos-i.w);
      float3 off=_WorldSpaceCameraPos-_Body.xyz;
      float3 tangent=off-view*dot(off,view);
      clip(_Body.w-length(tangent));
      float ndl=dot(n,normalize(_Sun.xyz));
      float light=smoothstep(.035,.72,ndl);
      float darkMask=smoothstep(.035,.21,ndl);
      float edge=smoothstep(.02,.16,dot(n,view));
      float c=tex2D(_MainTex,i.uv).r;
      float alpha=.58*smoothstep(.21,.895,c)*darkMask*edge;
      float3 cloud=lerp(float3(.29,.34,.42),float3(.82,.85,.88),light);
      return fixed4(cloud*alpha,alpha);
    }
    ENDCG
  }
}
Fallback Off
}

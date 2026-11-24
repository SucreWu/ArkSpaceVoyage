Shader "Ark/Space/Earth Thin Limb" {
Properties {
  _Body ("Earth center and radius", Vector)=(0,0,0,1)
  _Sun ("Sun direction", Vector)=(-.9,.34,-.46,0)
}
SubShader {
  Tags { "Queue"="Transparent+3" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Cull Back ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
  Pass {
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
    float4 _Body, _Sun;
    struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; };
    v2f vert(appdata_base v){v2f o; o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
    fixed4 frag(v2f i):SV_Target {
      float3 view=normalize(i.world-_WorldSpaceCameraPos);
      float3 fromCenter=_WorldSpaceCameraPos-_Body.xyz;
      float3 tangent=fromCenter-view*dot(fromCenter,view);
      float r=length(tangent)/_Body.w;
      float h=(r-1)/.018;
      clip(h); clip(1-h);
      float sun=clamp(dot(normalize(tangent),normalize(_Sun.xyz)),0,1);
      float light=smoothstep(.025,.66,sun);
      float profile=pow(1-saturate(h),1.75)*smoothstep(0,.10,h);
      float alpha=.25*light*profile;
      return fixed4(float3(.30,.55,.88)*alpha,alpha);
    }
    ENDCG
  }
}
Fallback Off
}

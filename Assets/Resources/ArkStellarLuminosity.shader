Shader "Ark/Stars/Intrinsic luminosity" {
Properties {
 _Color("Illustrative stellar tint", Color)=(.84,.91,1,1)
 _Radiance("Artistic image level",Range(0,1))=.92
}
SubShader {
 Tags {"Queue"="Geometry" "RenderType"="Opaque"}
 Cull Back ZWrite On
 Pass{
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color;float _Radiance;
 struct v {float4 pos:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;};
 v vert(appdata_base a){v o;o.pos=UnityObjectToClipPos(a.vertex);o.w=mul(unity_ObjectToWorld,a.vertex).xyz;o.n=UnityObjectToWorldNormal(a.normal);return o;}
 float h(float3 p){return frac(sin(dot(p,float3(12.9898,78.233,51.327)))*43758.5453);}
 float grain(float3 p){
   float3 i=floor(p),u=frac(p);u=u*u*(3-2*u);
   float a=lerp(lerp(h(i),h(i+float3(1,0,0)),u.x),lerp(h(i+float3(0,1,0)),h(i+float3(1,1,0)),u.x),u.y);
   float b=lerp(lerp(h(i+float3(0,0,1)),h(i+float3(1,0,1)),u.x),lerp(h(i+float3(0,1,1)),h(i+float3(1,1,1)),u.x),u.y);
   return lerp(a,b,u.z);
 }
 fixed4 frag(v i):SV_Target {
    float3 n=normalize(i.n);
    float mu=saturate(dot(n,normalize(_WorldSpaceCameraPos-i.w)));
    // Direction-independent radiance with mild optical limb darkening,
    // not a phase shadow illuminated by the scene sun.
    float limb=lerp(.70,1,pow(mu,.70));
    float fleck=.94+.027*(grain(n*58)-.5)+.036*(grain(n*9)-.5);
    return fixed4(min(_Color.rgb*_Radiance*limb*fleck,.985),1);
 }
 ENDCG
 }
}
FallBack Off
}

Shader "Ark/Space/TranslucentLamellarIce" {
Properties { _Sun("Direction to sun",Vector)=(-.9,.34,-.46,0) _FrostCoat("Near frost coat",Range(0,1))=0 }
SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Back ZWrite Off Blend One OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Sun; float _FrostCoat;
 struct o {float4 pos:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float3 local:TEXCOORD2;};
 o vert(appdata_base v){o x;x.pos=UnityObjectToClipPos(v.vertex);x.w=mul(unity_ObjectToWorld,v.vertex).xyz;x.local=v.vertex.xyz;x.n=UnityObjectToWorldNormal(v.normal);return x;}
 float h(float3 p){return frac(sin(dot(p,float3(13.17,51.79,121.83)))*42451.17);}
 float noise(float3 p){
   float3 k=floor(p),f=frac(p);f=f*f*(3-2*f);
   return lerp(lerp(lerp(h(k),h(k+float3(1,0,0)),f.x),lerp(h(k+float3(0,1,0)),h(k+float3(1,1,0)),f.x),f.y),
     lerp(lerp(h(k+float3(0,0,1)),h(k+float3(1,0,1)),f.x),lerp(h(k+float3(0,1,1)),h(k+float3(1,1,1)),f.x),f.y),f.z);
 }
 fixed4 frag(o i):SV_Target {
  float3 n=normalize(i.n),v=normalize(_WorldSpaceCameraPos-i.w),l=normalize(_Sun.xyz),p=i.local;
  // Transmitted light carries the darker blue body; narrow lamellae and
  // weak broad bulk scatter read separately from a glass-like Fresnel edge.
  float a=noise(p*8.5),fine=noise(p*39),patch=noise(p*17);
  float phase=dot(p,float3(9,27,5.4))+4.6*a+1.5*patch;
  float thin=abs(sin(phase));
  float cracks=(1-smoothstep(.07,.15+fwidth(thin),thin))*smoothstep(.29,.69,a)*(.64+.36*fine);
  cracks*=1-saturate(fwidth(phase)*.31);
  float scatter=smoothstep(.37,.83,a*.72+fine*.30);
  float rim=pow(1-saturate(dot(v,n)),2.5);
  float nl=dot(n,l), lit=saturate(nl), rear=saturate(-nl);
  float spec=pow(saturate(dot(n,normalize(v+l))),lerp(45,16,scatter));
  float alpha=clamp(.19+.21*rim+.29*scatter+.23*cracks+.10*spec,.16,.72);
  float3 tint=lerp(float3(.076,.151,.206),float3(.36,.55,.63),scatter*(.54+.46*lit));
  tint+=float3(.17,.28,.31)*rim*(.48+.52*lit);
  tint+=float3(.52,.62,.65)*cracks*(.30+.70*lit);
  tint+=float3(.56,.38,.21)*(.33*spec+.29*rear*scatter);
  // Irregular frost response only on the large foreground pieces. A coating
  // scatters light diffusely and broadens rather than erases solid ice glints.
  float coat=_FrostCoat*smoothstep(.35,.66,.48*a+.42*patch+.10*fine)
      *(1-.70*rim);
  float frostLobe=pow(saturate(dot(n,normalize(v+l))),7);
  tint=lerp(tint,float3(.49,.51,.52)*(.58+.42*lit)+float3(.08,.07,.06)*frostLobe,coat*.91);
  alpha=min(alpha+.16*coat,.76);
  return fixed4(tint*alpha,alpha);
 }
 ENDCG }
}
Fallback Off
}

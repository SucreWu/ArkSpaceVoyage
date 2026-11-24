Shader "Ark/Lunar Regolith Fine Detail" {
Properties {
 _MainTex("Existing lunar material",2D)="white"{}
 _Color("Existing lunar tint",Color)=(1,1,1,1)
 _TrialStrength("Regolith strength",Range(0,1))=1
}
SubShader {
 Tags {"RenderType"="Opaque" "Queue"="Geometry"}
 LOD 270
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex; half4 _Color; half _TrialStrength;
 struct Input {float2 uv_MainTex;float3 worldPos;};
 float h(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float dust(float2 p){
  float2 i=floor(p),f=frac(p);float2 u=f*f*(3-2*f);
  return lerp(lerp(h(i),h(i+float2(1,0)),u.x),lerp(h(i+float2(0,1)),h(i+1),u.x),u.y);
 }
 float filtered(float2 p){
   float cutoff=max(length(ddx(p)),length(ddy(p)));
   return (dust(p)-.5)*(1-smoothstep(.24,.95,cutoff));
 }
 void surf(Input i,inout SurfaceOutputStandard o){
   float2 p=i.worldPos.xz;
   float2 rotated=float2(.8*p.x-.6*p.y,.6*p.x+.8*p.y);
   float distribution=.17*filtered(rotated*1.9)+.35*filtered(p*5.4)+.30*filtered(rotated*13.7);
   float compact=filtered(p*1.1);
   fixed3 base=tex2D(_MainTex,i.uv_MainTex).rgb*_Color.rgb;
   o.Albedo=base*(1+_TrialStrength*distribution);
   o.Smoothness=lerp(.18,.11+compact*.034,_TrialStrength);
   o.Metallic=0;o.Occlusion=1;o.Alpha=1;
 }
 ENDCG
}
Fallback "Standard"
}

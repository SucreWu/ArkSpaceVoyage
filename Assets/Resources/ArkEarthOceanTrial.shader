Shader "Ark/Space/Earth Ocean Trial" {
Properties {
  _MainTex("NASA Blue Marble original",2D)="white"{}
  _OceanMask("Conservative open ocean",2D)="black"{}
  _BaseGloss("Existing surface gloss",Range(0,1))=.19
}
SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  LOD 300
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex,_OceanMask;
  half _BaseGloss;
  struct Input {float2 uv_MainTex;};
  void surf(Input i,inout SurfaceOutputStandard o) {
    fixed3 old=tex2D(_MainTex,i.uv_MainTex).rgb;
    half water=tex2D(_OceanMask,i.uv_MainTex).r;
    // Compress exaggerated seabed contrast; modest dielectric water lobe
    // with no fake streaks, emission, normal noise or additional light.
    o.Albedo=lerp(old,half3(.066,.139,.253),water*.49);
    o.Smoothness=lerp(_BaseGloss,.28,water);
    o.Metallic=0; o.Occlusion=1; o.Alpha=1;
  }
  ENDCG
}
FallBack "Standard"
}

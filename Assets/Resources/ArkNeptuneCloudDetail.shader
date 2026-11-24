Shader "Ark/Space/Neptune Cloud Detail" {
 Properties {
  _MainTex("Existing Neptune Atlas",2D)="white"{}
  _Color("Existing Tint",Color)=(1,1,1,1)
  _Metallic("Existing Metallic",Range(0,1))=0
  _Glossiness("Existing Gloss",Range(0,1))=.14
  _EmissionColor("Existing Emission",Color)=(.12,.12,.12,1)
 }
 SubShader {
  Tags {"RenderType"="Opaque" "Queue"="Geometry"}
  LOD 300
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows vertex:vtx
  #pragma target 3.0
  sampler2D _MainTex;
  half4 _Color,_EmissionColor;
  half _Metallic,_Glossiness;
  struct Input{float2 uv_MainTex;float3 localPos;};
  void vtx(inout appdata_full v,out Input i) {
    UNITY_INITIALIZE_OUTPUT(Input,i);i.localPos=v.vertex.xyz;
  }
  float H(float3 p){
    p=frac(p*.1031);p+=dot(p,p.yzx+33.33);
    return frac((p.x+p.y)*p.z);
  }
  float V(float3 p){
    float3 k=floor(p),t=frac(p);t=t*t*(3-2*t);
    return lerp(lerp(lerp(H(k),H(k+float3(1,0,0)),t.x),lerp(H(k+float3(0,1,0)),H(k+float3(1,1,0)),t.x),t.y),
     lerp(lerp(H(k+float3(0,0,1)),H(k+float3(1,0,1)),t.x),lerp(H(k+float3(0,1,1)),H(k+float3(1,1,1)),t.x),t.y),t.z);
  }
  void surf(Input i,inout SurfaceOutputStandard o){
    float3 p=normalize(i.localPos);
    // Latitude-elongated stochastic detail, not a striping/radial pattern or animation.
    float3 q=float3(p.x*18,(p.y+.08*p.x*p.z+.034*p.z)*85,p.z*18)+17;
    float width=max(length(ddx(q)),length(ddy(q)));
    float aa=1-smoothstep(.24,.82,width);
    float u=frac(i.uv_MainTex.x-.79+.5)-.5;
    float d=length(float2(u/.196,(i.uv_MainTex.y-.55)/.11));
    float aroundStorm=smoothstep(.80,1.20,d);
    half body=1+.145*(V(q)-.5)*aa*aroundStorm*(1-smoothstep(.88,.995,abs(p.y)));
    o.Albedo=tex2D(_MainTex,i.uv_MainTex).rgb*_Color.rgb*body;
    o.Metallic=_Metallic;o.Smoothness=_Glossiness;
    o.Emission=0;o.Occlusion=1;o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Standard"
}

Shader "Ark/Proxima/Hypothetical neutral rock" {
Properties {
 _MainTex("Existing illustrative terrain source",2D)="white"{}
 _Color("Artistic muted base",Color)=(.78,.76,.73,1)
}
SubShader {
 Tags {"RenderType"="Opaque" "Queue"="Geometry"}
 LOD 270
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex; half4 _Color;
 struct Input {float2 uv_MainTex;};
 void surf(Input i,inout SurfaceOutputStandard o){
   half3 raw=tex2D(_MainTex,i.uv_MainTex).rgb;
   half value=dot(raw,half3(.299,.587,.114));
   // Retain crater luminance and geography of the artwork; no data inference.
   o.Albedo=lerp(value.xxx,raw,.14)*_Color.rgb;
   o.Smoothness=.14;o.Metallic=0;o.Occlusion=1;o.Alpha=1;
 }
 ENDCG
}
Fallback "Standard"
}

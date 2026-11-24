Shader "Ark/Space/RingIce" {
Properties { _LightDir("Direction toward sunlight",Vector)=(-0.7,0.55,0.42,0) }
SubShader {
 Tags {"RenderType"="Opaque" "Queue"="Geometry"} Cull Back ZWrite On
 Pass { CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _LightDir;
 struct Input { float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float2 uv:TEXCOORD0; };
 struct Output { float4 pos:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float3 tint:TEXCOORD2;float2 uv:TEXCOORD3; };
 Output vert(Input i){ Output o;o.pos=UnityObjectToClipPos(i.vertex);o.w=mul(unity_ObjectToWorld,i.vertex).xyz;o.n=UnityObjectToWorldNormal(i.normal);o.tint=i.color.rgb;o.uv=i.uv;return o; }
 float hash31(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float noise(float3 p){
   float3 i=floor(p),u=frac(p);u=u*u*(3-2*u);
   return lerp(lerp(lerp(hash31(i),hash31(i+float3(1,0,0)),u.x),lerp(hash31(i+float3(0,1,0)),hash31(i+float3(1,1,0)),u.x),u.y),
   lerp(lerp(hash31(i+float3(0,0,1)),hash31(i+float3(1,0,1)),u.x),lerp(hash31(i+float3(0,1,1)),hash31(i+float3(1,1,1)),u.x),u.y),u.z);
 }
 fixed4 frag(Output i):SV_Target{
   float3 n=normalize(i.n),v=normalize(_WorldSpaceCameraPos.xyz-i.w),l=normalize(_LightDir.xyz);
   float facing=abs(dot(n,v)), nl=dot(n,l), fresnel=pow(1-saturate(facing),4.1);
   float coarse=noise(i.w*7.3), fine=noise(i.w*66);
   float stratum=sin(dot(i.w,float3(3.9,33,9.1))+7*coarse+i.tint.x*7.0);
   float cleft=smoothstep(.91,.997,stratum)*smoothstep(.46,.72,coarse);
   float rim=min(i.uv.x,min(i.uv.y,1-i.uv.x-i.uv.y));
   float edge=(1-smoothstep(.006,.053,rim))*smoothstep(.53,.80,fine);
   float trapped=exp(-lerp(.9,3.0,i.tint.z)*(.23+facing))*(.53+.47*coarse);
   float transmitted=(.17+.83*saturate(-nl))*trapped;
   float frost=smoothstep(.52,.92,.62*coarse+.32*fine+.20*i.tint.y);
   float3 slate=lerp(float3(.023,.044,.068),float3(.135,.252,.344),trapped);
   float3 cold=float3(.26,.47,.64),sun=float3(1,.76,.47);
   float spec=pow(saturate(dot(n,normalize(v+l))),lerp(112,22,frost));
   float3 lit=slate+cold*(.35*fresnel+.21*frost+.15*cleft)+sun*(.78*spec+.43*saturate(nl)*frost+.30*transmitted+.27*edge*saturate(nl+.25));
   return fixed4(lit,1);
 }
 ENDCG }
}
Fallback Off
}

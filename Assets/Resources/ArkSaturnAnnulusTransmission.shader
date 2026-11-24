Shader "Ark/Saturn/Annulus transmission" {
Properties {
 _MainTex("Existing annular color and density",2D)="white"{}
 _Color("Existing color family",Color)=(1,.94,.85,.93)
 _Sun("Inherited light",Vector)=(-.9,.34,-.46,0)
}
SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Back ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
 Pass{
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float4 _Color,_Sun;
 struct a {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
 struct v {float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 w:TEXCOORD1;float2 uv:TEXCOORD2;};
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.n=UnityObjectToWorldNormal(i.normal);o.w=mul(unity_ObjectToWorld,i.vertex).xyz;o.uv=i.uv;return o;}
 fixed4 frag(v i):SV_Target {
    fixed4 band=tex2D(_MainTex,i.uv);
    float3 n=normalize(i.n);
    float mu=abs(dot(n,normalize(_WorldSpaceCameraPos-i.w)));
    float sunlight=abs(dot(n,normalize(_Sun.xyz)));
    // Band alpha is an artistic optical-depth proxy, not a measured quantity.
    // Bounded slant path prevents an edge-on slab from becoming opaque.
    float tau=.59*pow(saturate(band.a),1.48);
    float alpha=(1-exp(-tau/max(.47,mu)))*_Color.a;
    float3 reflected=saturate(band.rgb*_Color.rgb*(1.38+.34*sunlight));
    return fixed4(reflected*alpha,alpha);
 }
 ENDCG
 }
}
Fallback Off
}

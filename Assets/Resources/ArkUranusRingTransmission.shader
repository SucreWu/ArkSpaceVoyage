Shader "Ark/Uranus/Gossamer ring transmission" {
Properties {
  _Color("Artistic particle color/opacity",Color)=(.48,.47,.46,.47)
  _Sun("Directional sun",Vector)=(-.9,.34,-.46,0)
}
SubShader {
  Tags {"RenderType"="Transparent" "Queue"="Transparent"}
  Cull Back ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
  Pass{
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
    float4 _Color, _Sun;
    struct v { float4 pos:SV_POSITION; float3 n:TEXCOORD0; };
    v vert(appdata_base i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.n=UnityObjectToWorldNormal(i.normal);return o;}
    fixed4 frag(v i):SV_Target{
        float sun=abs(dot(normalize(i.n),normalize(_Sun.xyz)));
        float alpha=saturate(_Color.a);
        return fixed4(_Color.rgb*(.90+.34*sun)*alpha,alpha);
    }
    ENDCG
  }
}
Fallback Off
}

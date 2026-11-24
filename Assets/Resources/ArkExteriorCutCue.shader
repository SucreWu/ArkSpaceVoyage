Shader "Hidden/Ark/Brief exterior cut" {
Properties { _MainTex ("Scene",2D)="white"{} _Strength("Cue",Range(0,1))=0 }
SubShader {
 Cull Off ZWrite Off ZTest Always
 Pass {
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float _Strength;
 fixed4 frag(v2f_img i):SV_Target{
    fixed4 scene=tex2D(_MainTex,i.uv);
    float2 xy=(i.uv-.5)*2;
    float edge=smoothstep(.16,1.46,dot(xy,xy));
    float veil=_Strength*(.73+.18*edge);
    return fixed4(lerp(scene.rgb,fixed3(.010,.015,.026),veil),scene.a);
 }
 ENDCG
 }
}
Fallback Off
}

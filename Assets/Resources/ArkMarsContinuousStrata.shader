Shader "Ark/Mars/Continuous canyon strata" {
Properties { _Grain("Microsurface grain strength",Range(0,1))=1 _WallProjection("Wall grain orientation",Range(0,1))=1 }
SubShader {
 Tags {"RenderType"="Opaque" "Queue"="Geometry"}
 LOD 270
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 half _Grain,_WallProjection;
 struct Input {float3 worldPos;float3 worldNormal;};
 float h(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float field(float2 p){
    float2 k=floor(p), f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(h(k),h(k+float2(1,0)),f.x),lerp(h(k+float2(0,1)),h(k+1),f.x),f.y);
 }
 float grain(float2 p) {
    float footprint=max(length(ddx(p)),length(ddy(p)));
    return (field(p)-.5)*(1-smoothstep(.18,.86,footprint));
 }
 float textured(float2 p){
    return .225*grain(p*3.7)+.20*grain((p.yx+float2(8,19))*9.1);
 }
 void surf(Input i,inout SurfaceOutputStandard o) {
    float2 xz=i.worldPos.xz;
    float tall=smoothstep(-6.7,-1.7,i.worldPos.y);
    float steep=smoothstep(.08,.66,1-saturate(normalize(i.worldNormal).y));
    float exposed=saturate(.68*tall+.44*steep);
    float warp=.60*(field(xz*.043)-.5)+.16*sin(xz.y*.085+xz.x*.054);
    float bed=i.worldPos.y+warp;
    float strata=.66*sin(bed*1.63)+.24*sin(bed*3.04+.45)+.10*sin(bed*.70-1);
    float wallTone=saturate(.53+.20*strata+.14*(field(xz*.092)-.5));
    float3 dust=float3(.755,.475,.305)*( .980+.052*field(xz*.084) );
    float3 wall=lerp(float3(.624,.370,.253),float3(.854,.583,.381),wallTone);
    // Map across the face as well as the floor, and let distant grains integrate out.
    float2 grains=float2(dot(xz,float2(.83,-.56)),dot(xz,float2(.56,.83)))
       +i.worldPos.y*float2(.53,-.67)*(.26+.74*steep);
    float3 n=normalize(i.worldNormal);
    float blend=_WallProjection*smoothstep(.08,.43,1-abs(n.y));
    float2 side=n.xz*n.xz;
    side=side*side/max(1e-7,dot(side,side));
    // Blend the sampled values, not discontinuous coordinates, across wall planes.
    float wallGrain=side.x*textured(i.worldPos.zy+float2(37,21))
                   +side.y*textured(i.worldPos.xy+float2(11,-9));
    float spread=lerp(textured(grains),wallGrain,blend)*(.87+.13*exposed);
    o.Albedo=lerp(dust,wall,exposed)*(1+_Grain*spread);
    o.Smoothness=.13-_Grain*(.035+.012*steep)*(1-.2*tall);
    o.Metallic=0;o.Occlusion=1;o.Alpha=1;
 }
 ENDCG
}
Fallback "Standard"
}

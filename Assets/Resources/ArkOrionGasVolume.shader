Shader "Ark/Space/M42 authored gas volume" {
 Properties {_Noise("Procedural density seed",3D)="white"{} _Filament("Outer cloud filament formation",Range(0,1))=1}
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Front ZTest Always ZWrite Off Blend One OneMinusSrcAlpha
 Pass {
 CGPROGRAM
 #pragma target 4.0
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler3D _Noise; float _Filament;
 struct V {float4 clip:SV_POSITION;float3 local:TEXCOORD0;};
 V vert(appdata_base v){V o;o.clip=UnityObjectToClipPos(v.vertex);o.local=v.vertex.xyz;return o;}
 float N(float3 p){return tex3Dlod(_Noise,float4(p,0)).r;}
 float Body(float3 p,float3 c,float3 r){float3 q=(p-c)/r;return saturate(1-dot(q,q));}
 float Rim(float3 p,float3 c,float3 r,float broad){
   float3 q=(p-c)/r;float d=abs(dot(q,q)-1);
   return 1-smoothstep(.07,broad,d);
 }
 float2 Star(float3 ro,float3 rd,float3 pos,float size){
   float3 a=pos-ro;float t=dot(a,rd);float3 off=a-rd*t;float k=dot(off,off)/(size*size);
   return float2(t,exp2(-k*1.8)+.19*exp2(-k*.22));
 }
 float4 frag(V i):SV_Target{
   float3 ro=mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz,rd=normalize(i.local-ro);
   float3 safe=sign(rd+1e-8)*max(abs(rd),1e-5);
   float3 m=(-.5-ro)/safe,n=(.5-ro)/safe, lo=min(m,n),hi=max(m,n);
   float first=max(0,max(lo.x,max(lo.y,lo.z))),last=min(hi.x,min(hi.y,hi.z));
   if(last<=first)discard;
   const int count=28;
   float stepSize=(last-first)/count;
   float2 s0=Star(ro,rd,float3(-.054,.025,.040),.0021);
   float2 s1=Star(ro,rd,float3(-.041,.042,.067),.0015);
   float2 s2=Star(ro,rd,float3(-.064,.055,.057),.00155);
   float2 s3=Star(ro,rd,float3(-.071,.023,.081),.00140);
   float2 s4=Star(ro,rd,float3(-.204,.242,.111),.0018);
   float3 sum=0;float trans=1;
   [loop] for(int j=0;j<count;j++){
     float t=first+(j+.5)*stepSize;float3 p=ro+rd*t;
     float w=N(p*.77+float3(.11,.37,.63)), grit=N(p*3.2+float3(.41,.23,.17));
     float ridge=1-abs(2*w-1);
     float f=smoothstep(.07,.84,.65*w+.35*grit);
     float3 q=p+float3(.128*(w-.5),.094*(grit-.5),.13*(w-.5));
     // Broken warm wall enclosing a hollow bay, lit from behind the foreground dust.
     float fan=Body(q,float3(-.035,.02,.054),float3(.417,.394,.349));
     float cavity=Body(q,float3(-.03,.04,-.06),float3(.20,.18,.21));
     float face=fan*(1-.97*cavity)*(.32+.60*f)*smoothstep(.12,.78,.69*w+.33*grit);
     float3 arcP=q;arcP.x+=.26*q.y+.82*q.y*q.y;
     float arc=Rim(arcP,float3(.079,-.023,.157),float3(.342,.260,.147),.18)
       *smoothstep(-.21,.29,arcP.x-.47*arcP.y)*(.10+.86*grit)*smoothstep(.25,.74,w);
     // Split only the far outer wall into two torn, spatially separated ridges,
     // with gaps between lobes. Preserve the hot cavity and foreground veil.
     float3 farP=(arcP-float3(.079,-.023,.157))/float3(.342,.260,.147);
     float3 forkP=(arcP-float3(.124,.019,.204))/float3(.294,.219,.111);
     float aR=dot(farP,farP), bR=dot(forkP,forkP);
     float firstRidge=(1-smoothstep(.031,.174,abs(aR-1+.12*(w-.5))));
     float fork=(1-smoothstep(.018,.148,abs(bR-1+.10*(grit-.5))));
     float mask=smoothstep(-.018,.22,arcP.x-.59*arcP.y);
     float breakUp=smoothstep(.26,.79,.70*w+.30*grit);
     float farFilament=mask*(firstRidge*(.22+.61*breakUp)
          +.46*fork*smoothstep(.26,.57,w)*smoothstep(-.17,.11,arcP.y))
          *smoothstep(.015,.137,arcP.z);
     arc=lerp(arc,farFilament,_Filament);
     float knot=Body(q,float3(-.249,.265,.106),float3(.081,.086,.147))*(.30+1.19*f);
     float core=Body(q,float3(-.050,.032,.072),float3(.156,.115,.157));
     float coreGlow=core*core*(.58+.72*f);
     float wing=Body(q,float3(.14,.17,.22),float3(.31,.149,.162))*(.14+.56*f);
     float veil=Body(q,float3(-.245,-.16,-.19),float3(.224,.294,.162))
       *smoothstep(.24,.77,.56*w+.44*grit);
     float3 lane=q;lane.x+=.415*lane.y+.083*lane.z;
     float notch=Body(lane,float3(-.16,.119,-.163),float3(.073,.233,.141));
     float finger=Body(lane,float3(-.077,-.16,-.145),float3(.064,.183,.122));
     float dust=(notch*.94+finger*.50)*(.19+.98*f);
     float ribbonA=q.y-(.53*q.x+.51*q.x*q.x+.012+.060*(w-.5));
     float ribbonB=q.x-(-.18+.30*q.y+.84*q.y*q.y-.069*(w-.5));
     float rib=Body(q,float3(-.10,.012,.039),float3(.410,.33,.205))*exp2(-abs(ribbonA)*27)*smoothstep(.24,.80,grit);
     float hook=Body(q,float3(-.265,-.142,-.012),float3(.238,.314,.19))*exp2(-abs(ribbonB)*35)*smoothstep(.15,.73,grit);
     float edge=1-smoothstep(.400,.495,max(abs(p.x),max(abs(p.y),abs(p.z))));
     float density=(face*.83+arc*1.34+knot*1.29+coreGlow*.91+veil*.52+wing*.33+dust*2.58+rib*.86+hook*.72)*edge;
     float3 emission=
        face*lerp(float3(.205,.045,.062),float3(.54,.142,.195),ridge)*(.55+.65*f)
        +arc*float3(.97,.267,.101)*.8
        +knot*float3(.90,.407,.76)
        +coreGlow*float3(2.31,1.51,1.06)
        +wing*float3(.15,.315,.338)
        +veil*float3(.11,.32,.36)
        +dust*float3(.053,.025,.032)+rib*float3(.97,.355,.54)+hook*float3(.68,.21,.135);
     float alpha=1-exp2(-5.65*density*stepSize);
     sum+=trans*alpha*saturate(emission/max(.13,density));
     float halfStep=stepSize*.5;
     float4 crosses=step(abs(float4(s0.x,s1.x,s2.x,s3.x)-t),halfStep);
     float coreStars=dot(crosses,float4(s0.y,s1.y*.66,s2.y*.82,s3.y*.47));
     sum+=trans*(coreStars*float3(1.20,1.12,1.02)+step(abs(s4.x-t),halfStep)*s4.y*float3(.68,.90,1.15));
     trans*=1-alpha;
     if(trans<.021)break;
   }
   return float4(sum,1-trans);
 }
 ENDCG
 }
 }
 Fallback Off
}

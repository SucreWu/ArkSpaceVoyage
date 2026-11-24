using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk
{
    internal static class DistrictDock
    {
        internal static void Build(DistrictSet d)
        {
            var red=d.Mat("编号漆","A93F32",null,.22f,.46f); var shell=d.Mat("搪瓷船壳","E1E2D8","hull",.27f,.51f); DistrictRooms.Shell(d,"dock",d.Gold);
            // 尾门坡道由单个斜面支撑，踏纹仅供显示。
            float tilt=-Mathf.Atan2(.37f,2.65f)*Mathf.Rad2Deg;
            d.Box(V(0,.124f,.47f),V(2.19f,.12f,2.68f),d.Steel,true,V(tilt,0,0));
            for(int n=0;n<13;n++){float z=-.81f+n*.212f; d.Box(V(0,.017f+n*.0296f,z),V(2.05f,.018f,.045f),d.Dark,false,V(tilt,0,0));}
            d.Box(V(0,.175f,4.58f),V(3.18f,.35f,5.61f),shell,true);
            for(int k=0;k<8;k++) d.Box(V(0,.354f,2.0f+.66f*k),V(2.7f,.008f,.05f),d.Dark);
            for(int s=-1;s<=1;s+=2)
            {
                for(int i=0;i<5;i++)
                {
                    float a=1.74f+i*1.05f,b=a+1.02f;
                    float x1=1.63f-.11f*Mathf.Max(0,i-1); float x2=1.63f-.11f*Mathf.Max(0,i);
                    var v0=V(s*(x1-.23f),.27f,a); var v1=V(s*(x2-.23f),.27f,b); var v2=V(s*(x2+.05f),1.20f,b); var v3=V(s*(x1+.05f),1.20f,a);
                    Panel(d,shell,v0,v1,v2,v3); d.Box(V(s*(x1-.02f),.78f,(a+b)/2),V(.31f,1.1f,1.08f),d.Panels,true,V(0,0,s*-8));
                    Panel(d,red,Vector3.Lerp(v0,v3,.67f)+V(s*.018f,0,0),Vector3.Lerp(v1,v2,.67f)+V(s*.018f,0,0),Vector3.Lerp(v1,v2,.83f)+V(s*.018f,0,0),Vector3.Lerp(v0,v3,.83f)+V(s*.018f,0,0));
                    var top0=V(s*(x1-.36f),2.51f,a+.13f);var top1=V(s*(x2-.36f),2.54f,b);
                    d.Rod(v3,top0,.061f,d.Steel); d.Rod(top0,top1,.064f,d.Dark);
                    if(i>0) Panel(d,d.Glass,v3,v2,top1,top0);
                    Panel(d,shell,top0,top1,V(s*.44f,2.67f,b),V(s*.44f,2.67f,a+.13f));
                    d.Box(V(s*(x1+.02f),.48f,(a+b)*.5f),V(.15f,.17f,.77f),d.Dark);
                }
                var side=V(s*2.02f,.89f,3.45f);
                d.Rod(side+V(0,0,-1.55f),side+V(0,0,1.52f),.54f,shell);
                d.Ball(side+V(0,0,1.48f),V(1.05f,1.03f,1.65f),shell);
                d.Rod(side+V(0,0,-1.75f),side+V(0,0,-1.48f),.47f,d.Steel); d.Rod(side+V(0,0,-1.79f),side+V(0,0,-1.76f),.37f,d.Dark); d.Rod(side+V(0,0,-1.82f),side+V(0,0,-1.80f),.22f,d.Aqua);
                for(int i=0;i<11;i++){float a=Mathf.PI*2*i/11f; d.Rod(side+V(Mathf.Cos(a)*.44f,Mathf.Sin(a)*.44f,-1.74f),side+V(Mathf.Cos(a)*.48f,Mathf.Sin(a)*.48f,-1.4f),.027f,d.Dark);}
                var wing=new[]{V(s*1.52f,.83f,3.25f),V(s*3.97f,.59f,2.05f),V(s*4.35f,.62f,2.95f),V(s*1.30f,1.05f,6.1f)}; Panel(d,shell,wing);
                Panel(d,red,Vector3.Lerp(wing[0],wing[1],.81f)+Vector3.up*.02f,wing[1]+Vector3.up*.02f,wing[2]+Vector3.up*.02f,Vector3.Lerp(wing[3],wing[2],.80f)+Vector3.up*.02f);
                Panel(d,d.Steel,wing[1],wing[2],wing[2]-Vector3.up*.19f,wing[1]-Vector3.up*.19f);
                d.Rod(V(s*1.13f,.72f,5.47f),V(s*1.58f,.12f,5.70f),.095f,d.Steel);d.Box(V(s*1.60f,.065f,5.73f),V(.57f,.125f,.79f),d.Dark);
                d.Rod(V(s*1.06f,.10f,-.11f),V(s*1.06f,.78f,1.65f),.047f,d.Gold);
                DistrictRooms.Seat(d,V(s*.88f,.35f,4.53f));
                var bench=V(s*8.6f,0,-.24f);d.Box(bench+V(0,.53f,0),V(3.20f,1.06f,1.15f),d.Gold,true); d.Box(bench+V(0,1.10f,0),V(3.53f,.14f,1.35f),d.Steel);
                for(int j=0;j<4;j++) d.Box(bench+V(0,.26f+j*.19f,-.59f),V(2.87f,.043f,.047f),d.Dark);
                for(int j=0;j<5;j++){var tool=bench+V(-1.04f+.44f*j,1.21f,-.10f+.16f*(j%2));d.Rod(tool,tool+V(.16f,0,.24f),.035f,d.Steel);d.Rod(tool,tool+V(-.045f,0,-.16f),.053f,red);}
            }
            Panel(d,d.Glass,V(-1.07f,1.37f,7.44f),V(1.07f,1.37f,7.44f),V(.86f,2.44f,6.43f),V(-.86f,2.44f,6.43f));
            Panel(d,shell,V(-1.11f,1.36f,7.45f),V(1.11f,1.36f,7.45f),V(.45f,.68f,8.78f),V(-.45f,.68f,8.78f));
            for(int s=-1;s<=1;s+=2) Panel(d,shell,V(s*1.26f,.33f,6.75f),V(s*.44f,.45f,8.82f),V(s*.45f,.68f,8.78f),V(s*1.12f,1.37f,7.45f));
            d.Box(V(0,.87f,6.85f),V(2.30f,.93f,.55f),d.Dark,true); d.Box(V(0,1.35f,6.79f),V(2.31f,.27f,.63f),d.Steel,false,V(-15,0,0));
            for(int j=0;j<3;j++) d.Box(V((j-1)*.70f,1.53f,6.70f),V(.61f,.06f,.42f),d.Console,false,V(-15,0,0));
            for(int k=0;k<5;k++){float z=-6+k*4.8f;d.Box(V(0,5.73f,z),V(20,.27f,.38f),d.Gold); for(int n=-9;n<9;n+=2){d.Rod(V(n,5.76f,z),V(n+1,6.29f,z),.052f,d.Steel);d.Rod(V(n+1,6.29f,z),V(n+2,5.76f,z),.052f,d.Steel);}}
            for(int k=0;k<12;k++) for(int s=-1;s<=1;s+=2) d.Box(V(s*3.30f,.022f,-6.2f+1.74f*k),V(.16f,.026f,.80f),d.Gold);
            var arm=d.gameObject.AddComponent<DistrictArm>(); arm.Build(d,V(-5.9f,0,3.6f));
            d.Mark(V(-5.1f,1.65f,3.54f),1.1f,"检修机械臂","看夹具夹住的零件。机械臂会把它慢慢托高，然后放回工作位置。",arm.Demonstrate);
            d.Mark(V(2.01f,.91f,1.65f),.68f,"穿梭机尾喷口","这里是发动机的喷口。两台发动机中间有一道活动坡板，沿坡板可以走进舱内。");
            d.Mark(V(0,1.45f,6.60f),.70f,"驾驶台 · 选择航线","你走进了穿梭机。现在可以选择想去的地方。",d.Site.FlightMap);
        }
        static void Panel(DistrictSet d,Material m,params Vector3[] points){d.Add(DistrictShapes.Panel(d,points),Vector3.zero,Vector3.one,m,Quaternion.identity);}
    }
}

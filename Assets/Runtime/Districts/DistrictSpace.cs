using System.Collections.Generic;
using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk
{
    internal static class DistrictSpace
    {
        internal static void Build(DistrictSet d,string id)
        {
            d.Site.Spawn=V(0,2,0);d.Site.Yaw=0;d.Site.Pitch=id=="moon"?6:-3;d.Site.Exit=V(11,4,39);
            RenderSettings.fog=false; RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight=C(id=="proxima"?"4D2636":"555965");
            var shader=Resources.Load<Shader>("Districts/DistrictSky");
            var sky=d.Mat("远星","FFFFFF","stars"); if(shader) {sky.shader=shader;sky.mainTexture=Resources.Load<Texture2D>("Districts/stars");sky.renderQueue=1005;}
            var skybox=d.Visual("三维星空穹顶",Primitive(PrimitiveType.Sphere),sky,d.transform,Vector3.zero,Vector3.one*6000,Quaternion.identity);skybox.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            d.Sun(id=="proxima"?V(.88f,-.21f,.38f):id=="mercury"?V(.7f,-.29f,-.55f):V(.9f,-.34f,.46f),C(id=="proxima"?"FFAB82":"FFF0D7"),1.14f);
            var round=Round(d);
            switch(id)
            {
                case "mercury": ArkSolarNeighbors.Mercury(d,round);break;
                case "venus": ArkSolarNeighbors.Venus(d,round);break;
                case "uranus": ArkIceGiants.Uranus(d,round);break;
                case "neptune": ArkIceGiants.Neptune(d,round);break;
                case "sirius": ArkStellarRoutes.Sirius(d,round);break;
                case "pleiades": ArkStellarRoutes.Pleiades(d,round);break;
                case "earth": ArkInnerWorlds.Earth(d);break;
                case "orion": if(!ArkOrionVolume.Build(d))ArkOrionObservation.Build(d);break;
                case "station-orbit": ArkSpaceStation.Build(d,round);return;
                case "mars": ArkInnerWorlds.Mars(d);break;
                case "moon": Moon(d,round);break;
                case "jupiter": Jupiter(d,round);break;
                case "saturn": Saturn(d,round);break;
                case "proxima": Proxima(d,round);break;
            }
        }
        internal static Mesh Round(DistrictSet d,int slices=96,int bands=48)
        {
            var v=new Vector3[(slices+1)*(bands+1)];var uv=new Vector2[v.Length];var t=new int[slices*bands*6];int n=0;
            for(int j=0;j<=bands;j++) for(int i=0;i<=slices;i++)
            {
                float u=i/(float)slices, w=j/(float)bands;float a=u*Mathf.PI*2,b=(w-.5f)*Mathf.PI;int k=j*(slices+1)+i;
                v[k]=V(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(b),Mathf.Sin(a)*Mathf.Cos(b)); uv[k]=new Vector2(u,w);
                if(i<slices&&j<bands){t[n++]=k;t[n++]=k+slices+1;t[n++]=k+1;t[n++]=k+1;t[n++]=k+slices+1;t[n++]=k+slices+2;}
            }
            var m=d.Own(new Mesh {name="航行天体球"});m.vertices=v;m.normals=v;m.uv=uv;m.triangles=t;m.RecalculateBounds();return m;
        }
        static Transform Globe(DistrictSet d,Mesh mesh,string title,string file,Vector3 p,float radius,string color="FFFFFF",float glow=0,float angle=0)
        {
            var mat=d.Mat(title,color,file,0,.14f,glow);var t=d.Visual(title,mesh,mat,d.transform,p,Vector3.one*radius,Quaternion.Euler(0,angle,0));
            t.gameObject.AddComponent<SphereCollider>().radius=1;
            if(glow>0) t.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }
        static void Beacon(DistrictSet d,Vector3 p,float a)
        {
            d.Box(p,V(.84f,.66f,.76f)*a,d.Panels,true);
            d.Rod(p+V(0,.28f*a,0),p+V(0,.91f*a,0),.033f*a,d.Steel);d.Ball(p+V(0,.96f*a,0),Vector3.one*.18f*a,d.Gold);
            var panel=d.Mat("太阳电池","254E7F",null,.40f,.62f);
            for(int s=-1;s<=1;s+=2){d.Rod(p,p+V(s*2.08f*a,0,0),.045f*a,d.Steel);for(int i=0;i<3;i++){var w=p+V(s*(.77f+i*.49f)*a,0,0);d.Box(w,V(.45f,.057f,1.27f)*a,panel); for(int n=0;n<5;n++)d.Box(w+V(0,.036f*a,(-.5f+.25f*n)*a),V(.445f,.014f,.014f)*a,d.Gold);}}
        }
        static void Moon(DistrictSet d,Mesh sphere)
        {
            Globe(d,sphere,"月轨所见地球","earth",V(-20,18,113),13,"FFFFFF",0,66);
            var m=LunarGround(d);var mat=d.Mat("月壤","ABACAA","moon",0,.18f);var deck=d.Visual("有环形坑的月壤",m,mat,d.transform,Vector3.zero,Vector3.one,Quaternion.identity);deck.gameObject.AddComponent<MeshCollider>().sharedMesh=m; ArkLunarRegolith.Apply(deck);
            var basalt=d.Mat("月岩","8A8887",null,.02f,.25f);
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-arkVerify")>=0 && System.Array.IndexOf(args,"-arkMoonAB")>=0) ArkLunarRocks.Build(d,basalt,Height);
            else {
                var basaltGrades=new[]{d.Mat("Moon dry basalt 0","5F5D5B",null,.02f,.15f),
                    d.Mat("Moon dry basalt 1","706D6A",null,.02f,.18f),
                    d.Mat("Moon dry basalt 2","797774",null,.02f,.21f)};
                if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkMoonRockToneAB")>=0)
                    d.gameObject.AddComponent<ArkMoonRockToneWitness>().Prior=basalt;
                var stones=new[]{DistrictShapes.Rock(d,0),DistrictShapes.Rock(d,1),DistrictShapes.Rock(d,2)};
                for(int i=0;i<145;i++){float x=d.Random(-50,50),z=d.Random(-10,109),r=d.Random(.22f,1.6f);d.Add(stones[i%3],V(x,Height(x,z)+r*.22f,z),V(r*1.32f,r*.68f,r),basaltGrades[(int)((unchecked((uint)i*2654435761u)>>22)%3u)],Quaternion.Euler(d.Random(-16,16),d.Random(0,360),d.Random(-11,11)));}
            }
            d.Mark(V(-19,17,100),11,"从月轨看地球","从月球附近回望，白色云层下面有蓝色海洋。地球挂在月面的地平线上方。");
            d.Mark(V(5,-5.7f,18),3.5f,"月面环形坑","看坑边亮起的那一圈。斜着照来的阳光让坑壁投下一片长长的阴影。");
        }
        static float Height(float x,float z)
        {
            float y=-7.4f+.47f*Mathf.Sin(x*.16f-z*.08f)+.34f*Mathf.Cos(z*.29f+x*.055f)+.10f*Mathf.Sin(z*.85f-x*.63f);
            y+=Crater(x-5,z-18,6);y+=Crater(x+16,z-39,10);y+=Crater(x-23,z-58,9);y+=Crater(x+7,z-88,15.5f);return y;
        }
        static float Crater(float x,float z,float r){float q=Mathf.Sqrt(x*x+z*z)/r; return -1.9f*Mathf.Exp(-Mathf.Pow(q/.78f,4))+1.55f*Mathf.Exp(-Mathf.Pow((q-1)/.18f,2));}
        static Mesh LunarGround(DistrictSet d)
        {
            const int nx=105,nz=151;var v=new Vector3[nx*nz];var uv=new Vector2[v.Length];var idx=new int[(nx-1)*(nz-1)*6];int n=0;
            for(int j=0;j<nz;j++)for(int i=0;i<nx;i++)
            {
                float x=(i-52)*1.42f,z=-40+j*1.4f;int k=i+nx*j;v[k]=V(x,Height(x,z),z);uv[k]=new Vector2(.44f+x*.0021f,.58f+z*.0019f);
                if(i<nx-1&&j<nz-1){idx[n++]=k;idx[n++]=k+nx;idx[n++]=k+1;idx[n++]=k+1;idx[n++]=k+nx;idx[n++]=k+nx+1;}
            }
            var m=d.Own(new Mesh(){name="月面环形坑"});m.vertices=v;m.uv=uv;m.triangles=idx;m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        static void Jupiter(DistrictSet d,Mesh m)
        {
            ArkJupiter.Build(d,m,"木星");Globe(d,m,"木卫一","io",V(-15,5,51),5.15f,"FFFFFF",0,81);Globe(d,m,"木卫二","europa",V(22,7.5f,69),4.15f,"FFFFFF",0,147);
            d.Mark(V(11,8,86),27,"木星的云带","看看木星表面一深一浅的条纹。你看到的是不同颜色的云层。");
            d.Mark(V(-15,5,46.1f),4.0f,"木卫一","这颗颜色像硫黄的小天体叫木卫一。它绕着后方巨大的木星运行。");
            d.Mark(V(22,7.5f,65.1f),3.2f,"木卫二","放慢飞船看一看，冰壳表面有纵横交错的暗纹。");
        }
        static void Saturn(DistrictSet d,Mesh m)
        {
            var p=V(6,10,126);
            var tilt=Quaternion.Euler(-9,3,17);
            var shape=Globe(d,m,"Saturn authored globe","saturn_sss_2k",p,26,"FFFFFF",0,0);
            shape.localRotation=tilt*Quaternion.Euler(0,56,0);
            shape.localScale=V(26,23.43f,26);
            var ring=Ring(d,31.1f,57.5f);var finish=d.Mat("Saturn rings","FFF0D8","saturn_ring",0,.18f,.54f,.93f);
            var look=finish;
            var shader=Resources.Load<Shader>("ArkSaturnAnnulusTransmission");
            if(shader&&shader.isSupported){
                look=d.Mat("Saturn annulus variable transmission","FFF0D8");
                look.shader=shader;look.mainTexture=finish.mainTexture;look.color=finish.color;
                look.renderQueue=3000;look.SetVector("_Sun",new Vector4(-.9f,.34f,-.46f,0));
            }
            var annulus=d.Visual("Distant annular ring bands",ring,look,d.transform,p,Vector3.one,tilt);
            var audit=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(audit,"-arkVerify")>=0&&System.Array.IndexOf(audit,"-arkSaturnAnnulusAB")>=0)
                annulus.gameObject.AddComponent<ArkSaturnAnnulusComparison>().Previous=finish;
            ArkWalk.Art.SaturnCrystalRibbon.Build(d.transform,V(-.9f,.34f,-.46f));
            var hero=V(5.8f,-.32f,20.3f);
            var body=d.Group("Keel shard collision");
            body.localPosition=hero;
            body.gameObject.AddComponent<SphereCollider>().radius=1.7f;
            d.Mark(p+V(0,2,-32),16,"Saturn texture credits","Solar System Scope (INOVE), CC BY 4.0. Image unmodified; relief and flight scale are illustrative.");
            d.Mark(hero,1.85f,"环里的冰块","看近处这一块冰。向旁边飞一点，就能看出它和远处同伴之间的距离。");
            d.Mark(p+V(0,0,-25),21,"土星与行星环","从远处看，它像一整圈薄薄的带子。飞近后，眼前的冰块就分开了。");
        }
        static Mesh Ring(DistrictSet d,float a,float b)
        {
            const int n=192;var v=new List<Vector3>();var uv=new List<Vector2>();var idx=new List<int>();
            for(int side=0;side<2;side++)for(int j=0;j<=n;j++)
            {
                int k=v.Count;float ang=2*Mathf.PI*j/n;var r=V(Mathf.Cos(ang),0,Mathf.Sin(ang));v.Add(r*a);v.Add(r*b);uv.Add(new Vector2(.997f,.5f));uv.Add(new Vector2(.002f,.5f));
                if(j<n){if(side==0)idx.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});else idx.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}
            }
            var m=d.Own(new Mesh());m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(idx,0);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        static void Proxima(DistrictSet d,Mesh m)
        {
            var star=V(-22,13,128);Globe(d,m,"比邻星","star",star,14,"FF955D",1.5f,61);var rock=Globe(d,m,"岩石行星示意","ProximaTerrain",V(20,5,86),15,"C99C9A",0,33);
            ArkProximaCompanionComparison.Apply(d,rock);
            d.Mark(star+V(0,0,-13),11.5f,"比邻星","从这里望过去，这颗恒星带着橘红色的光。它是太阳的近邻。");
            d.Mark(V(19,5,72),12,"比邻星旁的岩石行星","我们用想象的地表来画这颗行星。你可以绕着它，寻找亮面和暗面相遇的那条线。");
        }
    }
}

using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk
{
    internal static class DistrictRooms
    {
        internal static void Shell(DistrictSet d,string kind, Material accent)
        {
            var garden = kind=="ecology";
            var bridge = kind=="bridge";
            var workshop = kind=="dock";
            d.Site.Spawn=V(0,0,-5.3f); d.Site.Yaw=0; d.Site.Pitch=2; d.Site.Exit=V(0,0,14.3f);
            RenderSettings.fog=false;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=C(garden?"B7C9BF":"8D9DA9");
            RenderSettings.ambientEquatorColor=C("788C91"); RenderSettings.ambientGroundColor=C("555960");
            var floor = garden?d.Mat("湿石地坪","C9CCB8","PorchWetStone_BaseColor",.12f,.55f,0,1,8):d.Mat("甲板","B9BABB","deck",.26f,.43f,0,1,9);
            d.Box(V(0,-.19f,3.8f),V(24.8f,.38f,25.2f),floor,true);
            for(int k=-5;k<=5;k++) d.Box(V(k*2,.005f,3.8f),V(.017f,.008f,25.1f),d.Dark);
            for(int k=-4;k<=7;k++) d.Box(V(0,.006f,k*2),V(24.6f,.008f,.018f),d.Dark);
            for(int s=-1;s<=1;s+=2)
            {
                float x=s*12.2f;
                d.Box(V(x,1,3.8f),V(.28f,2,25.2f),d.Panels,true);
                d.Box(V(x,4.55f,3.8f),V(.4f,.42f,25.2f),accent);
                d.Box(V(s*7.14f,2.3f,16.15f),V(10.3f,4.6f,.25f),bridge?d.Dark:d.Hull,true);
                d.Box(V(s*1.92f,1.73f,16.12f),V(.20f,3.46f,.60f),d.Steel);
                d.Box(V(s*1.80f,1.70f,15.82f),V(.07f,3,.06f),d.Aqua);
                for(int k=0;k<7;k++)
                {
                    float z=-8.2f+4.0f*k;
                    d.Box(V(x,3.25f,z),V(.36f,3.4f,.2f),d.Steel);
                    d.Rod(V(x,4.48f,z),V(s*9.4f,6.30f,z),.105f,d.Steel);
                    if(k<6 && !garden) d.Box(V(x,3.0f,z+2),V(.16f,2.15f,3.82f),bridge?d.Glass:d.Hull);
                }
                for(int k=0;k<3;k++)
                {
                    var w=V(s*(3.5f+3.0f*k),3.38f,15.90f);
                    d.Box(w,V(2.63f,1.84f,.15f),d.Dark);
                    d.Box(w+V(0,0,-.09f),V(2.37f,1.57f,.055f),bridge?d.Mat("深空窗","4C6F93","stars",0,.15f,.55f):d.Mat("侧舷窗","51747F",null,.31f,.59f));
                    d.Box(w+V(0,0,-.14f),V(.045f,1.58f,.055f),d.Steel);
                }
                d.Rod(V(x-s*.2f,1.48f,-8),V(x-s*.2f,1.48f,15.9f),.051f,d.Gold);
                d.Box(V(s*10.9f,.019f,3.8f),V(.057f,.019f,24.2f),accent);
            }
            d.Box(V(0,3.53f,16.10f),V(4,.27f,.63f),d.Steel);
            d.Box(V(0,1.5f,16.52f),V(3.9f,3,.15f),d.Dark,true);
            d.Box(V(0,2.6f,-8.72f),V(24.8f,5.2f,.32f),d.Hull,true);
            if(!garden) d.Box(V(0,6.61f,3.8f),V(24.7f,.17f,25.3f),workshop||bridge?d.Dark:d.Hull);
            for(int k=0;k<4;k++)
            {
                float z=-6.4f+k*6.65f;
                d.Box(V(0,6.37f,z),V(20.4f,.22f,.26f),d.Steel);
                for(int s=-1;s<=1;s+=2) d.Box(V(6.0f*s,6.22f,z),V(4.1f,.095f,.16f),d.Light);
                if(garden) for(int s=-1;s<=1;s+=2) d.Box(V(s*9.4f,5.76f,z),V(4.5f,.13f,6.0f),d.Glass);
            }
            d.Sun(V(.7f,-1,.54f),C(garden?"FFE7B1":"F9E0C4"),garden?1.45f:.93f, true);
            d.Lamp(V(-4,3.1f,2.5f),C("FFDAAD"),2.6f,13); d.Lamp(V(4,3.4f,7),C("ADEAF2"),2.7f,14);
        }
        static void Tree(DistrictSet d,Vector3 p,float h,float yaw)
        {
            var parent=d.Group("河岸乔木"); parent.localPosition=p; parent.localRotation=Quaternion.Euler(0,yaw,0); parent.localScale=Vector3.one*(h/6.1f);
            d.Model("OakWood",d.Mat("树皮","C1A68F","park_bark"),parent,Vector3.zero,Vector3.one);
            d.Model("OakLeaves",d.Mat("乔木叶簇","B4D5A2","leaves"),parent,Vector3.zero,Vector3.one);
            d.Block(p+Vector3.up*(h*.25f),V(.46f,h*.50f,.48f),Quaternion.identity);
            for(int k=0;k<8;k++)
            {
                float a=k*2.39f+yaw; var edge=V(Mathf.Cos(a),0,Mathf.Sin(a)); var joint=p+edge*.53f+Vector3.up*.15f;
                d.Rod(p+Vector3.up*.32f,joint,.12f,d.Mat("树皮","C1A68F","park_bark")); d.Rod(joint,p+edge*h*.23f+Vector3.up*.075f,.071f,d.Mat("树皮","C1A68F","park_bark"));
            }
        }
        internal static void Ecology(DistrictSet d)
        {
            var jade=d.Mat("生物绿","648E76",null,.17f,.38f); Shell(d,"ecology",jade);
            var soil=d.Mat("耕作土","514134",null,0,.24f); var water=d.Mat("循环水面","435C58",null,0f,.16f);
            foreach(float x in new[]{-2.65f,7.45f})
            {
                d.Box(V(x,.082f,3.48f),V(1.44f,.14f,19.1f),water);
                for(int s=-1;s<=1;s+=2) d.Box(V(x+s*.76f,.135f,3.48f),V(.10f,.26f,19.3f),d.Steel);
                foreach(float z in new[]{-.5f,7.4f}) for(int i=0;i<9;i++) d.Box(V(x,.18f,z-.66f+i*.17f),V(1.82f,.08f,.151f),d.Oak);
            }
            foreach(var bed in new[]{V(-6,0,.1f),V(-6.3f,0,6.7f),V(-8.1f,0,11),V(4.35f,0,9.4f),V(8.9f,0,2.15f)})
            {
                d.Box(bed+Vector3.up*.12f,V(4.25f,.24f,4.15f),d.Oak,true); d.Box(bed+Vector3.up*.26f,V(4.04f,.095f,3.91f),soil);
                Tree(d,bed+Vector3.up*.31f,d.Random(4.55f,6.05f),d.Random(0,360));
            }
            ArkWalk.Art.NurseryTrough.Build(d.transform,V(3.5f,0,-2.75f),-8);
            ArkWalk.Art.NurseryTrough.Build(d.transform,V(9.7f,0,4.4f),11);
            var pump=V(-5.1f,0,12.9f);
            for(int i=0;i<3;i++)
            {
                var a=pump+V(-i*1.75f,0,0); d.Rod(a+V(0,.14f,0),a+V(0,2.35f,0),.65f,jade); d.Block(a+Vector3.up*1.2f,V(1.27f,2.4f,1.27f),Quaternion.identity);
                foreach(float y in new[]{.18f,.59f,2.14f,2.40f}) d.Rod(a+V(0,y,0),a+V(0,y+.09f,0),.69f,d.Steel);
                d.Rod(a+V(0,2.4f,0),a+V(0,2.77f,0),.115f,d.Steel); d.Rod(a+V(0,2.72f,0),a+V(1.8f,2.72f,0),.12f,d.Steel);
            }
            var show = d.gameObject.AddComponent<DistrictWater>(); show.Build(d,pump+V(0,1.23f,-.70f));
            d.Mark(V(-3.0f,.85f,.5f),1.05f,"树根与水渠","沿着水边看看，大树露出来的根把湿土牢牢抓住。");
            d.Mark(V(3.5f,1.14f,-2.65f),1.05f,"蔬菜苗床","幼苗的根扎在湿土里。抬头看看，温室顶让阳光照到叶片上。");
            d.Mark(pump+V(0,1.3f,-.76f),.88f,"净水观察窗","点一下水泵，看叶轮转动。两条水道里的水纹也会加快。",show.Demonstrate);
        }
        internal static void Clinic(DistrictSet d)
        {
            var teal=d.Mat("护理蓝绿","678F98",null,.27f,.52f); Shell(d,"clinic",teal);
            var quilt=DistrictShapes.Quilt(d); var cloth=d.Mat("保温毯","ABBEB1","worn_linen",0,.24f); var palest=d.Mat("床单","E2DCCB");
            var sleep=d.Mat("睡眠乘员","FFFFFF","sleeperPalette"); var worker=d.Mat("护理装","FFFFFF","technicianPalette");
            for(int s=-1;s<=1;s+=2) for(int i=0;i<4;i++)
            {
                var p=V(s*(4.2f+(i%2)*1.1f),0,-2.45f+i*3.8f);
                d.Box(p+V(0,.37f,0),V(1.55f,.74f,2.80f),d.Panels,true);
                d.Box(p+V(0,.79f,0),V(1.46f,.16f,2.72f),teal); d.Box(p+V(0,.89f,0),V(1.21f,.15f,2.50f),palest);
                d.Ball(p+V(0,1.00f,.73f),V(.87f,.15f,.53f),palest);
                d.Add(quilt,p+V(0,.96f,0),Vector3.one,cloth,Quaternion.identity);
                var person=d.Model("sleeper",sleep,d.transform,p+V(0,1.105f,.48f),Vector3.one); if(person) person.localRotation=Quaternion.AngleAxis(s*11f,Vector3.forward)*Quaternion.Euler(0,180,0)*Quaternion.Euler(-73,0,0)*Quaternion.Euler(0,43,0);
                for(int side=-1;side<=1;side+=2){d.Rod(p+V(side*.7f,.88f,-1.19f),p+V(side*.7f,.88f,1.19f),.043f,d.Steel); d.Box(p+V(side*.78f,.53f,0),V(.038f,.063f,2.1f),d.Aqua);}
                foreach(float z in new[]{-1.09f,1.09f})
                {
                    Vector3 prior=p+V(-.74f,.91f,z);
                    for(int k=1;k<=12;k++){float a=k*Mathf.PI/12; var v=p+V(-.74f*Mathf.Cos(a),.91f+.65f*Mathf.Sin(a),z); d.Rod(prior,v,.039f,d.Steel); prior=v;}
                }
                d.Add(DistrictShapes.Canopy(d),p+V(0,.91f,0),V(.73f,.64f,1.08f),d.Glass,Quaternion.identity);
                var panel=p+V(-s*.83f,.89f,-.78f); d.Box(panel,V(.40f,.48f,.17f),d.Dark,false,V(21,0,0)); d.Box(panel+V(0,.045f,-.105f),V(.33f,.34f,.044f),d.Console,false,V(21,0,0));
            }
            var station=V(2.9f,0,11.7f); d.Box(station+V(0,.48f,0),V(2.8f,.96f,1.16f),d.Panels,true); d.Box(station+V(0,1.03f,0),V(3.2f,.13f,1.3f),d.Steel);
            for(int i=0;i<2;i++){var p=station+V(-.8f+1.5f*i,1.51f,.26f); d.Box(p,V(1.12f,.69f,.12f),d.Dark); d.Box(p+V(0,0,-.08f),V(1,.57f,.06f),d.Console);}
            d.Model("technician",worker,d.transform,station+V(-2.15f,0,.02f),Vector3.one,-35);
            d.Mark(V(-4.2f,1.28f,-2.45f),.91f,"休眠舱","透过弧形舱盖，可以看到枕头、保温毯和正在休息的乘员。");
            d.Mark(station+V(0,1.47f,0),1.15f,"护理工作台","医护人员在这里留意呼吸和舱内环境，确认每一位乘员都安稳。");
        }
        internal static void Bridge(DistrictSet d)
        {
            Shell(d,"bridge",d.Gold);
            var desk=V(0,0,2.9f); d.Box(desk+V(0,.47f,0),V(3.35f,.94f,2.24f),d.Panels,true); d.Box(desk+V(0,1.02f,0),V(4.1f,.17f,2.6f),d.Gold);
            d.Box(desk+V(0,1.14f,0),V(3.8f,.095f,2.32f),d.Mat("航图底图","AFDDE0","ArrivalChart",.1f,.49f,.15f));
            Vector3 last=desk+V(-1.52f,1.34f,-.49f);
            for(int n=0;n<6;n++){var pos=desk+V(-1.5f+n*.6f,1.36f+n*.086f,.35f*Mathf.Sin(n*1.4f)); d.Rod(last,pos+.006f*Vector3.right,.017f,d.Aqua); d.Ball(pos,Vector3.one*(n==5?.21f:.105f),n==5?d.Gold:d.Aqua); last=pos;}
            for(int s=-1;s<=1;s+=2) for(int i=0;i<3;i++)
            {
                var p=V(s*(4.4f+i*.7f),0,.3f+i*3.4f); d.Box(p+V(0,.48f,0),V(2.60f,.96f,1.12f),d.Dark,true);
                d.Box(p+V(0,1.15f,.07f),V(2.51f,.21f,1.09f),d.Steel,false,V(-13,0,0)); d.Box(p+V(0,1.315f,.16f),V(2.27f,.08f,.69f),d.Console,false,V(-13,0,0));
                Seat(d,p+V(0,0,-1.18f));
            }
            var mail=V(-3.25f,0,-3.05f); d.Box(mail+V(0,.92f,0),V(2.62f,.14f,1.15f),d.Oak,true);
            for(int s=-1;s<=1;s+=2) d.Box(mail+V(s*1.03f,.44f,.07f),V(.11f,.88f,.82f),d.Steel);
            for(int i=0;i<4;i++){var p=mail+V(-.87f+i*.51f,1.013f,-.08f+Mathf.Sin(i)*.17f);d.Box(p,V(.40f,.024f,.28f),d.Mat("信纸","E8D5B1"),false,V(0,-13+i*7,0));d.Rod(p+V(-.15f,.016f,.102f),p+V(0,.016f,-.05f),.008f,d.Gold);d.Rod(p+V(0,.016f,-.05f),p+V(.15f,.016f,.102f),.008f,d.Gold);}
            d.Mark(mail+V(0,1.05f,0),1.01f,"来自家里的信","桌上放着四封信。飞船走得很远，写信的人也能把家里的消息带到这里。");
            d.Mark(desk+V(0,1.58f,-.40f),1.21f,"立体航图","从近处的小光点看到高处的那一颗，细线连着我们一路经过的地方。");
            d.Mark(V(5.1f,1.36f,3.9f),1.1f,"值班导航台","领航员从这张桌子观察星空，核对飞船的方向。");
        }
        internal static void Seat(DistrictSet d, Vector3 p)
        {
            d.Rod(p+V(0,.1f,0),p+V(0,.51f,0),.13f,d.Steel); d.Box(p+V(0,.54f,0),V(.67f,.17f,.63f),d.Dark);
            d.Box(p+V(0,.92f,-.25f),V(.62f,.71f,.13f),d.Panels,false,V(-8,0,0)); d.Box(p+V(0,1.3f,-.27f),V(.40f,.21f,.16f),d.Dark);
        }
    }
}

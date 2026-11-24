using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk
{
    public static class ResidentialWorld
    {
        public static void Build(WalkSite site)
        {
            bool home=site.Id=="home";
            site.Spawn=new Vector3(0,.035f,home?-5.3f:-5.4f);
            site.Yaw=0f;
            site.Pitch=home?3.3f:2.7f;
            site.Exit=new Vector3(0,0,12.1f);
            var root=site.transform;
            Lights(root,home);
            ResidentialKit.Place(root,"atrium",Vector3.zero);
            ResidentialKit.Place(root,"walkway",Vector3.zero);
            ResidentialKit.Solid(root,"连续主路",new Vector3(-.90f,-.121f,3.02f),new Vector3(6.20f,.24f,22.12f));
            ResidentialKit.Solid(root,"街巷南端",new Vector3(-.92f,.86f,-8.08f),new Vector3(6.19f,1.72f,.14f));
            ResidentialKit.Solid(root,"环线站北侧栏",new Vector3(-.92f,.85f,14.12f),new Vector3(6.20f,1.70f,.12f));
            Garden(root);
            site.Mark(new Vector3(5.24f,.17f,-.33f),.28f,"草地上的足球","足球里面装着空气。它碰到地面会变扁一点，再鼓起来就弹走啦。");
            if(home) Homes(site);
            else Commons(site);
        }

        private static void Homes(WalkSite site)
        {
            var root=site.transform;
            for(int i=0;i<5;i++)
            {
                float z=-5.35f+3.98f*i;
                var wall=new Vector3(-2.91f,.002f,z);
                ResidentialKit.Place(root,"porch",wall,-90f);
                if(i<4) DetailLod(root,wall);
                ResidentialKit.Place(root,"flat",new Vector3(-3.10f,2.85f,z),-90f);
                if(i%2==1) ResidentialKit.Place(root,"flat",new Vector3(-3.45f,5.66f,z),-90f);
                ResidentialKit.Solid(root,"门内地面",new Vector3(-4.02f,-.09f,z),new Vector3(2.38f,.18f,3.88f));
                ResidentialKit.Solid(root,"住宅后壁",new Vector3(-5.15f,1.4f,z),new Vector3(.13f,2.80f,3.94f));
                ResidentialKit.Solid(root,"隔墙",new Vector3(-4.06f,1.36f,z-1.94f),new Vector3(2.18f,2.70f,.12f));
                ResidentialKit.Solid(root,"窗台",new Vector3(-2.97f,1.14f,z+.79f),new Vector3(.17f,2.28f,2.13f));
                if(i<3)
                {
                    Lamp(root,new Vector3(-2.83f,2.03f,z-1.64f),new Color(1f,.78f,.50f),.75f,3.1f);
                    Lamp(root,new Vector3(-4.10f,1.83f,z-.5f),new Color(1f,.81f,.56f),.87f,3.8f);
                }
            }
            ResidentialKit.Place(root,"streetlife",Vector3.zero);
            ResidentialKit.Place(root,"hydrangea",Vector3.zero);
            ResidentialKit.Solid(root,"玩具车",new Vector3(-.98f,.24f,-1.76f),new Vector3(.59f,.48f,1.09f),-60.2f);
            ResidentialKit.Solid(root,"木长椅",new Vector3(-2.01f,.36f,-2.20f),new Vector3(.47f,.72f,1.16f));
            foreach(Vector3 p in new[]{new Vector3(-2.44f,.16f,-1.55f),new Vector3(-2.62f,.18f,2.70f),new Vector3(-2.48f,.16f,6.35f),new Vector3(-2.60f,.16f,-5.57f)})
                ResidentialKit.Solid(root,"门前陶盆",p,new Vector3(.36f,.32f,.36f));
            ResidentialPeople.Neighbor(root,"住宅边散步的居民",new Vector3(-1.42f,0,5.4f),new Vector3(-1.42f,0,8.45f),1.65f);
            var girl=ResidentialPeople.Girl(root);
            site.Mark(new Vector3(0,1.305f,-.15f),.25f,"阿禾与纸星","我把纸折成小星星。想看看它是怎么从两只手里折出来的吗？",
                ()=>site.OfferStar(girl.Offer,girl.Accept,girl.Paper));
            site.Mark(new Vector3(-.98f,.32f,-1.76f),.42f,"红色小车","小车停在家门口。栏杆那边，是孩子们踢球的草地。");
            site.Mark(new Vector3(-2.82f,1.56f,-1.69f),.39f,"舰上的家","窗边暖暖的。住在环形城市里，抬起头还能看见对面的邻居。");
            Lamp(root,new Vector3(-.23f,1.70f,-1.54f),new Color(1f,.92f,.83f),.76f,3.2f);
            Lamp(root,new Vector3(.90f,1.92f,.69f),new Color(1f,.75f,.52f),.92f,3.5f);
        }

        private static void DetailLod(Transform root,Vector3 at)
        {
            var anchor=ResidentialKit.Group(root,"屋内器物距离层次",at);
            var near=ResidentialKit.Place(anchor,"details",Vector3.zero,-90f);
            var far=ResidentialKit.Place(anchor,"details_distance",Vector3.zero,-90f);
            var lod=anchor.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[]{new LOD(.22f,near.GetComponentsInChildren<Renderer>()),new LOD(.012f,far.GetComponentsInChildren<Renderer>())});
            lod.RecalculateBounds();
        }

        private static void Commons(WalkSite site)
        {
            var root=site.transform;
            ResidentialKit.Place(root,"commons",Vector3.zero);
            Wing(root,-5.60f,1.42f);
            Wing(root,2.10f,9.90f);
            foreach(float x in new[]{-5.66f,-4.25f,-2.89f})
            foreach(float z in new[]{-3.75f,-2.17f,-.48f})
            {
                ResidentialKit.Solid(root,"课桌",new Vector3(x,.37f,z),new Vector3(.67f,.75f,.96f));
                ResidentialKit.Solid(root,"教室椅",new Vector3(x+.57f,.34f,z),new Vector3(.39f,.68f,.42f));
            }
            ResidentialKit.Solid(root,"取餐台",new Vector3(-5.81f,.48f,5.73f),new Vector3(.94f,.97f,4.13f));
            foreach(float z in new[]{4.22f,6.55f,8.20f})
            {
                ResidentialKit.Solid(root,"餐桌",new Vector3(-3.56f,.4f,z),new Vector3(1.11f,.80f,1.4f));
                foreach(float x in new[]{-4.39f,-2.74f}) ResidentialKit.Solid(root,"餐桌边长凳",new Vector3(x,.24f,z),new Vector3(.37f,.48f,1.35f));
            }
            ResidentialKit.Solid(root,"望远镜基座",new Vector3(3.75f,.46f,7.5f),new Vector3(.52f,.93f,.52f));
            ResidentialPeople.StandingNeighbor(root,"午餐区的居民",new Vector3(-6.43f,0,4.90f),1.68f,72f);
            site.Mark(new Vector3(-6.80f,1.72f,-1.96f),.75f,"教室星图","孩子们在这里学认星座，也会给远方的朋友写信。");
            site.Mark(new Vector3(-5.65f,1.16f,4.20f),.54f,"餐桌上的午餐","舱里的厨师把刚收下来的蔬菜做成午饭。");
            site.Mark(new Vector3(3.81f,1.40f,7.51f),.45f,"公园望远镜","架稳镜筒，慢慢转动它，就能把远处的星光带到眼前。");
            Lamp(root,new Vector3(-4.48f,2.34f,-2.40f),new Color(1f,.91f,.75f),1.00f,5.3f);
            Lamp(root,new Vector3(-4.26f,2.30f,5.2f),new Color(1f,.84f,.66f),1.14f,5.7f);
        }

        private static void Wing(Transform parent,float min,float max)
        {
            float mid=(min+max)*.5f;
            ResidentialKit.Solid(parent,"社区室内地面",new Vector3(-4.49f,-.093f,mid),new Vector3(5.2f,.185f,max-min));
            ResidentialKit.Solid(parent,"社区后墙",new Vector3(-7.0f,1.5f,mid),new Vector3(.13f,3f,max-min));
            foreach(float z in new[]{min,max})
                ResidentialKit.Solid(parent,"社区侧墙",new Vector3(-4.5f,1.4f,z),new Vector3(5.1f,2.8f,.13f));
            foreach(float z in new[]{min+.12f,min+(max-min)*.33f,min+(max-min)*.68f,max-.12f})
                ResidentialKit.Solid(parent,"廊柱",new Vector3(-1.97f,1.5f,z),new Vector3(.105f,3f,.105f));
        }

        private static void Garden(Transform root)
        {
            ResidentialKit.Place(root,"court",Vector3.zero);
            ResidentialKit.Place(root,"terraces",Vector3.zero);
            CourtyardWater.Place(root,new Vector3(6.55f,7.43f,28.15f),1.36f,3.27f);
            CourtyardWater.Place(root,new Vector3(6.15f,4.22f,25.81f),1.08f,3.28f);
            ResidentialKit.Place(root,"oak_rail",Vector3.zero);
            ResidentialKit.Place(root,"tree",new Vector3(5.8f,.04f,4.8f),-16f,.94f);
            ResidentialKit.Place(root,"tree",new Vector3(7.18f,.04f,11.70f),-166f,1.10f);
            ResidentialKit.Place(root,"tree_distance",new Vector3(12.8f,-.02f,5f),69f,.92f);
            ResidentialKit.Place(root,"tree_distance",new Vector3(4.5f,-.04f,19.7f),-42f,.76f);
            ResidentialKit.Place(root,"tree_distance",new Vector3(-5.9f,-.04f,19.4f),-49f,.85f);
            ResidentialKit.Place(root,"tree_distance",new Vector3(17.4f,-.04f,18.4f),-67f,1.12f);
            ResidentialKit.Solid(root,"公共庭院地面",new Vector3(6.50f,-.125f,4.4f),new Vector3(9.10f,.25f,18.8f));
            ResidentialKit.Solid(root,"树干",new Vector3(5.8f,1.32f,4.80f),new Vector3(.63f,2.64f,.63f));
            ResidentialKit.Solid(root,"北边树干",new Vector3(7.18f,1.09f,11.70f),new Vector3(.42f,2.18f,.42f));
            ResidentialKit.Solid(root,"庭院东沿",new Vector3(11.05f,.49f,4.40f),new Vector3(.17f,.98f,18.80f));
            ResidentialKit.Solid(root,"庭院北沿",new Vector3(6.57f,.49f,13.82f),new Vector3(9f,.98f,.16f));
            ResidentialKit.Solid(root,"庭院南沿",new Vector3(6.57f,.49f,-4.97f),new Vector3(9f,.98f,.16f));
            ResidentialKit.Solid(root,"南扶手",new Vector3(2.18f,.51f,-.995f),new Vector3(.105f,1.02f,7.89f));
            ResidentialKit.Solid(root,"北扶手",new Vector3(2.18f,.51f,9.635f),new Vector3(.105f,1.02f,8.29f));
            for(int i=0;i<4;i++)
            {
                float a=Mathf.PI*(.69f+.18f*i);
                ResidentialKit.Solid(root,"树池木椅",new Vector3(5.8f+1.83f*Mathf.Cos(a),.26f,4.8f-1.83f*Mathf.Sin(a)),
                    new Vector3(.97f,.52f,.45f),a*Mathf.Rad2Deg+90f);
            }
            ResidentialPeople.Neighbor(root,"高架桥上的居民",new Vector3(1.2f,5.01f,15.51f),new Vector3(8.0f,5.03f,14.98f),1.67f);
            ResidentialPeople.StandingNeighbor(root,"桥上看球的孩子",new Vector3(-2.0f,5.01f,15.08f),1.12f,155f);
            ResidentialPeople.Neighbor(root,"结伴看球的孩子",new Vector3(3.92f,.015f,7.05f),new Vector3(5.12f,.015f,8.40f),1.08f);
            ResidentialPeople.StandingNeighbor(root,"树下的邻居",new Vector3(6.34f,.015f,8.49f),1.58f,-104f);
        }

        private static Material daylightSky;

        private static void Lights(Transform parent,bool home)
        {
            if(!daylightSky) daylightSky=new Material(Resources.Load<Shader>("Residence/AtriumSky"));
            RenderSettings.skybox=daylightSky;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.74f,.79f,.91f);
            RenderSettings.ambientEquatorColor=new Color(.46f,.46f,.54f);
            RenderSettings.ambientGroundColor=new Color(.34f,.29f,.25f);
            RenderSettings.ambientIntensity=.95f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogDensity=home?.014f:.013f;
            RenderSettings.fogColor=new Color(.87f,.79f,.69f);
            var source=ResidentialKit.Group(parent,"穿过舰城采光顶的暖光",new Vector3(7.2f,5.4f,6.2f));
            source.localRotation=Quaternion.LookRotation(-source.localPosition.normalized,Vector3.up);
            var sun=source.gameObject.AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.845f,.66f);
            sun.intensity=1.56f;
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.73f;
            sun.shadowBias=.048f;
            sun.shadowNormalBias=.20f;
            RenderSettings.sun=sun;
            var reflection=Resources.Load<Cubemap>("Residence/CourtyardReflection");
            if(reflection)
            {
                var probe=ResidentialKit.Group(parent,"街巷环境反射",new Vector3(.4f,1.9f,4.25f)).gameObject.AddComponent<ReflectionProbe>();
                probe.mode=ReflectionProbeMode.Custom;
                probe.customBakedTexture=reflection;
                probe.size=new Vector3(95,40,105);
                probe.boxProjection=true;
                probe.intensity=.93f;
            }
        }

        private static void Lamp(Transform parent,Vector3 position,Color color,float strength,float range)
        {
            var child=ResidentialKit.Group(parent,"局部反射与室内灯光",position);
            var light=child.gameObject.AddComponent<Light>();
            light.type=LightType.Point;
            light.renderMode=LightRenderMode.ForcePixel;
            light.range=range;
            light.color=color;
            light.intensity=strength;
            light.shadows=LightShadows.None;
        }
    }
}

using UnityEngine;
namespace ArkWalk
{
    internal static class ArkTakeoverMounts
    {
        // Legacy interior hook disabled for this standalone project.
        private static void Add()
        {
            ArkWingRegistry.Register("home",Home);
            ArkWingRegistry.Register("annex",Annex);
            ArkWingRegistry.Register("clinic",Clinic);
            ArkWingRegistry.Register("bridge",Bridge);
            ArkWingRegistry.Register("ecology",Ecology);
        }
        private static Transform Anchor(Transform p,string n,Vector3 point)
        {
            var g=new GameObject(n);g.transform.SetParent(p,false);g.transform.position=point;return g.transform;
        }
        private static void Home(WalkSite s)
        {
            s.Mark(new Vector3(1.72f,1.15f,6.4f),.7f,"去中庭、训练与实验翼","",()=>s.Visit("annex"));
        }
        private static void Annex(WalkSite s)
        {
            s.Spawn=new Vector3(0,0,1.5f);s.Yaw=0;s.Pitch=0;
            var wing=ArkCabinWing.Build(s.transform);
            s.Mark(new Vector3(5.31f,1.06f,8f),.48f,"点亮或熄灭节庆灯","",()=>{var lit=wing.ToggleFestivalLantern();Debug.Log("ATRIUM_LANTERN="+lit);s.Tell("点灯节",lit?"纸灯点亮了。":"纸灯熄灭了。");});
            s.Mark(new Vector3(0,1.08f,.22f),.62f,"返回住宅街巷","",()=>s.Visit("home"));
            s.Mark(new Vector3(0,1.05f,15.8f),.58f,"中庭遮阳帆 · 请走上踏板","");
            s.Mark(new Vector3(-14,1.05f,11.6f),.58f,"训练飞轮 · 沿三个足位依次前进","");
            s.Mark(new Vector3(14.5f,1.02f,11.6f),.55f,"实验舱 · 观察光敏花","");
            Mount(s,new Vector3(17.5f,.66f,15.5f),ArkLifeStation.Kind.Xenobiology,"异星生物模拟");
        }
        private static void Clinic(WalkSite s)
        {
            Mount(s,new Vector3(6.05f,.34f,.8f),ArkLifeStation.Kind.Cryogenics,"空置休眠展示");
        }
        private static void Bridge(WalkSite s)
        {
            Mount(s,new Vector3(-4.25f,.68f,8.4f),ArkLifeStation.Kind.Navigator,"离线值班员");
        }
        private static void Mount(WalkSite s,Vector3 p,ArkLifeStation.Kind kind,string title)
        {
            var station=ArkLifeStation.Install(Anchor(s.transform,title,p),kind);
            if(kind==ArkLifeStation.Kind.Navigator)
            {
                ArkLifeStation.RegisterRoom("xeno",null);
                ArkLifeStation.RegisterRoom("cryo",null);
            }
            int count=kind==ArkLifeStation.Kind.Navigator?3:2;
            var controls=new WalkMark[count];
            void Refresh()
            {
                var choices=station.Choices;
                for(int i=0;i<controls.Length;i++)if(controls[i])
                {
                    controls[i].gameObject.SetActive(i<choices.Length);
                    if(i<choices.Length)controls[i].Title=choices[i];
                }
            }
            for(int i=0;i<count;i++)
            {
                int j=i;
                controls[i]=s.Mark(p+new Vector3((i-(count-1)*.5f)*.55f,.52f,-.67f),.28f,title,"",()=>
                {s.Tell(title,station.Interact(j));Refresh();}).GetComponent<WalkMark>();
            }
            Refresh();
        }
        private static void Ecology(WalkSite s)
        {
            var prefab=Resources.Load<GameObject>("Astra/Dewfall");
            if(!prefab)return;
            var obj=UnityEngine.Object.Instantiate(prefab,s.transform,false);
            obj.name="Dewfall authored planting column";obj.transform.localPosition=new Vector3(4.7f,0,6.9f);
            var solid=obj.AddComponent<CapsuleCollider>();solid.center=new Vector3(0,1.10f,0);solid.height=2.20f;solid.radius=.67f;
        }
    }
}

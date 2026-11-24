using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ArkWalk
{
    public sealed class WalkCheck : MonoBehaviour
    {
        private WalkApp app;
        private string folder;
        private readonly List<string> checks = new List<string>();
        private int failures;

        public void Begin(WalkApp owner, string path)
        {
            app = owner;
            folder = Path.GetFullPath(path);
            Directory.CreateDirectory(folder);
            StartCoroutine(Run());
        }

        private void Check(bool ok, string label)
        {
            if (!ok) failures++;
            checks.Add((ok ? "通过　" : "失败　") + label);
            Debug.Log(checks[checks.Count-1]);
            File.WriteAllLines(Path.Combine(folder, "运行验证.txt"),checks, new UTF8Encoding(false));
        }

        private IEnumerator Run()
        {
            checks.Add(Application.unityVersion + "  " + SystemInfo.graphicsDeviceName + "  " + Screen.width + " × " + Screen.height);
            yield return new WaitForSeconds(.8f);
            app.Hud.Clean = true;
            yield return Snap("01-沿街看");
            Vector3 start = app.Pilot.transform.position;
            yield return WalkToZ(-.92f, 5);
            Check(app.Pilot.transform.position.z > -1.6f && Vector3.Distance(start,app.Pilot.transform.position)>3, "从出生点沿主路走近阿禾");
            var ahe = app.Site.Marks.Find(m => m.Title == "阿禾与纸星");
            var touch = PointAt(ahe, true);
            Check(touch == ahe, "点击所见人物开启递星");
            yield return new WaitForSeconds(1.5f);
            var paper = app.Site.Marks.Find(m => m.Paper && m.isActiveAndEnabled);
            Check(paper && app.Narrator.clip && app.Narrator.clip.samples > 0, "递星与本地普通话资源");
            yield return Snap("02-阿禾递星");
            if (paper) PointAt(paper,false);
            Check(paper && !paper.gameObject.activeSelf && app.Narrator.clip && app.Narrator.clip.name == WalkApp.VoiceId("这颗小星星，送给今天来舰上做客的你。"), "从双手间接到纸星");
            app.Hud.Clean = false;
            app.Hud.OpenRoutes(false);
            yield return Snap("03-漫游地图");
            int words = 0;
            for (int i=0;i<Array.IndexOf(WalkApp.Places,"annex");i++)
            {
                app.Go(WalkApp.Places[i]);
                app.Hud.Clean=true;
                yield return null;
                int targets = 0;
                var missing = new List<string>();
                foreach (var mark in app.Site.Marks)
                {
                    if (string.IsNullOrEmpty(mark.Speech)) continue;
                    targets++;
                    var clip = Resources.Load<AudioClip>("Voices/" + WalkApp.VoiceId(mark.Speech));
                    if (!clip || clip.samples == 0) missing.Add(mark.Title);
                }
                words += targets;
                Check(targets > 1 && missing.Count == 0, app.Title + "可介绍物件 " + targets + "，缺音 " + string.Join("、",missing));
                Check(app.Site.GetComponentsInChildren<Renderer>().Length > 4, app.Title + "的场景渲染器已实例化");
                if (i > 0) yield return Snap((i+3).ToString("00")+"-"+app.Title);
            }
            app.Go("dock");
            yield return WalkToZ(5.72f,10);
            var seat = app.Site.Marks.Find(m=>m.Title.Contains("驾驶台"));
            Check(app.Pilot.transform.position.z > 5.4f && app.Pilot.transform.position.y > .12f, "徒步从尾门经坡板进入驾驶舱");
            if (seat) PointAt(seat,true);
            Check(app.Hud.Modal,"点击驾驶台可选择航线");
            foreach(var orbit in ArkVoyage.Orbits)
            {
                app.Hud.OpenRoutes(true);
                int card=Array.IndexOf(ArkVoyage.Orbits,orbit);
                app.Hud.TapQAFlightCard(card);
                yield return new WaitForEndOfFrame();
                Check(app.Site.Id==orbit.Id && app.Pilot.InAir,"Actual HUD flight card "+orbit.Id);
                app.Hud.Clean=true;
                if(orbit.Id=="earth" || orbit.Id=="mars" || orbit.Id=="mercury" || orbit.Id=="venus" || orbit.Id=="uranus" || orbit.Id=="neptune" || orbit.Id=="sirius" || orbit.Id=="pleiades")
                {
                    yield return Snap("28-"+orbit.Id+"-Arrival");
                    var original=app.Pilot.transform.position;
                    for(int n=0;n<40;n++){app.Pilot.Move(new Vector2(.55f,.55f),.025f);yield return null;}
                    Check(Vector3.Distance(original,app.Pilot.transform.position)>1f,"Controlled flight "+orbit.Id);
                    if(orbit.Id=="sirius")
                    {
                        var world=app.Site.GetComponentsInChildren<Renderer>();
                        var a=Array.Find(world,x=>x.name=="Sirius A blue-white photosphere");
                        var b=Array.Find(world,x=>x.name=="Sirius B white dwarf");
                        Check(a && b && Vector3.Distance(a.bounds.center,b.bounds.center)>10 && a.bounds.size.magnitude>b.bounds.size.magnitude*3,"Native binary pair separated in 3D");
                    }
                    if(orbit.Id=="pleiades")
                    {
                        var stars=Array.FindAll(app.Site.GetComponentsInChildren<Renderer>(),x=>x.name.StartsWith("Pleiades landmark "));
                        Check(stars.Length>=5 && Array.Exists(stars,x=>Mathf.Abs(x.bounds.center.z-stars[0].bounds.center.z)>15),"Pleiades actual 3D multi-depth landmarks");
                    }
                    if(orbit.Id=="neptune")
                    {
                        bool hitTriton=false;
                        foreach(var c in app.Site.GetComponentsInChildren<SphereCollider>())
                            if(c.name=="Triton foreground flyby")
                            {
                                Vector3 p=c.bounds.center;
                                hitTriton=c.Raycast(new Ray(p+Vector3.up*(c.bounds.extents.y+2f),Vector3.down),out var hit,5f) && hit.distance>0;
                            }
                        Check(hitTriton,"Triton solid physics ray hits the native scene collider");
                    }
                    if(orbit.Id=="mars")
                    {
                        var mesh=app.Site.GetComponentsInChildren<MeshCollider>();
                        bool terrain=false;
                        foreach(var c in mesh)if(c.name=="Martian canyon") terrain=c.Raycast(new Ray(new Vector3(0,100,15),Vector3.down),out var hit,240) && hit.distance>0;
                        Check(terrain,"Mars solid canyon physics ray hits mesh");
                    }
                    yield return Snap("29-"+orbit.Id+"-Piloted");
                }
                app.Site.ReturnToDock();
                Check(app.Site.Id=="dock" && !app.Pilot.InAir,"Restores dock walking mode "+orbit.Id);
            }
            var dockPos=app.Pilot.transform.position;
            app.Pilot.Move(Vector2.right,.04f);
            Check(!app.Pilot.InAir && Vector3.Distance(dockPos,app.Pilot.transform.position)>.01f,"Walk movement after last return");
            app.Go("saturn");
            start=app.Pilot.transform.position;
            app.Pilot.ToggleCruise();
            yield return new WaitForSeconds(1.3f);
            Check(app.Pilot.Cruise && Vector3.Distance(start,app.Pilot.transform.position)>1.2f,"土星巡航实际位移");
            app.Pilot.ToggleCruise();
            start=app.Pilot.transform.position;
            for (int i=0;i<45;i++) app.Pilot.Move(Vector2.right,1f/45f);
            Check(!app.Pilot.Cruise && app.Pilot.transform.position.x-start.x > 3f, "退出巡航后可手动驾驶");
            app.Site.ReturnToDock();
            Check(app.Site.Id=="dock" && !app.Pilot.InAir, "返航后回到船坞步行状态");
            app.Go("commons");
            app.Pilot.Place(app.Site.Exit-Vector3.forward*1.55f,0,0,false);
            yield return new WaitForSeconds(1.25f);
            for(int i=0;i<120 && app.Site.Id=="commons";i++)
            {
                yield return new WaitForEndOfFrame();
                app.Pilot.Move(Vector2.up,.016f);
                // 让场景负责判定脚下的通道，与正常连续行走采用同一条路径。
                app.CheckGate();
            }
            Check(app.Site.Id=="ecology","步行经过舱区连接口");
            yield return Circuit();
            yield return TakeoverRooms();
            checks.Add("热点讲解计数 " + words + "，未通过 " + failures);
            File.WriteAllLines(Path.Combine(folder,"运行验证.txt"),checks,new UTF8Encoding(false));
            Application.Quit(failures == 0 ? 0 : 3);
        }

        private IEnumerator TakeoverRooms()
        {
            app.Go("home");yield return null;
            var entry=app.Site.Marks.Find(m=>m.Title.Contains("训练与实验翼"));
            Check(entry!=null,"住宅真实可见新翼入口存在");
            if(!entry)yield break;
            app.Pilot.Place(new Vector3(1.15f,.035f,4.7f),0,0,false);
            PointAt(entry,true);yield return null;
            bool loaded=app.Site.Id=="annex"&&app.Site.GetComponentInChildren<ArkCabinWing>();
            Check(loaded,"从住宅点选入口加载实际中庭、训练与实验几何");
            if(!loaded)yield break;
            app.Hud.Clean=true;yield return Snap("13-新翼中庭");
            var start=app.Pilot.transform.position;
            for(int i=0;i<125;i++){app.Pilot.Move(Vector2.up,.022f);yield return null;}
            Check(Vector3.Distance(start,app.Pilot.transform.position)>2.4f,"中庭可通过实际碰撞体自由步行");
            var group=app.Site.GetComponentInChildren<ArkCabinWing>();
            var sail=group.transform.Find("Retractable sun sail");
            var oldSail=sail.localScale;
            yield return MoveTo(new Vector2(0,17));
            yield return new WaitForSeconds(.9f);
            Check(Vector3.Distance(oldSail,sail.localScale)>.05f,"Atrium treadle changes actual sun sail");
            yield return MoveTo(new Vector2(0,12));
            yield return MoveTo(new Vector2(-7.62f,12));
            yield return MoveTo(new Vector2(-7.65f,21.41f));
            Debug.Log("ATRIUM_RAMP "+app.Pilot.transform.position.ToString("F3"));
            yield return MoveTo(new Vector2(-7.65f,22.70f));
            Debug.Log("ATRIUM_TURN "+app.Pilot.transform.position.ToString("F3"));
            Check(app.Pilot.transform.position.y>2.65f,"Atrium west ramp rises with existing controller");
            yield return MoveTo(new Vector2(0,21.93f));
            Debug.Log("ATRIUM_MIDSPAN "+app.Pilot.transform.position.ToString("F3"));
            yield return MoveTo(new Vector2(6.95f,21.22f));
            Debug.Log("ATRIUM_LANDING "+app.Pilot.transform.position.ToString("F3"));
            Check(app.Pilot.transform.position.x>6.5f && app.Pilot.transform.position.y>2.65f,"Atrium walked across high bridge to upper landing");
            app.Pilot.Aim(221,12);
            yield return Snap("26-AtriumUpperLanding");
            yield return MoveTo(new Vector2(-7.48f,22.67f));
            yield return MoveTo(new Vector2(-7.62f,12));
            Check(app.Pilot.transform.position.y<.18f,"Atrium after return ramp back at lower deck");
            var rotor=group.transform.Find("Human powered flywheel");
            yield return MoveTo(new Vector2(0,12));
            yield return MoveTo(new Vector2(-14,12));
            yield return MoveTo(new Vector2(-20,12));
            yield return MoveTo(new Vector2(-26,12));
            float wheel=rotor.localEulerAngles.x;
            yield return new WaitForSeconds(.40f);
            Check(Mathf.Abs(Mathf.DeltaAngle(wheel,rotor.localEulerAngles.x))>15&&app.Pilot.transform.position.x < -25,
                "现在通过真实物理行走触发三个训练踏板与飞轮");
            app.Pilot.Aim(270,0);yield return Snap("15-训练踏板飞轮");
            yield return MoveTo(new Vector2(0,12));
            var flower=group.transform.Find("Phototropic corolla");
            yield return MoveTo(new Vector2(14.5f,12));yield return new WaitForSeconds(1.15f);
            Check(flower.localScale.x>.78f,"实验翼光敏花踏板后实物展开");
            app.Pilot.Aim(90,0);yield return Snap("16-光敏实验舱");
            var life=app.Site.GetComponentInChildren<ArkLifeStation>();
            bool xeno=life&&life.Type==ArkLifeStation.Kind.Xenobiology&&life.Status.Contains("日光");
            if(xeno){app.Pilot.Place(new Vector3(17.5f,0,12.8f),0,0,false);var sw=app.Site.Marks.Find(m=>m.Title=="切换琥珀光谱");PointAt(sw,true);yield return null;xeno=life.Status.Contains("琥珀");}
            Check(xeno,"实验观察池热点可改变实际光谱状态");
            yield return Snap("14-实验观察池");
            var back=app.Site.Marks.Find(m=>m.Title=="返回住宅街巷");
            if(back){app.Pilot.Place(new Vector3(0,0,2.4f),180,0,false);PointAt(back,true);}
            Check(app.Site.Id=="home","新翼使用实际返回热点回住宅");
            app.Go("clinic");yield return null;
            life=app.Site.GetComponentInChildren<ArkLifeStation>();
            Check(life&&life.Type==ArkLifeStation.Kind.Cryogenics&&life.Status.Contains("空舱"),"医疗区空置冷冻展示加载");
            var lid=life?life.transform.Find("侧轴翻盖"):null;
            var openMark=app.Site.Marks.Find(m=>m.Title=="打开模拟舱盖");
            app.Pilot.Place(new Vector3(5.8f,0,-1.35f),0,0,false);
            var actualTap=PointAt(openMark,true);yield return new WaitForSeconds(1.35f);
            Check(actualTap==openMark&&lid&&lid.localEulerAngles.z>25,"空置冷冻展示真实热点掀开舱盖");
            yield return Snap("17-空置休眠舱");
            app.Go("bridge");yield return null;
            life=app.Site.GetComponentInChildren<ArkLifeStation>();
            Check(life&&life.Type==ArkLifeStation.Kind.Navigator&&life.Choices.Length==2,"舰桥离线值班员入口加载");
            app.Pilot.Place(new Vector3(-4.25f,0,6.1f),0,0,false);
            var question=app.Site.Marks.Find(m=>m.Title=="看看休眠舱");
            var picked=PointAt(question,true);yield return null;
            var locate=app.Site.Marks.Find(m=>m.Title=="指向这个舱室");
            var chosen=PointAt(locate,true);
            Check(picked==question&&chosen==locate&&life.Destination==null,"离线值班员可选话题；未加载舱室不假造指引");
            yield return Snap("18-舰桥离线值班员");
            app.Go("ecology");yield return null;
            var plant=app.Site.transform.Find("Dewfall authored planting column");
            Check(plant!=null,"真实种植塔已装入生态舱");
            app.Pilot.Place(new Vector3(2.8f,0,3.6f),30,0,false);
            yield return Snap("19-生态Dewfall近景");
            var pump=app.Site.GetComponentInChildren<DistrictWater>();
            var mark=app.Site.Marks.Find(m=>m.Title=="净水观察窗");
            app.Pilot.Place(new Vector3(-4.55f,0,10.55f),0,0,false);
            var direction=Quaternion.LookRotation(mark.Point-app.Eye.transform.position).eulerAngles;
            app.Pilot.Aim(direction.y,Mathf.DeltaAngle(0,direction.x));
            yield return Snap("20A-水道默认");
            float phase=pump.FlowPhase;
            var tapped=PointAt(mark,true);
            yield return new WaitForSeconds(.72f);
            Check(tapped==mark&&pump.FlowPhase-phase>.6f,"真实净水观察窗点击使新水纹加快，泵的行为保持");
            yield return Snap("20B-水泵启动");
            app.Go("annex");yield return null;
            app.Pilot.Place(new Vector3(0,0,11.7f),180,0,false);
            yield return Snap("21-中庭入口回望");
            yield return MoveTo(new Vector2(0,-3.4f));
            var vestibule=app.Pilot.transform.position;
            Check(vestibule.z< -3.23f&&vestibule.y>-.10f,"原玩家控制器步入有实体地面的换乘前室");
            app.Pilot.Aim(180,0);yield return Snap("22-住宅换乘前室");
            yield return MoveTo(new Vector2(0,2.4f));
            Check(app.Pilot.transform.position.z>2.20f&&app.Pilot.transform.position.y>-.10f,"同一玩家双向穿门无阻退回中庭");
            var rec=app.Site.Marks.Find(m=>m.Title=="返回住宅街巷");
            if(rec)PointAt(rec,true);
            Check(app.Site.Id=="home","回望镜头后仍能走回中庭入口返住宅");
        }

        private IEnumerator Circuit()
        {
            app.Go("home");
            app.Pilot.Place(app.Site.Spawn,app.Site.Yaw,app.Site.Pitch,false);
            for (int index=0;index<6;index++)
            {
                string id=WalkApp.Places[index];
                if (app.Site.Id!=id) break;
                yield return new WaitForSeconds(1.24f);
                // 住宅绕过阿禾，船坞从右侧绕过机翼，舰桥从航图桌旁经过。
                float side=id=="dock"?4.9f:id=="bridge"?2.45f:id=="home"?.85f:0f;
                float z=app.Site.Exit.z;
                var points=new[] {new Vector2(side,-3.5f),new Vector2(side,z-2),new Vector2(app.Site.Exit.x,z)};
                foreach (var point in points)
                {
                    for (int step=0;step<720 && app.Site.Id==id;step++)
                    {
                        Vector3 at=app.Pilot.transform.position;
                        Vector2 delta=point-new Vector2(at.x,at.z);
                        if (delta.magnitude<.075f) break;
                        app.Pilot.Aim(Mathf.Atan2(delta.x,delta.y)*Mathf.Rad2Deg,0);
                        app.Pilot.Move(Vector2.up,.02f);
                        app.CheckGate();
                        if (step%24==0) yield return null;
                    }
                }
                Check(app.Site.Id==WalkApp.Places[(index+1)%6],WalkApp.Names[index]+"沿路步行抵达"+WalkApp.Names[(index+1)%6]);
            }
        }
        private WalkMark PointAt(WalkMark mark,bool aim)
        {
            if (!mark) return null;
            if (aim)
            {
                Vector3 angles = Quaternion.LookRotation(mark.Point-app.Eye.transform.position).eulerAngles;
                app.Pilot.Aim(angles.y,Mathf.DeltaAngle(0,angles.x));
            }
            return app.Pick(app.Eye.WorldToScreenPoint(mark.Point));
        }
        private IEnumerator WalkToZ(float limit, float seconds)
        {
            float elapsed=0;
            while (app.Pilot.transform.position.z < limit && elapsed < seconds)
            {
                elapsed+=.02f;
                app.Pilot.Move(Vector2.up,.02f);
                yield return new WaitForFixedUpdate();
            }
        }
        private IEnumerator MoveTo(Vector2 point)
        {
            for(int i=0;i<300;i++)
            {
                var pos=app.Pilot.transform.position;
                Vector2 d=point-new Vector2(pos.x,pos.z);
                if(d.magnitude<.18f)yield break;
                app.Pilot.Aim(Mathf.Atan2(d.x,d.y)*Mathf.Rad2Deg,0);
                app.Pilot.Move(Vector2.up,.055f);
                yield return null;
            }
        }
        private IEnumerator Snap(string name)
        {
            yield return null;
            if(name=="03-漫游地图")yield break; // These are actual scene cameras, not GUI screenshots.
            const int w=1920,h=1080;
            var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);
            var original=RenderTexture.active;var previous=app.Eye.targetTexture;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            try
            {
                app.Eye.targetTexture=rt;app.Eye.Render();
                RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0,false);tex.Apply(false);
                var px=tex.GetPixels32();int vari=0;
                for(int i=17;i<px.Length;i+=113)if(!px[i].Equals(px[0]))vari++;
                Check(vari>300,"实景相机非黑/非纯色 "+name+" at "+app.Eye.transform.position.ToString("F3"));
                if(vari>300)File.WriteAllBytes(Path.Combine(folder,name+".png"),tex.EncodeToPNG());
            }
            finally {app.Eye.targetTexture=previous;RenderTexture.active=original;RenderTexture.ReleaseTemporary(rt);Destroy(tex);}
        }
    }
}



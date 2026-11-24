using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace ArkWalk
{
    internal sealed class ArkSpaceCheck:MonoBehaviour
    {
        WalkApp app; string path; int failures;
        internal void Begin(WalkApp a,string p){app=a;path=p;Directory.CreateDirectory(p);StartCoroutine(Run());}
        void Check(bool yes,string label){if(!yes)failures++;Debug.Log("SPACE_CHECK "+(yes?"PASS ":"FAIL ")+label);}
        void Frame(string name) {
            RenderFrame(name);
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkStationTrussAB")>=0 &&
                (name=="01-StationOpening"||name=="07-Station-SideApproach")){
                var station=GameObject.Find("ArkStationExterior");
                var newRail=station?station.transform.Find("Hull mesh A0ABB2"):null;
                var oldRail=station?station.transform.Find("Hull mesh 8B9CA3"):null;
                var r=newRail?newRail.GetComponent<MeshRenderer>():null;
                var previous=oldRail?oldRail.GetComponent<MeshRenderer>():null;
                Check(r && previous && !newRail.GetComponent<Collider>(),"station truss finish only: "+name);
                if(r&&previous){
                    var current=r.sharedMaterial;
                    try{r.sharedMaterial=previous.sharedMaterial;RenderFrame(name+"-TrussPrevious");}
                    finally{r.sharedMaterial=current;}
                }
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkEarthCloudAB")>=0 &&
                (name=="01-StationOpening"||name=="02-earth-Arrival"||name=="14-Earth-NorthPole")){
                var layer=GameObject.Find("Dated cloud cover layer");
                var cloud=layer?layer.GetComponent<MeshRenderer>():null;
                var opacity=cloud?cloud.sharedMaterial.mainTexture:null;
                Check(cloud && opacity && opacity.width==4096 && opacity.height==2048 && cloud.sharedMaterial.shader.isSupported && !layer.GetComponent<Collider>(),"Earth dated cloud shell and opacity: "+name);
                if(cloud){
                    try{cloud.enabled=false;RenderFrame(name+"-CloudOff");}
                    finally{cloud.enabled=true;}
                }
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkEarthOceanAB")>=0 &&
                (name=="01-StationOpening"||name=="02-earth-Arrival"||name=="14-Earth-NorthPole")){
                var ball=GameObject.Find(name=="01-StationOpening"?"Earth behind external station":"Ocean and continents");
                var render=ball?ball.GetComponent<MeshRenderer>():null;
                var trial=ball?ball.GetComponent<ArkEarthOceanTrial>():null;
                var candidate=render?render.sharedMaterial:null;
                var mask=candidate&&candidate.HasProperty("_OceanMask")?candidate.GetTexture("_OceanMask"):null;
                Check(trial && mask && mask.width==2700 && mask.height==1350 && candidate.shader.isSupported,"conservative ocean mask and trial shader: "+name);
                if(trial&&candidate){
                    try {render.sharedMaterial=trial.Original;RenderFrame(name+"-OriginalOcean");}
                    finally {render.sharedMaterial=candidate;}
                }
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkEarthBaseAB")>=0 &&
                (name=="01-StationOpening"||name=="02-earth-Arrival"||name=="14-Earth-NorthPole")){
                var ball=GameObject.Find(name=="01-StationOpening"?"Earth behind external station":"Ocean and continents");
                var render=ball?ball.GetComponent<MeshRenderer>():null;
                var baseMap=Resources.Load<Texture2D>("Districts/earth_nasa_bmng_july_base");
                var before=render?render.sharedMaterial.mainTexture:null;
                Check(baseMap && before && baseMap.width==5400 && baseMap.height==2700,"NASA Earth base and retained relief textures: "+name);
                if(baseMap&&before){
                    try {render.sharedMaterial.mainTexture=baseMap;RenderFrame(name+"-BaseTrial");}
                    finally {render.sharedMaterial.mainTexture=before;}
                }
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkEarthLimbAB")<0 ||
                (name!="01-StationOpening"&&name!="02-earth-Arrival"&&name!="14-Earth-NorthPole"))return;
            var shell=GameObject.Find("Earth thin rim atmosphere");
            var renderer=shell?shell.GetComponent<MeshRenderer>():null;
            Check(renderer && renderer.sharedMaterial.shader.isSupported && !shell.GetComponent<Collider>(),"Earth limb draw supported with no collision: "+name);
            if(!renderer)return;
            try {renderer.enabled=false;RenderFrame(name+"-LimbOff");}
            finally{renderer.enabled=true;}
        }
        Texture2D sourceCue;
        Texture2D ReadPixels() {
            var eye=app.Eye;var old=eye.targetTexture;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try{eye.targetTexture=rt;eye.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1920,1080),0,0);t.Apply();return t;}
            finally{eye.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
        }
        void SaveFrame(Texture2D t,string name){try{File.WriteAllBytes(Path.Combine(path,name+".png"),t.EncodeToPNG());}finally{Destroy(t);}}
        void RenderFrame(string name){SaveFrame(ReadPixels(),name);}
        IEnumerator UranusAB() {
            var globe=GameObject.Find("Uranus side-on poles");
            var marker=globe?globe.GetComponent<ArkUranusOfficial>():null;
            var r=globe?globe.GetComponent<MeshRenderer>():null;
            var f=globe?globe.GetComponent<MeshFilter>():null;
            var texture=r?r.sharedMaterial.mainTexture:null;
            Check(marker && f.sharedMesh.vertexCount==3256 && texture && texture.width==1024 && texture.height==512,"Uranus official mesh, native atlas and matching projection");
            if(!marker)yield break;
            var rings=GameObject.Find("Uranus equatorial ribbon 0");
            Check(rings && Mathf.Abs(Vector3.Dot(globe.transform.up,rings.transform.up))>.9999f,"Uranus oblate axis matches preserved ring plane");
            var mesh=f.sharedMesh;var mat=r.sharedMaterial;
            void Pair(string shot) {
                RenderFrame(shot);
                try {f.sharedMesh=marker.OriginalMesh;r.sharedMaterial=marker.OriginalMat;RenderFrame(shot+"-Previous");}
                finally {f.sharedMesh=mesh;r.sharedMaterial=mat;}
            }
            Pair("23-Uranus-Arrival");
            var toward=(globe.transform.position-app.Pilot.transform.position).normalized;
            app.Pilot.Aim(Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg,-Mathf.Asin(toward.y)*Mathf.Rad2Deg);
            var origin=app.Pilot.transform.position;
            for(int i=0;i<330 && Vector3.Distance(globe.transform.position,app.Pilot.transform.position)>35.3f;i++){
                app.Pilot.Move(Vector2.up,.035f);yield return null;
            }
            float clearance=Vector3.Distance(globe.transform.position,app.Pilot.transform.position);
            Debug.Log($"URANUS QA real pilot from {origin:F3} to {app.Pilot.transform.position:F3}, radius distance={clearance:F3}");
            Check(clearance>34.9f&&clearance<36.2f&&Vector3.Distance(origin,app.Pilot.transform.position)>70f,"Uranus playable close approach respects existing hazard");
            Pair("24-Uranus-CloseFlight");
        }
        IEnumerator MoonLowPass() {
            var origin=app.Pilot.transform.position;
            var materialPass=FindAnyObjectByType<ArkLunarRegolith>();
            var render=materialPass?materialPass.GetComponent<MeshRenderer>():null;
            Check(render && render.sharedMaterial.shader.isSupported && render.GetComponent<MeshCollider>().sharedMesh==render.GetComponent<MeshFilter>().sharedMesh,"Moon material only: render and collision retain one terrain mesh");
            void Pair(string shot) {
                RenderFrame(shot);
                if(!render)return;
                var test=render.sharedMaterial;
                try{render.sharedMaterial=materialPass.Original;RenderFrame(shot+"-Previous");}
                finally{render.sharedMaterial=test;}
            }
            Pair("25-Moon-Arrival");
            var scatter=FindAnyObjectByType<ArkLunarRocks>();
            var rocks=scatter?scatter.GetComponent<MeshFilter>():null;
            Check(scatter && rocks && scatter.Count==145 && scatter.Previous && !scatter.GetComponent<Collider>(),"Moon existing 145 render-only boulders and original comparison batch");
            void RockPair(string shot) {
                RenderFrame(shot);
                if(!rocks||!scatter.Previous)return;
                var candidate=rocks.sharedMesh;
                try{rocks.sharedMesh=scatter.Previous;RenderFrame(shot+"-PreviousRocks");}
                finally{rocks.sharedMesh=candidate;}
            }
            RockPair("27-Moon-RocksArrival");
            app.Pilot.Aim(7,18);
            for(int i=0;i<180&&app.Pilot.transform.position.y>-3.8f;i++){
                app.Pilot.Move(Vector2.up,.035f);yield return null;
            }
            app.Pilot.Aim(7,0);
            for(int i=0;i<160&&app.Pilot.transform.position.z<24f;i++){
                app.Pilot.Move(Vector2.up,.035f);yield return null;
            }
            var open=app.Pilot.transform.position;
            bool hit=Physics.Raycast(open,Vector3.down,out var ground,15,Physics.DefaultRaycastLayers&~(1<<WalkPilot.PlayerLayer));
            Check(Vector3.Distance(origin,open)>20&&hit&&ground.collider is MeshCollider&&ground.distance>1&&ground.distance<8,"Moon real low pass over unchanged solid terrain");
            Debug.Log($"MOON LOW PASS pilot {origin:F3} -> {open:F3}; terrain below {ground.distance:F3}; solid collider={ground.collider is MeshCollider}");
            app.Pilot.Aim(7,12);
            Pair("26-Moon-LowPass");
            RockPair("28-Moon-RocksLowPass");
        }
        IEnumerator VenusOrbitalPass() {
            var globe=GameObject.Find("Venus: layered cloud-top atlas");
            if(!globe){Check(false,"Venus orbital model");yield break;}
            var state=globe.GetComponent<ArkVenusComparison>();
            var display=globe.GetComponent<MeshRenderer>();
            Check(state && state.Previous && display && display.sharedMaterial.shader.isSupported && globe.GetComponent<SphereCollider>().radius==1,"Venus opaque cloud material and prior atlas, same collision");
            void Pair(string shot){
                RenderFrame(shot);
                if(!state||!state.Previous)return;
                var candidate=display.sharedMaterial;
                try{display.sharedMaterial=state.Previous;RenderFrame(shot+"-Previous");}
                finally{display.sharedMaterial=candidate;}
            }
            Pair("29-Venus-Arrival");
            var start=app.Pilot.transform.position;
            app.Pilot.Aim(11,-2);
            for(int i=0;i<260&&app.Pilot.transform.position.z<62;i++){
                app.Pilot.Move(Vector2.up,.035f);yield return null;
            }
            var end=app.Pilot.transform.position;
            float range=Vector3.Distance(end,globe.transform.position);
            app.Pilot.Aim(24,-1);
            bool clear=!Physics.CheckSphere(end,.3f,Physics.DefaultRaycastLayers&~(1<<WalkPilot.PlayerLayer));
            Check(Vector3.Distance(start,end)>55&&range>42&&range<58&&clear,"Venus real exterior flyby reaches clear limb view");
            Debug.Log($"VENUS QA pilot {start:F3} -> {end:F3}; range {range:F3}; clear={clear}");
            Pair("30-Venus-Flyby");
        }
        IEnumerator SaturnPass()
        {
            var ribbon=GameObject.Find("Saturn crystal ribbon");
            Check(ribbon && ribbon.GetComponentsInChildren<MeshRenderer>().Length==28,"28 real crystal meshes");
            var field=GameObject.Find("Saturn intermediate ring fragments");
            Check(field && field.GetComponentsInChildren<MeshFilter>().Length==8 && field.GetComponentsInChildren<Collider>().Length==0,"Saturn middle fragments are true geometry and add no collision");
            var shard=GameObject.Find("Keel Shard");
            var globe=GameObject.Find("Saturn authored globe");
            if(!shard||!globe)yield break;
            var mat=shard.GetComponent<MeshRenderer>().sharedMaterial;
            Check(mat.shader.isSupported && mat.shader==Resources.Load<Shader>("SaturnCrystalRibbon") && mat.renderQueue>=3000,"actual transparent crystal shader");
            Frame("02-saturn-Arrival");
            app.Pilot.Aim(0,0);
            for(int i=0;i<140 && app.Pilot.transform.position.z<7;i++){app.Pilot.Move(new Vector2(0,1),.035f);yield return null;}
            app.Pilot.Aim(12,0);
            Vector3 pointA=app.Pilot.transform.position;
            Vector3 a=app.Eye.WorldToViewportPoint(shard.transform.position), b=app.Eye.WorldToViewportPoint(globe.transform.position);
            Frame("05-Saturn-TangentA");
            for(int i=0;i<110 && app.Pilot.transform.position.z<pointA.z+6;i++){app.Pilot.Move(new Vector2(0,1),.035f);yield return null;}
            Vector3 pointB=app.Pilot.transform.position;
            Vector3 c=app.Eye.WorldToViewportPoint(shard.transform.position), d=app.Eye.WorldToViewportPoint(globe.transform.position);
            float n=Vector2.Distance(a,c), f=Vector2.Distance(b,d);
            Debug.Log($"SATURN TANGENT {pointA:F3} -> {pointB:F3}; normalized near {n:F4}, far {f:F4}; shader {mat.shader.name}, queue {mat.renderQueue}");
            Check(pointB.z-pointA.z>=6f && n>f*2 && c.z>0,"controlled tangent and stronger near parallax");
            bool frostAB=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkSaturnFrostAB")>=0;
            void FrostFrame(string shot){
                Frame(shot);
                if(!frostAB)return;
                try{mat.SetFloat("_FrostCoat",0);Frame(shot+"-PriorOptics");}
                finally{mat.SetFloat("_FrostCoat",1);}
            }
            FrostFrame("06-Saturn-TangentB");
            var filters=(frostAB?field:ribbon).GetComponentsInChildren<MeshFilter>();
            MeshFilter mid=Array.Find(filters,x=> {float z=frostAB?x.GetComponent<MeshRenderer>().bounds.center.z:x.transform.position.z;return z>43&&z<53;});
            if(mid){
                Vector3 midCenter=frostAB?mid.GetComponent<MeshRenderer>().bounds.center:mid.transform.position;Vector3 midB=app.Eye.WorldToViewportPoint(midCenter);
                for(int i=0;i<135 && app.Pilot.transform.position.x>pointB.x-7;i++){
                    app.Pilot.Move(new Vector2(-1,0),.035f);yield return null;
                }
                var pointC=app.Pilot.transform.position;
                float nearSide=Vector2.Distance(c,app.Eye.WorldToViewportPoint(shard.transform.position));
                float midSide=Vector2.Distance(midB,app.Eye.WorldToViewportPoint(midCenter));
                float farSide=Vector2.Distance(d,app.Eye.WorldToViewportPoint(globe.transform.position));
                Debug.Log($"SATURN LATERAL {pointB:F3} -> {pointC:F3}; near={nearSide:F4} mid={midSide:F4} far={farSide:F4}");
                Check(pointC.x<=pointB.x-6.9f && nearSide>midSide && midSide>farSide, "Saturn real lateral travel with near/mid/far parallax");
                if(frostAB)Check(mid.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_FrostCoat")==0
                    &&mat.GetFloat("_FrostCoat")==1&&ribbon.GetComponentsInChildren<Collider>().Length==0,
                    "near frost isolated from true middle batches; unchanged render-only flight clearance");
                FrostFrame("16-Saturn-Lateral");
            }else Check(false,"Saturn middle-distance ice witness");
        }

        IEnumerator Run()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkFlightTrace")>=0){yield return ArkFlightTrace.Run(app,path);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkRoutePerf")>=0){
                yield return ArkRoutePerf.Probe(app,path);
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkNeptuneRawPerfOnly")>=0){
                    bool exact=ArkIceGiants.CheckBakedNeptune();
                    Debug.Log($"NEPTUNE_RAW Windows runtime RGBA generated == baked texels: {exact}");
                    Application.Quit(exact?0:3);yield break;
                }
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkAllOrbitAudit")>=0){yield return ArkOrbitAudit.Run(app,path,RenderFrame);yield break;}
            yield return new WaitForSeconds(.4f);
            Check(app.Site.Id=="station-orbit" && app.Pilot.InAir && !app.Hud.Modal,"input-free exterior startup");
            Check(app.Eye.GetComponentsInChildren<Renderer>().Length==0,"no cockpit or camera-attached geometry");
            Check(!Physics.CheckSphere(app.Eye.transform.position,.25f,Physics.DefaultRaycastLayers & ~(1<<WalkPilot.PlayerLayer)),"spawn clear");
            Frame("01-StationOpening");
            var station = GameObject.Find("ArkStationExterior");
            Check(station && station.GetComponentsInChildren<MeshRenderer>().Length >= 8,"authored exterior mesh groups");
            Vector3 initial=app.Pilot.transform.position;
            for(int i=0;i<150;i++){app.Pilot.Move(new Vector2(-1f,.50f),.035f);yield return null;}
            Vector3 moved=app.Pilot.transform.position;
            Debug.Log($"STATION SIDE {initial:F3} -> {moved:F3}");
            Check(moved.x<initial.x-20 && moved.z>initial.z+20,"controlled approach and sidebar translation");
            if(station){Vector3 aim=station.transform.position-app.Eye.transform.position;
                app.Pilot.Aim(Mathf.Atan2(aim.x,aim.z)*Mathf.Rad2Deg,-Mathf.Atan2(aim.y,new Vector2(aim.x,aim.z).magnitude)*Mathf.Rad2Deg);}
            Check(!Physics.CheckSphere(app.Eye.transform.position,.25f,Physics.DefaultRaycastLayers & ~(1<<WalkPilot.PlayerLayer)),"exterior approach collision clear");
            Frame("07-Station-SideApproach");
            string[] ordered={"earth","mercury","venus","mars","jupiter","saturn","uranus","neptune","moon","proxima","sirius","pleiades","orion"};
            foreach(var id in ordered)
            {
                app.Hud.OpenRoutes(true);int i=Array.FindIndex(ArkVoyage.Orbits,x=>x.Id==id);
                if(i<0){Check(false,"missing route: "+id);continue;}
                bool audioWitness=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkTravelAudio")>=0;
                int lastStart=app.TravelTone.Accepted,lastArrival=app.TravelTone.Arrived; float oldVolume=AudioListener.volume;
                if(audioWitness&&id=="mercury")AudioListener.volume=0;
                bool cueWitness=id=="earth"&&Array.IndexOf(Environment.GetCommandLineArgs(),"-arkFlightCueAB")>=0;
                if(cueWitness){RenderFrame("20-Travel-Source");yield return null;}
                float cueStart=Time.realtimeSinceStartup;
                app.Hud.TapQAFlightCard(i);
                if(cueWitness){
                    var originSite=app.Site;
                    Check(app.FlightCueBusy&&app.Site.Id=="station-orbit","route cue begins outside before deliberate cut");
                    Check(!app.RequestFlight("jupiter")&&!app.RequestFlight("clinic"),"cue rejects second selection and invalid destination");
                    app.Hud.Back();app.Hud.OpenRoutes(true);app.Hud.TapQAReturn();
                    Check(!app.Hud.Modal&&app.Site==originSite,"cue blocks rapid return and Esc/chart");
                }
                bool captured=false;
                float cutAt=-1;
                while(app.FlightCueBusy && Time.realtimeSinceStartup-cueStart<1.5f){
                    yield return null;
                    if(cueWitness&&app.Site.Id==id&&cutAt<0)cutAt=Time.realtimeSinceStartup-cueStart;
                    if(cueWitness&&!captured&&app.Site.Id!="earth"&&app.FlightCueStrength>.25f){
                        // Render visually; defer PNG encoding until after the measured cue.
                        sourceCue=ReadPixels();captured=true;
                    }
                }
                if(cueWitness){
                    float total=Time.realtimeSinceStartup-cueStart;
                    Debug.Log($"FLIGHT_CUE QA cut observed {cutAt*1000:F0}ms, input open {total*1000:F0}ms");
                    Check(captured&&cutAt>=.09f&&total>=.215f&&total<.60f&&!app.FlightCueBusy&&app.FlightCueStrength==0,"brief visible travel cue completes once and clears");
                    if(sourceCue){SaveFrame(sourceCue,"21-Travel-SourceVeil");sourceCue=null;}
                    RenderFrame("22-Travel-ArrivedClear");
                    Check(!app.RequestFlight("earth")&&!app.RequestFlight("clinic"),"no duplicate current route and invalid idle route");
                }
                yield return new WaitForEndOfFrame();
                if(audioWitness&&id=="earth"){
                    yield return new WaitForSecondsRealtime(.09f);
                    var p=app.TravelTone.Probe;
                    float begin=p?p.AcceptPeak:0f,end=p?p.ArrivePeak:0f;
                    Debug.Log($"TRAVEL_AUDIO actual source DSP peaks raw={begin:F5},{end:F5}, gain={ArkTravelTone.Gain:F3}; QA now silences source output");
                    Check(lastStart==0 && lastArrival==0 && app.TravelTone.Accepted==1 && app.TravelTone.Arrived==1,"audio has no startup or duplicate route cues and exactly two accepted events");
                    Check(p&&begin>.005f&&end>.005f&&begin<.6f&&end<.6f,"actual Windows audio DSP produced restrained separate route cues");
                }
                if(audioWitness&&id=="mercury"){
                    Check(app.TravelTone.Accepted==lastStart && app.TravelTone.Arrived==lastArrival,"game listener mute suppresses both travel audio events");
                    AudioListener.volume=oldVolume;
                }
                Check(app.Site.Id==id && app.Pilot.InAir && app.Eye.GetComponentsInChildren<Renderer>().Length==0,"route "+id);
                if(id=="mars"){Frame("10-Mars-CanyonExisting");if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMarsArtTrial")>=0)ArkMarsArtTrial.Capture(app.Eye,Frame,Check);}
                if(id=="earth" && Array.IndexOf(Environment.GetCommandLineArgs(),"-arkEarthNorth")>=0){
                    var globe=GameObject.Find("Ocean and continents");
                    var mesh=globe?globe.GetComponent<MeshFilter>().sharedMesh:null;
                    Check(mesh,"Earth actual globe for north sample");
                    if(mesh){
                        var verts=mesh.vertices;var uv=mesh.uv;
                        int at=-1;float best=10;
                        for(int k=0;k<uv.Length;k++){
                            if(uv[k].x>.01f)continue;
                            float score=Mathf.Abs(verts[k].y)+uv[k].x;
                            if(score<best){at=k;best=score;}
                        }
                        Check(at>=0,"located Earth UV seam");
                        if(at>=0){
                            var eye=app.Eye.transform;var position=eye.position;var rotation=eye.rotation;
                            var eq=new Vector3(verts[at].x,0,verts[at].z).normalized;
                            try{
                                foreach(float lon in new[]{-5f,0f,5f}){
                                    var az=Quaternion.AngleAxis(lon,Vector3.up)*eq;
                                    var aim=(Vector3.up*Mathf.Sin(86*Mathf.Deg2Rad)+az*Mathf.Cos(86*Mathf.Deg2Rad)).normalized;
                                    Vector3 world=globe.transform.TransformDirection(aim);
                                    Vector3 sample=globe.transform.position+world*45.5f;
                                    var spawn=new Vector3(0,2,0);
                                    Check(Vector3.Distance(sample,ArkVoyage.Bound("earth",spawn,spawn,sample))<.02f,"north camera within normal flight bounds "+lon);
                                    eye.SetPositionAndRotation(sample,Quaternion.LookRotation(-world,globe.transform.TransformDirection(az)));
                                    Frame("13-Earth-North-"+(lon<0?"Before":lon>0?"After":"Seam"));
                                }
                                eye.SetPositionAndRotation(globe.transform.position+globe.transform.up*53,Quaternion.LookRotation(-globe.transform.up,globe.transform.TransformDirection(eq)));
                                Frame("14-Earth-NorthPole");
                            }finally{eye.SetPositionAndRotation(position,rotation);}
                        }
                    }
                }
                if(id=="jupiter"){
                    var filters=FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
                    MeshFilter planet=Array.Find(filters,x=>x.sharedMesh && x.sharedMesh.name=="NASA VTAD Jupiter");
                    var tex=planet?planet.GetComponent<MeshRenderer>().sharedMaterial.mainTexture:null;
                    Check(planet && planet.sharedMesh.vertexCount==1649 && tex && tex.width==4096 && tex.height==3072,"NASA VTAD modeled Jupiter with full atlas");
                    Frame("08-Jupiter-Arrival");
                    if(planet){
                        var io=Array.Find(filters,x=>x.sharedMesh && Vector3.Distance(x.transform.position,new Vector3(-15,5,51))<.01f);
                        var oldTex=Resources.Load<Texture2D>("Districts/jupiter");
                        Check(io && oldTex,"original Jupiter comparison model and texture");
                        if(io && oldTex){
                            var runner=planet.GetComponent<MeshRenderer>();var savedMat=runner.sharedMaterial;var savedMesh=planet.sharedMesh;
                            var original=new Material(savedMat){mainTexture=oldTex};
                            try {runner.sharedMaterial=original;planet.sharedMesh=io.sharedMesh;Frame("08-Jupiter-LegacyMatched");}
                            finally{runner.sharedMaterial=savedMat;planet.sharedMesh=savedMesh;Destroy(original);}
                        }
                        var eye=app.Eye.transform;var oldPos=eye.position;var oldRot=eye.rotation;
                        foreach(var sample in new[]{Vector3.back,Vector3.forward,Vector3.up,Vector3.down}){
                            eye.position=planet.transform.position+sample*89;
                            eye.rotation=Quaternion.LookRotation(-sample,Mathf.Abs(sample.y)>.9f?Vector3.forward:Vector3.up);
                            Frame("09-Jupiter-"+(sample.z<0?"Front":sample.z>0?"Far":sample.y>0?"North":"South"));
                        }
                        eye.SetPositionAndRotation(oldPos,oldRot);
                    }
                }
                if(id=="venus"&&Array.IndexOf(Environment.GetCommandLineArgs(),"-arkVenusAB")>=0)yield return VenusOrbitalPass();
                if(id=="uranus"&&Array.IndexOf(Environment.GetCommandLineArgs(),"-arkUranusAB")>=0)yield return UranusAB();
                if(id=="moon"&&Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMoonAB")>=0)yield return MoonLowPass();
                if(id=="saturn"){yield return SaturnPass();continue;} if(id=="earth"||id=="sirius"||id=="pleiades"||id=="orion")
                {
                    Frame("02-"+id+"-Arrival");Vector3 old=app.Pilot.transform.position;
                    for(int k=0;k<40;k++){app.Pilot.Move(new Vector2(.60f,.64f),.03f);yield return null;}
                    Check(Vector3.Distance(app.Pilot.transform.position,old)>2,"steering "+id);Frame("03-"+id+"-Flight");
                }
            }
            app.Go("clinic"); Check(app.Site.Id=="orion","rejects interior id");
            app.Hud.TapQAReturn();float returning=Time.realtimeSinceStartup;while(app.FlightCueBusy&&Time.realtimeSinceStartup-returning<1.5f)yield return null;yield return new WaitForEndOfFrame();
            Check(app.Site.Id=="station-orbit" && app.Pilot.InAir,"return to exterior station");
            Frame("04-StationReturn");Application.Quit(failures==0?0:3);
        }
    }
}

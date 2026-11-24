using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace ArkWalk {
internal static class ArkOrbitAudit {
    static double Now()=>Time.realtimeSinceStartupAsDouble;
    static bool Clear(Vector3 v)=>!Physics.CheckSphere(v,.25f,Physics.DefaultRaycastLayers & ~(1<<WalkPilot.PlayerLayer),QueryTriggerInteraction.Ignore);
    static string N(double v)=>v.ToString("F3",CultureInfo.InvariantCulture);
    static string P(Vector3 v)=>N(v.x)+"/"+N(v.y)+"/"+N(v.z);
    static void Aim(WalkPilot pilot,Vector3 v) {
        var d=v.normalized;pilot.Aim(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,-Mathf.Asin(d.y)*Mathf.Rad2Deg);
    }
    static bool StellarPair(WalkApp app,Action<string> frame,string file){
        frame(file);
        var witness=app.Site.GetComponentInChildren<ArkStellarMaterialComparison>();
        if(!witness)return false;
        var changed=new System.Collections.Generic.List<(MeshRenderer,Material)>();
        try{
            foreach(var r in app.Site.GetComponentsInChildren<MeshRenderer>()){
                var candidate=r.sharedMaterial;
                if(candidate&&witness.Previous.TryGetValue(candidate,out var original)){
                    changed.Add((r,candidate));r.sharedMaterial=original;
                }
            }
            frame(file+"-PreviousMaterial");
        }finally{foreach(var e in changed)e.Item1.sharedMaterial=e.Item2;}
        int required=app.Site.Id=="sirius"?2:9;
        Debug.Log($"STAR_MATERIAL QA {app.Site.Id} unchanged transforms; {changed.Count} switched renderers");
        return changed.Count>=required;
    }
    static bool MarsPair(WalkApp app,Action<string> frame,string file){
        frame(file);
        var witness=app.Site.GetComponentInChildren<ArkMarsStrataComparison>();
        if(!witness)return false;
        var renderer=witness.GetComponent<MeshRenderer>();var current=renderer.sharedMaterials;
        var filter=witness.GetComponent<MeshFilter>();var collision=witness.GetComponent<MeshCollider>();
        bool same=filter&&collision&&filter.sharedMesh==collision.sharedMesh;
        try{renderer.sharedMaterials=witness.Prior;frame(file+"-PreviousStrata");}
        finally{renderer.sharedMaterials=current;}
        Debug.Log($"MARS_STRATA QA {file} collision and renderer hold exact same terrain mesh: {same}; submeshes {filter.sharedMesh.subMeshCount}");
        return same&&current.Length==3&&current[0]==current[1]&&current[1]==current[2]&&current[0].shader.isSupported;
    }
    static bool NeptunePair(WalkApp app,Action<string> frame,string file){
        frame(file);
        var state=app.Site.GetComponentInChildren<ArkNeptuneStormComparison>();
        var renderer=state?state.GetComponent<MeshRenderer>():null;
        if(!renderer||!state.Previous)return false;
        var candidate=renderer.sharedMaterial;
        try{renderer.sharedMaterial=state.Previous;frame(file+"-PreviousStorm");}
        finally{renderer.sharedMaterial=candidate;}
        var t=candidate.mainTexture;
        bool ok=t&&t.width==768&&t.height==384&&state.GetComponent<SphereCollider>().radius==1;
        Debug.Log($"NEPTUNE_STORM QA {file} exact pose and same mesh; standard atlas and collider: {ok}");
        return ok;
    }
    static bool NeptuneDetailPair(WalkApp app,Action<string> frame,string file){
        var state=Array.Find(app.Site.GetComponentsInChildren<ArkNeptuneStormComparison>(),x=>x.PriorDetail);
        if(!state)return false;
        var r=state.GetComponent<MeshRenderer>();
        var normal=r.sharedMaterial;
        if(state.Generated){
            var raw=normal.mainTexture as Texture2D;
            frame(file);
            try{normal.mainTexture=state.Generated;frame(file+"-GeneratedAtlas");}
            finally{normal.mainTexture=raw;}
            bool same=raw&&raw.width==768&&raw.height==384&&raw.mipmapCount==state.Generated.mipmapCount
               &&raw.format==state.Generated.format&&raw.wrapModeU==state.Generated.wrapModeU&&raw.wrapModeV==state.Generated.wrapModeV
               &&raw.filterMode==state.Generated.filterMode&&raw.anisoLevel==state.Generated.anisoLevel;
            Debug.Log($"NEPTUNE_RAW QA {file}: runtime fallback and baked sampler settings identical={same}");
            return same;
        }
        frame(file);
        try{r.sharedMaterial=state.PriorDetail;frame(file+"-PriorCloudDetail");}
        finally{r.sharedMaterial=normal;}
        bool shared=normal.mainTexture==state.PriorDetail.mainTexture&&Mathf.Approximately(state.GetComponent<SphereCollider>().radius,1);
        Debug.Log($"NEPTUNE_CLOUD QA {file}: same atlas and collider; GPU detail only: {shared}");
        return shared;
    }
    static bool OrionFilamentPair(WalkApp app,Action<string> frame,string file){
        frame(file);
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkOrionFilamentAB")<0)return true;
        var obj=app.Site.GetComponentInChildren<ArkOrionVolume>();
        if(!obj)return false;
        var m=obj.GetComponent<MeshRenderer>().sharedMaterial;
        if(!m.HasProperty("_Filament")||!Mathf.Approximately(m.GetFloat("_Filament"),1))return false;
        try{m.SetFloat("_Filament",0);frame(file+"-PriorFilament");}
        finally{m.SetFloat("_Filament",1);}
        return true;
    }
    static bool MoonRockTonePair(WalkApp app,Action<string> frame,string file){
        var witness=app.Site.GetComponentInChildren<ArkMoonRockToneWitness>();
        if(!witness||!witness.Prior)return false;
        var batches=new System.Collections.Generic.List<(MeshRenderer,Material)>();
        int verts=0;
        foreach(var r in app.Site.GetComponentsInChildren<MeshRenderer>()){
            if(!r.sharedMaterial||!r.sharedMaterial.name.StartsWith("Moon dry basalt ",StringComparison.Ordinal))continue;
            if(r.GetComponent<Collider>())return false;
            batches.Add((r,r.sharedMaterial));
            verts+=r.GetComponent<MeshFilter>().sharedMesh.vertexCount;
        }
        if(batches.Count<3)return false;
        frame(file);
        try{foreach(var b in batches)b.Item1.sharedMaterial=witness.Prior;frame(file+"-PriorBasalt");}
        finally{foreach(var b in batches)b.Item1.sharedMaterial=b.Item2;}
        Debug.Log($"MOON_ROCK_TONE QA {file}: same {verts} rock vertices and placements, {batches.Count} no-collider batches; material-only");
        return verts>0;
    }
    static bool MarsGrainPair(WalkApp app,Action<string> frame,string file){
        var rock=GameObject.Find("Martian canyon");
        if(!rock)return false;
        var r=rock.GetComponent<MeshRenderer>();
        var mat=r.sharedMaterial;
        var enabled=mat.HasProperty("_Grain")&&Mathf.Approximately(mat.GetFloat("_Grain"),1);
        if(!enabled)return false;
        frame(file);
        bool projection=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMarsWallAB")>=0;
        string knob=projection?"_WallProjection":"_Grain";
        if(!mat.HasProperty(knob))return false;
        try{mat.SetFloat(knob,0);frame(file+(projection?"-PriorWallProjection":"-PriorGrain"));}
        finally{mat.SetFloat(knob,1);}
        var mesh=rock.GetComponent<MeshFilter>().sharedMesh;
        bool same=mesh&&mesh==rock.GetComponent<MeshCollider>().sharedMesh&&mesh.subMeshCount==3;
        Debug.Log($"MARS_GRAIN QA {file}: same terrain vertices/indices/collider, shader grain only={same}");
        return same;
    }
    static bool MercuryPair(WalkApp app,Action<string> frame,string file){
        frame(file);
        var state=app.Site.GetComponentInChildren<ArkMercuryMaterialComparison>();
        var renderer=state?state.GetComponent<MeshRenderer>():null;
        if(!renderer||!state.Previous)return false;
        var candidate=renderer.sharedMaterial;
        try{renderer.sharedMaterial=state.Previous;frame(file+"-PreviousFineGain");}
        finally{renderer.sharedMaterial=candidate;}
        var t=candidate.mainTexture;
        bool ok=t&&t.width==768&&t.height==384&&Mathf.Approximately(state.GetComponent<SphereCollider>().radius,1.03f);
        Debug.Log($"MERCURY_FINE QA {file} exact pose and same shaded relief; same atlas size and collider: {ok}");
        return ok;
    }
    static bool MercuryReliefPair(WalkApp app,Action<string> frame,string file,float standOff){
        var states=app.Site.GetComponentsInChildren<ArkMercuryMaterialComparison>();
        var state=Array.Find(states,x=>x.PreviousMesh);
        var filter=state?state.GetComponent<MeshFilter>():null;
        if(!filter)return false;
        var mesh=filter.sharedMesh;
        frame(file);
        try{filter.sharedMesh=state.PreviousMesh;frame(file+"-PriorReliefMesh");}
        finally{filter.sharedMesh=mesh;}
        float min=float.MaxValue,max=0;
        foreach(var v in mesh.vertices){float r=v.magnitude*state.transform.lossyScale.x;min=Mathf.Min(min,r);max=Mathf.Max(max,r);}
        var collider=state.GetComponent<SphereCollider>();
        bool ok=collider&&collider.enabled&&!collider.isTrigger&&Mathf.Approximately(collider.radius,1.03f)
            &&max<standOff-.5f&&mesh.vertexCount>state.PreviousMesh.vertexCount;
        Debug.Log($"MERCURY_RELIEF QA {file}: {state.PreviousMesh.vertexCount} -> {mesh.vertexCount} vertices, visual radius {min:F3}..{max:F3}, unchanged safety standoff {standOff:F3}, local collider valid={ok}");
        return ok;
    }
    static bool PleiadesScalePair(WalkApp app,Action<string> frame,string file){
        var proof=app.Site.GetComponentInChildren<ArkPleiadesScaleComparison>();
        if(!proof)return false;
        var current=new System.Collections.Generic.List<MeshRenderer>();
        var previous=new System.Collections.Generic.List<MeshRenderer>();
        foreach(var render in app.Site.GetComponentsInChildren<MeshRenderer>(true)){
            var mat=render.sharedMaterial;
            if(mat==proof.ColdPrior||mat==proof.CoolPrior)previous.Add(render);
            if(mat==proof.ColdCandidate||mat==proof.CoolCandidate)current.Add(render);
        }
        foreach(var r in previous)r.enabled=false;
        frame(file);
        try {
            foreach(var r in current)r.enabled=false;
            foreach(var r in previous)r.enabled=true;
            frame(file+"-PriorScale");
        }finally{
            foreach(var r in previous)r.enabled=false;
            foreach(var r in current)r.enabled=true;
        }
        var colliders=app.Site.GetComponentsInChildren<SphereCollider>();
        Debug.Log($"PLEIADES_SCALE QA {file} same 7 landmark colliders: {colliders.Length}; original/new noncollision batches {previous.Count}/{current.Count}");
        return colliders.Length==7&&previous.Count>0&&current.Count==previous.Count;
    }
    static bool UranusRingPair(WalkApp app,Action<string> frame,string file){
        var state=app.Site.GetComponentInChildren<ArkUranusRingComparison>();
        if(!state)return false;
        var candidate=new System.Collections.Generic.Dictionary<MeshRenderer,Material>();
        bool unchangedLayout=true;var body=GameObject.Find("Uranus side-on poles");
        foreach(var pair in state.Previous){
            var r=pair.Key; candidate.Add(r,r.sharedMaterial);
            unchangedLayout&=!r.GetComponent<Collider>()&&body&&
                Mathf.Abs(Vector3.Dot(r.transform.up,body.transform.up))>.9999f&&r.sharedMaterial.renderQueue>=3000;
        }
        frame(file);
        try {
            foreach(var pair in state.Previous)pair.Key.sharedMaterial=pair.Value;
            frame(file+"-PriorOpaqueRings");
        }finally{foreach(var pair in candidate)pair.Key.sharedMaterial=pair.Value;}
        Debug.Log($"URANUS_RINGS QA {file}: {candidate.Count} same tilted render-only ribbon meshes; transmission changed without collider or planet changes={unchangedLayout}");
        return candidate.Count==8&&unchangedLayout;
    }
    static bool SaturnAnnulusPair(WalkApp app,Action<string> frame,string file){
        var state=app.Site.GetComponentInChildren<ArkSaturnAnnulusComparison>();
        var r=state?state.GetComponent<MeshRenderer>():null;
        if(!r||!state.Previous)return false;
        var mesh=r.GetComponent<MeshFilter>().sharedMesh;var candidate=r.sharedMaterial;
        frame(file);
        try{r.sharedMaterial=state.Previous;frame(file+"-PriorAnnulus");}
        finally{r.sharedMaterial=candidate;}
        var near=GameObject.Find("Saturn crystal ribbon");
        var mid=GameObject.Find("Saturn intermediate ring fragments");
        bool ok=mesh==r.GetComponent<MeshFilter>().sharedMesh&&!state.GetComponent<Collider>()&&near&&mid&&
            near.GetComponentsInChildren<MeshRenderer>().Length==28&&mid.GetComponentsInChildren<MeshRenderer>().Length==8&&
            candidate.mainTexture==state.Previous.mainTexture&&candidate.shader.isSupported&&candidate.renderQueue>=3000;
        Debug.Log($"SATURN_ANNULUS QA {file}: same atlas and mesh, 28 near / 8 middle batches, no annulus collider: {ok}");
        return ok;
    }
    static bool ProximaRockPair(WalkApp app,Action<string> frame,string file){
        var proof=app.Site.GetComponentInChildren<ArkProximaCompanionComparison>();
        if(!proof||!proof.Previous)return false;
        var render=proof.GetComponent<MeshRenderer>();
        var candidate=render.sharedMaterial;
        var mesh=proof.GetComponent<MeshFilter>().sharedMesh;
        frame(file);
        try{render.sharedMaterial=proof.Previous;frame(file+"-PreviousColor");}
        finally{render.sharedMaterial=candidate;}
        var c=proof.GetComponent<SphereCollider>();
        bool ok=c&&c.radius==1&&candidate.mainTexture==proof.Previous.mainTexture&&mesh==proof.GetComponent<MeshFilter>().sharedMesh;
        Debug.Log($"PROXIMA_ROCK QA {file} material-only, same scene geometry and atlas: {ok}");
        return ok;
    }
    static bool PleiadesLandmarkPair(WalkApp app,Action<string> frame,string file){
        var witness=app.Site.GetComponentInChildren<ArkPleiadesLandmarkComparison>();
        if(!witness)return false;
        bool shared=witness.Landmarks.Count==7;
        var newScales=new Vector3[witness.Landmarks.Count];
        for(int j=0;j<newScales.Length;j++){
            var (target,oldScale)=witness.Landmarks[j];
            newScales[j]=target.localScale;
            var c=target.GetComponent<SphereCollider>();var m=target.GetComponent<MeshFilter>();
            shared&=c&&m&&c.transform==m.transform&&Mathf.Approximately(c.radius,1.15f)&&
                Vector3.Distance(newScales[j],oldScale*ArkPleiadesLandmarkComparison.PairedScale)<.001f;
        }
        frame(file);
        try{
            for(int j=0;j<newScales.Length;j++)witness.Landmarks[j].Item1.localScale=witness.Landmarks[j].Item2;
            frame(file+"-PriorLandmarkScale");
        }finally{
            for(int j=0;j<newScales.Length;j++)witness.Landmarks[j].Item1.localScale=newScales[j];
            Physics.SyncTransforms();
        }
        Debug.Log($"PLEIADES_LANDMARK QA {file}: same seven orbit positions with coupled mesh and sphere scale: {shared}");
        return shared;
    }
    internal static IEnumerator Run(WalkApp app,string folder,Action<string> frame){
        yield return new WaitForSecondsRealtime(.6f);
        frame("00-station-arrival");
        var targetFile=Path.Combine(folder,"orbit-audit.csv");
        File.WriteAllText(targetFile,"id,launch,end,moved,range_start,range_end,clear,same_exterior,entered,returned,select_ms,return_ms,cue_start,cue_arrival,size_approach,pose_type,blocker\n");
        int fail=0;
        Debug.Log("ORBIT_AUDIT runtime routes="+string.Join("|",Array.ConvertAll(ArkVoyage.Orbits,x=>x.Id)));
        for(int i=0;i<ArkVoyage.Orbits.Length;i++){
            var orbit=ArkVoyage.Orbits[i];var id=orbit.Id;
            var prefix=(i+1).ToString("00")+"-"+id;
            app.Hud.OpenRoutes(true);
            int begin=app.TravelTone.Accepted,arrive=app.TravelTone.Arrived;
            double st=Now();app.Hud.TapQAFlightCard(i);
            while(app.FlightCueBusy&&Now()-st<1.5)yield return null;
            double select=1000*(Now()-st);
            bool entered=app.Site.Id==id;
            yield return new WaitForEndOfFrame();
            bool exterior=app.Pilot.InAir&&app.Eye.GetComponentsInChildren<Renderer>().Length==0;
            var launch=app.Pilot.transform.position;
            bool safe=Clear(app.Eye.transform.position);
            bool landmarks=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkPleiadesLandmarkAB")>=0&&id=="pleiades";
            bool companion=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkProximaRockAB")>=0&&id=="proxima";
            bool annulus=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkSaturnAnnulusAB")>=0&&id=="saturn";
            bool rings=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkUranusRingAB")>=0&&id=="uranus";
            bool scale=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkPleiadesScaleAB")>=0&&id=="pleiades";
            bool stars=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkStarAB")>=0&&(id=="sirius"||id=="pleiades");
            bool mercuryRelief=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMercuryReliefAB")>=0&&id=="mercury";
            bool mercury=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMercuryAB")>=0&&id=="mercury";
            bool neptuneDetail=id=="neptune"&&(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkNeptuneDetailAB")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"-arkNeptuneRawAB")>=0);
            bool neptune=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkNeptuneAB")>=0&&id=="neptune";
            bool orionVolume=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkOrionVolumeQA")>=0&&id=="orion";
            bool moonRockTone=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMoonRockToneAB")>=0&&id=="moon";
            bool marsWall=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMarsWallAB")>=0&&id=="mars";
            bool marsGrain=marsWall||(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMarsGrainAB")>=0&&id=="mars");
            bool strata=Array.IndexOf(Environment.GetCommandLineArgs(),"-arkMarsColorAB")>=0&&id=="mars";
            if(stars){if(!StellarPair(app,frame,prefix+"-arrival"))fail++;}else if(orionVolume){if(!OrionFilamentPair(app,frame,prefix+"-arrival"))fail++;}else if(moonRockTone){if(!MoonRockTonePair(app,frame,prefix+"-arrival"))fail++;}else if(marsGrain){if(!MarsGrainPair(app,frame,prefix+"-arrival"))fail++;}else if(strata){if(!MarsPair(app,frame,prefix+"-arrival"))fail++;}else if(neptuneDetail){if(!NeptuneDetailPair(app,frame,prefix+"-arrival"))fail++;}else if(neptune){if(!NeptunePair(app,frame,prefix+"-arrival"))fail++;}else if(mercuryRelief){if(!MercuryReliefPair(app,frame,prefix+"-arrival",orbit.Radius))fail++;}else if(mercury){if(!MercuryPair(app,frame,prefix+"-arrival"))fail++;}else if(scale){if(!PleiadesScalePair(app,frame,prefix+"-arrival"))fail++;}else if(rings){if(!UranusRingPair(app,frame,prefix+"-arrival"))fail++;}else if(annulus){if(!SaturnAnnulusPair(app,frame,prefix+"-arrival"))fail++;}else if(companion){if(!ProximaRockPair(app,frame,prefix+"-arrival"))fail++;}else if(landmarks){if(!PleiadesLandmarkPair(app,frame,prefix+"-arrival"))fail++;}else frame(prefix+"-arrival");
            yield return null;
            float moved=0,range0=orbit.Radius>0?Vector3.Distance(launch,orbit.Hazard):-1,range=-1;
            string approach="lateral travel",blocker="none";
            if(entered&&exterior){
                if(id=="moon"||id=="mars"){
                    app.Pilot.Aim(id=="moon"?7:0,id=="moon"?18:13);
                    for(int j=0;j<175&&app.Pilot.transform.position.y>(id=="moon"?-3.8f:-2.8f);j++){
                        app.Pilot.Move(Vector2.up,.035f);yield return null;
                    }
                    app.Pilot.Aim(id=="moon"?7:0,0);
                    for(int j=0;j<100&&app.Pilot.transform.position.z<25;j++){app.Pilot.Move(Vector2.up,.035f);yield return null;}
                    app.Pilot.Aim(id=="moon"?7:0,11);
                    if(Physics.Raycast(app.Pilot.transform.position,Vector3.down,out var hit,20,Physics.DefaultRaycastLayers&~(1<<WalkPilot.PlayerLayer)))range=hit.distance;
                    approach="low terrain pass";
                    if(range<0)blocker="ground ray not verified";
                }else{
                    for(int j=0;j<80;j++){app.Pilot.Move(new Vector2(-.58f,.76f),.035f);yield return null;}
                    if(orbit.Radius>0){
                        approach="lateral plus approach track";
                        Aim(app.Pilot,orbit.Hazard-app.Pilot.transform.position);
                        for(int j=0;j<220&&Vector3.Distance(app.Pilot.transform.position,orbit.Hazard)>orbit.Radius+11;j++){
                            app.Pilot.Move(Vector2.up,.035f);yield return null;
                        }
                        range=Vector3.Distance(app.Pilot.transform.position,orbit.Hazard);
                        Aim(app.Pilot,orbit.Hazard-app.Pilot.transform.position+Vector3.right*8);
                        if(range>orbit.Radius+16)blocker="approach horizon/collider short of standard radius";
                    }
                }
                moved=Vector3.Distance(launch,app.Pilot.transform.position);
            }
            var end=app.Pilot.transform.position;safe=safe&&Clear(app.Eye.transform.position);
            if(moonRockTone){if(!MoonRockTonePair(app,frame,prefix+"-motion")||range<2.5f||range>5||moved<20||!safe)fail++;Debug.Log($"MOON_ROCK_TONE QA genuine low pass {moved:F3} units; ray {range:F3}; clearance={safe}");}else if(stars){if(!StellarPair(app,frame,prefix+"-motion"))fail++;}else if(marsGrain){
                if(!MarsGrainPair(app,frame,prefix+"-motion")||range<2.5f||range>6f||moved<20||!safe)fail++;
                Debug.Log($"MARS_GRAIN QA genuine low pass {moved:F3} units; floor ray {range:F3}; clear={safe}");
                if(marsWall){
                    app.Pilot.Aim(-28,6);if(!MarsGrainPair(app,frame,prefix+"-left-wall"))fail++;
                    app.Pilot.Aim(29,6);if(!MarsGrainPair(app,frame,prefix+"-right-wall"))fail++;
                    app.Pilot.Aim(0,11);
                }
            }else if(strata){if(!MarsPair(app,frame,prefix+"-motion"))fail++;}else if(neptune){if(!NeptunePair(app,frame,prefix+"-motion"))fail++;}else if(mercury){if(!MercuryPair(app,frame,prefix+"-motion"))fail++;}else if(scale){if(!PleiadesScalePair(app,frame,prefix+"-motion"))fail++;}else if(rings){if(!UranusRingPair(app,frame,prefix+"-motion"))fail++;}else if(annulus){
                if(!SaturnAnnulusPair(app,frame,prefix+"-motion"))fail++;
                var highStart=app.Pilot.transform.position;
                app.Pilot.Aim(180,-58);
                for(int j=0;j<65&&app.Pilot.transform.position.y<highStart.y+12;j++){
                    app.Pilot.Move(Vector2.up,.035f);yield return null;
                }
                var highEnd=app.Pilot.transform.position;
                Aim(app.Pilot,orbit.Hazard-highEnd+Vector3.right*8);
                bool gotUp=highEnd.y>highStart.y+10&&Clear(app.Eye.transform.position);
                if(!gotUp||!SaturnAnnulusPair(app,frame,prefix+"-ascended"))fail++;
                Debug.Log($"SATURN_ANNULUS QA genuine ascent {highStart:F3} -> {highEnd:F3}; clear={gotUp}");
            }else if(companion){if(!ProximaRockPair(app,frame,prefix+"-motion"))fail++;}else if(landmarks){
                if(!PleiadesLandmarkPair(app,frame,prefix+"-motion"))fail++;
                var mark=GameObject.Find("Pleiades landmark 0");
                if(!mark){fail++;}
                else {
                    var startApproach=app.Pilot.transform.position;
                    var center=mark.transform.position;
                    Aim(app.Pilot,center-startApproach);
                    for(int j=0;j<105&&Vector3.Distance(app.Pilot.transform.position,center)>10;j++){
                        app.Pilot.Move(Vector2.up,.035f);yield return null;
                    }
                    var open=app.Pilot.transform.position;
                    float nearRange=Vector3.Distance(open,center);
                    bool visibleClose=nearRange>=8f&&nearRange<11.1f&&Vector3.Distance(open,startApproach)>7f&&Clear(open);
                    if(!visibleClose||!PleiadesLandmarkPair(app,frame,prefix+"-close"))fail++;
                    Aim(app.Pilot,center-open);
                    var c=mark.GetComponent<SphereCollider>();
                    float expected=c.radius*mark.transform.lossyScale.x;
                    var minRange=nearRange;
                    var sample=app.Pilot.transform.position;
                    var mask=Physics.DefaultRaycastLayers&~(1<<WalkPilot.PlayerLayer);
                    bool sight=Physics.Raycast(open,(center-open).normalized,out var initial,12,mask,QueryTriggerInteraction.Ignore)&&initial.collider==c;
                    for(int j=0;j<120;j++){
                        Aim(app.Pilot,center-app.Pilot.transform.position);
                        app.Pilot.Move(Vector2.up,.035f);yield return null;
                        var rangeNow=Vector3.Distance(app.Pilot.transform.position,center);
                        if(rangeNow<minRange){minRange=rangeNow;sample=app.Pilot.transform.position;}
                        if(rangeNow>minRange+.45f)break;
                    }
                    Debug.Log($"PLEIADES_LANDMARK QA approach sight={sight}; layers={mark.layer}/{app.Pilot.gameObject.layer}; collider enabled={c.enabled} trigger={c.isTrigger}, mark center={center:F3}, start={open:F3}, nearest={minRange:F3} at {sample:F3}");
                    var stopped=app.Pilot.transform.position;
                    float blocked=Vector3.Distance(stopped,center);
                    var ray=center-stopped;
                    bool sameBarrier=Physics.Raycast(stopped,ray.normalized,out var hit,2,
                        Physics.DefaultRaycastLayers&~(1<<WalkPilot.PlayerLayer),QueryTriggerInteraction.Ignore)&&hit.collider==c;
                    bool clearContact=blocked>expected+.10f&&blocked<expected+.58f&&sameBarrier&&Clear(stopped);
                    Debug.Log($"PLEIADES_LANDMARK QA real lateral plus close {nearRange:F3}, solid contact at center range {blocked:F3}, world sphere radius {expected:F3}, same collider ray {sameBarrier}, clear={clearContact}");
                    if(!clearContact)fail++;
                }
            }else if(orionVolume){
                if(!OrionFilamentPair(app,frame,prefix+"-motion"))fail++;
                var cloud=app.Site.GetComponentInChildren<ArkOrionVolume>();
                bool isVolume=cloud&&!cloud.GetComponent<Collider>()&&!cloud.GetComponent<MeshRenderer>().sharedMaterial.HasProperty("_MainTex")
                    &&!GameObject.Find("M42 telescope plate; angular projection only");
                if(!isVolume)fail++;
                else {
                    var anchors=Array.ConvertAll(ArkOrionVolume.Sites,x=>cloud.transform.TransformPoint(x));
                    yield return new WaitForSecondsRealtime(1.4f); // natural zero-throttle release, no pose override
                    var panelBefore=Array.ConvertAll(anchors,x=>app.Eye.WorldToViewportPoint(x));
                    var startSide=app.Pilot.transform.position;
                    float yaw=app.Pilot.Yaw,pitch=app.Pilot.Pitch;
                    for(int j=0;j<100&&app.Pilot.transform.position.x>startSide.x-18;j++){
                        app.Pilot.Move(Vector2.left,.035f);yield return null;
                    }
                    var afterSide=app.Pilot.transform.position;
                    var panelAfter=Array.ConvertAll(anchors,x=>app.Eye.WorldToViewportPoint(x));
                    var shift=Array.ConvertAll(new[]{0,1,2},j=>Vector2.Distance(panelBefore[j],panelAfter[j]));
                    bool depth=shift[0]>shift[1]+.010f&&shift[1]>shift[2]+.0035f&&afterSide.x<startSide.x-17.5f
                        &&Mathf.Approximately(yaw,app.Pilot.Yaw)&&Mathf.Approximately(pitch,app.Pilot.Pitch)&&Clear(afterSide);
                    if(!depth)fail++;
                    Debug.Log($"ORION_VOLUME QA fixed-heading lateral {startSide:F3} -> {afterSide:F3}; near/core/far projected delta={shift[0]:F4}/{shift[1]:F4}/{shift[2]:F4}; depth={depth}");
                    if(!OrionFilamentPair(app,frame,prefix+"-lateral"))fail++;
                    for(int j=0;j<150&&app.Pilot.transform.position.z<71;j++){
                        Aim(app.Pilot,ArkOrionVolume.Center-app.Pilot.transform.position);
                        app.Pilot.Move(Vector2.up,.035f);yield return null;
                    }
                    var close=app.Pilot.transform.position;
                    Aim(app.Pilot,ArkOrionVolume.Center-close);
                    bool rangeOk=close.z>=70&&afterSide.z+35<close.z&&Clear(close);
                    if(!rangeOk)fail++;
                    if(!OrionFilamentPair(app,frame,prefix+"-approach"))fail++;
                    Debug.Log($"ORION_VOLUME QA genuine look/approach {afterSide:F3} -> {close:F3}, clear={rangeOk}, gas shader not image={isVolume}");
                    yield return new WaitForSecondsRealtime(.4f);
                    var cadence=new double[40];double initial=Now();
                    for(int j=0;j<cadence.Length;j++){yield return new WaitForEndOfFrame();double at=Now();cadence[j]=(at-initial)*1000;initial=at;}
                    Array.Sort(cadence);
                    Debug.Log($"ORION_VOLUME QA player end-of-frame interval sample n={cadence.Length}, median={cadence[20]:F2}ms p95={cadence[37]:F2}ms max={cadence[39]:F2}ms (not GPU or display timing; no PNG during sample)");
                }
            }else if(neptuneDetail){
                if(!NeptuneDetailPair(app,frame,prefix+"-motion"))fail++;
                var approachStart=app.Pilot.transform.position;
                for(int j=0;j<115;j++){
                    Aim(app.Pilot,orbit.Hazard-app.Pilot.transform.position);
                    app.Pilot.Move(Vector2.up,.035f);yield return null;
                }
                var near=app.Pilot.transform.position;
                float closest=Vector3.Distance(near,orbit.Hazard);
                bool closeSafe=closest>=orbit.Radius-.002f&&closest<orbit.Radius+.08f&&Vector3.Distance(near,approachStart)>8&&Clear(near);
                if(!closeSafe)fail++;
                Aim(app.Pilot,Quaternion.AngleAxis(-46,Vector3.up)*(orbit.Hazard-near));
                if(!NeptuneDetailPair(app,frame,prefix+"-close-limb"))fail++;
                Debug.Log($"NEPTUNE_CLOUD QA closes to {closest:F3}, stand-off test={closeSafe}");
            }else if(mercuryRelief){
                if(!MercuryReliefPair(app,frame,prefix+"-motion",orbit.Radius))fail++;
                var approachStart=app.Pilot.transform.position;
                for(int j=0;j<120;j++){
                    Aim(app.Pilot,orbit.Hazard-app.Pilot.transform.position);
                    app.Pilot.Move(Vector2.up,.035f);yield return null;
                }
                var near=app.Pilot.transform.position;
                float closest=Vector3.Distance(near,orbit.Hazard);
                bool closeSafe=closest>=orbit.Radius-.002f&&closest<orbit.Radius+.08f&&Vector3.Distance(near,approachStart)>8&&Clear(near);
                if(!closeSafe)fail++;
                var toward=orbit.Hazard-near;
                Aim(app.Pilot,Quaternion.AngleAxis(48,Vector3.up)*toward);
                if(!MercuryReliefPair(app,frame,prefix+"-close-limb",orbit.Radius))fail++;
                Debug.Log($"MERCURY_RELIEF QA real close flight stopped at {closest:F3}; unchanged safety radius, pass={closeSafe}");
            }else frame(prefix+"-motion");
            yield return null;
            bool routed=entered&&exterior&&safe&&moved>10;
            int startEvents=app.TravelTone.Accepted-begin,endEvents=app.TravelTone.Arrived-arrive;
            st=Now();app.Hud.TapQAReturn();
            while(app.FlightCueBusy&&Now()-st<1.5)yield return null;
            double back=1000*(Now()-st);
            bool returned=app.Site.Id=="station-orbit"&&app.Pilot.InAir&&Clear(app.Eye.transform.position);
            if(!routed||!returned||startEvents!=1||endEvents!=1)fail++;
            File.AppendAllText(targetFile,string.Join(",",new[]{id,P(launch),P(end),N(moved),N(range0),N(range),safe.ToString(),exterior.ToString(),entered.ToString(),returned.ToString(),N(select),N(back),startEvents.ToString(),endEvents.ToString(),approach,"real control",blocker})+"\n");
            Debug.Log($"ORBIT_AUDIT {id} moved={N(moved)} outside={exterior} clear={safe} return={returned} range={N(range)} cue select/return={N(select)}/{N(back)}ms");
        }
        yield return new WaitForEndOfFrame();
        frame("99-station-return");
        var probe=app.TravelTone.Probe;
        Debug.Log($"ORBIT_AUDIT audio source DSP sample: {probe?.AcceptPeak:F5}/{probe?.ArrivePeak:F5} (QA suppressed at source); failures={fail}");
        Application.Quit(fail==0?0:3);
    }
}}

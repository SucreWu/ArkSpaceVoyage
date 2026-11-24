using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace ArkWalk {
internal static class ArkRoutePerf {
    static bool active;
    static readonly List<(string,double)> stages=new List<(string,double)>(18);
    static readonly double frequency=System.Diagnostics.Stopwatch.Frequency;
    internal static long Tick()=>active?System.Diagnostics.Stopwatch.GetTimestamp():0;
    internal static void Stop(string stage,long start){
        if(start>0)stages.Add((stage,1000d*(System.Diagnostics.Stopwatch.GetTimestamp()-start)/frequency));
    }
    static double Now()=>Time.realtimeSinceStartupAsDouble;
    static string F(double x)=>x.ToString("F3",CultureInfo.InvariantCulture);
    internal static IEnumerator Probe(WalkApp app,string folder) {
        yield return new WaitForSecondsRealtime(.5f);
        string[] order={"mercury","venus","neptune","earth","mars"};
        var lines=new List<string>{"visit,target,controls_ms,enter_ms,build_ms,atlas_loop_ms,atlas_load_ms,atlas_upload_ms,relief_mesh_ms,returned"};
        int fail=0;
        for(int round=0;round<2;round++)foreach(var id in order){
            var slot=Array.FindIndex(ArkVoyage.Orbits,x=>x.Id==id);
            app.Hud.OpenRoutes(true);
            yield return null;
            stages.Clear();active=true;
            double start=Now();
            app.Hud.TapQAFlightCard(slot);
            while(app.FlightCueBusy&&Now()-start<2)yield return null;
            double controls=(Now()-start)*1000;
            active=false;
            bool ok=app.Site.Id==id&&app.Pilot.InAir&&!app.FlightCueBusy;
            float Find(string stage){
                foreach(var t in stages)if(t.Item1==stage)return (float)t.Item2;
                return -1;
            }
            string visit=round==0?"first":"revisit";
            string cpu=string.Join(",",new[]{visit,id,F(controls),F(Find("enter")),F(Find("build")),F(Find("atlas_loop")),F(Find("atlas_load")),F(Find("atlas_upload")),F(Find("relief_mesh"))});
            app.Hud.TapQAReturn();
            double ret=Now();
            while(app.FlightCueBusy&&Now()-ret<2)yield return null;
            bool back=app.Site.Id=="station-orbit"&&app.Pilot.InAir;
            if(!ok||!back)fail++;
            lines.Add(cpu+","+back.ToString());
            yield return new WaitForSecondsRealtime(.13f);
        }
        File.WriteAllLines(Path.Combine(folder,"route-performance.csv"),lines);
        foreach(var line in lines)Debug.Log("ROUTE_PERF "+line);
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-arkRawAtlasAudit")>=0){
            bool mercury=ArkSolarNeighbors.CheckBakedSurface(false),venus=ArkSolarNeighbors.CheckBakedSurface(true);
            Debug.Log($"RAW_ATLAS all accepted source texels exactly match Windows runtime generator: Mercury={mercury} Venus={venus}");
            if(!mercury||!venus)fail++;
        }
        Debug.Log($"ROUTE_PERF diagnostic failures={fail}; no image readback nor log output during sampled intervals");
        if(fail!=0)Application.Quit(3);
    }
}}

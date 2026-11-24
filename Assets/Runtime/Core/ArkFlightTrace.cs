using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ArkWalk {
internal static class ArkFlightTrace {
    static double Ms()=>System.Diagnostics.Stopwatch.GetTimestamp()*1000d/System.Diagnostics.Stopwatch.Frequency;
    static void Sample(string key,WalkApp app) {
        Debug.Log($"FLIGHT_TRACE {key} frame={Time.frameCount} utc={DateTimeOffset.UtcNow:O} unity={Time.realtimeSinceStartupAsDouble:F4} site={app.Site.Id} pose={app.Pilot.transform.position:F4} yaw={app.Pilot.Yaw:F2} pitch={app.Pilot.Pitch:F2} cruise={app.Pilot.Cruise} modal={app.Hud.Modal} exterior={app.Eye.GetComponentsInChildren<Renderer>().Length==0}");
    }
    static void ScreenWitness(string folder,string name){
        var tex=ScreenCapture.CaptureScreenshotAsTexture();
        try{File.WriteAllBytes(Path.Combine(folder,name+".png"),tex.EncodeToPNG());}
        finally{UnityEngine.Object.Destroy(tex);}
    }
    static IEnumerator Cadence(string label,int count) {
        var values=new List<double>();double last=Ms();
        for(int i=0;i<count;i++){yield return new WaitForEndOfFrame();double now=Ms();values.Add(now-last);last=now;}
        values.Sort();
        Debug.Log($"FLIGHT_TRACE cadence {label} n={count} p50={values[count/2]:F2}ms p95={values[(int)((count-1)*.95)]:F2}ms max={values[count-1]:F2}ms delayed50ms={values.FindAll(v=>v>50).Count}");
    }
    internal static IEnumerator Run(WalkApp app,string dir) {
        yield return new WaitForEndOfFrame();
        Sample("startup_end_of_frame",app);
        yield return new WaitForSecondsRealtime(.65f);
        app.Pilot.Aim(23,-7);app.Pilot.ToggleCruise();
        var initial=app.Pilot.transform.position;
        yield return Cadence("station_moving",90);
        var outside=app.Pilot.transform.position;
        Sample("station_after_cruise",app);
        bool ok=Vector3.Distance(initial,outside)>2;
        double openStart=Ms();
        app.Hud.OpenRoutes(false);
        double openCpu=Ms()-openStart;
        yield return new WaitForEndOfFrame();
        Debug.Log($"FLIGHT_TRACE open_chart sync={openCpu:F2}ms to_end_of_frame={Ms()-openStart:F2}ms");
        Sample("chart_first_end_of_frame",app);
        // Snapshot the HUD outside any measured route selection.
        ScreenWitness(dir,"A-Actual-Chart");
        yield return Cadence("chart_idle",45);
        var launch=app.Pilot.transform.position;
        Debug.Log($"FLIGHT_TRACE modal_translation={Vector3.Distance(outside,launch):F5}");
        int index=Array.FindIndex(ArkVoyage.Orbits,x=>x.Id=="earth");
        yield return null;
        Sample("pre_select",app);
        var sw=Ms();
        app.Hud.TapQAFlightCard(index);
        var sync=Ms()-sw;
        var firstPose=app.Pilot.transform.position;
        bool earth=app.Site.Id=="earth"&&!app.Hud.Modal&&app.Pilot.InAir;
        yield return new WaitForEndOfFrame();
        Debug.Log($"FLIGHT_TRACE select_to_Earth synchronous={sync:F2}ms to_end_of_frame={Ms()-sw:F2}ms");
        Sample("earth_first_end_of_frame",app);
        yield return Cadence("earth_idle_after_arrival",60);
        var still=app.Pilot.transform.position;
        double steer=Ms();app.Pilot.Move(Vector2.right,.035f);
        Debug.Log($"FLIGHT_TRACE steer_Earth direct_move_call={Ms()-steer:F3}ms displacement={Vector3.Distance(still,app.Pilot.transform.position):F4} idle_drift={Vector3.Distance(firstPose,still):F5}");
        yield return new WaitForEndOfFrame();
        ScreenWitness(dir,"B-Actual-Earth");
        yield return new WaitForSecondsRealtime(.4f);
        yield return null;
        Sample("pre_return",app);
        sw=Ms();app.ReturnToDock();sync=Ms()-sw;
        bool station=app.Site.Id=="station-orbit"&&app.Pilot.InAir;
        yield return new WaitForEndOfFrame();
        Debug.Log($"FLIGHT_TRACE return_to_station synchronous={sync:F2}ms to_end_of_frame={Ms()-sw:F2}ms");
        Sample("returned_first_end_of_frame",app);
        yield return Cadence("station_after_return",60);
        ScreenWitness(dir,"C-Actual-Returned");
        var clear=!Physics.CheckSphere(app.Eye.transform.position,.25f,Physics.DefaultRaycastLayers&~(1<<WalkPilot.PlayerLayer));
        Debug.Log($"FLIGHT_TRACE RESULT moved={ok} earth={earth} return={station} clear={clear}");
        Application.Quit(ok&&earth&&station&&clear?0:3);
    }
}}

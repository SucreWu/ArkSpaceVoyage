using System;
using System.IO;
using UnityEngine;
namespace ArkWalk
{
    [Serializable] internal sealed class ArkPose
    {
        public int schema;
        public string site;
        public Vector3 point;
        public float yaw, pitch;
        public bool Valid()=>schema==1&&(!string.IsNullOrWhiteSpace(site)&&ArkVoyage.IsOrbit(site))
          && Finite(point.x)&&Finite(point.y)&&Finite(point.z)&&point.sqrMagnitude<=30000f
          && Finite(yaw)&&yaw>=0&&yaw<=360&&Finite(pitch)&&pitch>=-78&&pitch<=75;
        private static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
    }
    internal static class ArkCheckpoint
    {
        private static string Location()
        {
            var qa=Environment.GetEnvironmentVariable("ARK_QA_SAVE");
            if(!string.IsNullOrWhiteSpace(qa))
            {
                qa=Path.GetFullPath(qa);
                if(!Path.GetFileName(qa.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)).StartsWith("QA-",StringComparison.Ordinal))
                    throw new InvalidOperationException("ARK_QA_SAVE requires isolated QA-* directory");
            }
            return Path.Combine(string.IsNullOrWhiteSpace(qa)?Application.persistentDataPath:qa,"space-route-v1.json");
        }
        internal static ArkPose Read()
        {
            try
            {
                string p=Location();
                foreach(string candidate in new[]{p,p+".bak"})
                {
                    try {if(!File.Exists(candidate))continue;var v=JsonUtility.FromJson<ArkPose>(File.ReadAllText(candidate));if(v!=null&&v.Valid())return v;}
                    catch(Exception e) {Debug.LogWarning("ARK_CHECKPOINT_READ "+e.GetType().Name);}
                }
            }
            catch(Exception e){Debug.LogWarning("ARK_CHECKPOINT_PATH "+e.GetType().Name);}
            return null;
        }
        internal static bool Write(ArkPose v)
        {
            if(v==null||!v.Valid())return false;
            try
            {
                string p=Location();Directory.CreateDirectory(Path.GetDirectoryName(p));File.WriteAllText(p+".new",JsonUtility.ToJson(v));
                if(File.Exists(p))File.Replace(p+".new",p,p+".bak");else File.Move(p+".new",p);
                return true;
            }
            catch(Exception e){Debug.LogWarning("ARK_CHECKPOINT_WRITE "+e.GetType().Name);return false;}
        }
    }
    internal static class ArkSaveWitness
    {
        internal static void Run(WalkApp app,string mode)
        {
            if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ARK_QA_SAVE"))){Application.Quit(3);return;}
            if(mode=="seed"||mode=="seed-earth"||mode=="seed-mars"||mode=="seed-mercury"||mode=="seed-venus"||mode=="seed-uranus"||mode=="seed-neptune"||mode=="seed-sirius"||mode=="seed-pleiades")
            {
                app.Go(mode=="seed"?"jupiter":mode.Substring(5));var start=app.Pilot.transform.position;
                for(int i=0;i<100;i++)app.Pilot.Move(new Vector2(.2f,1f),.05f);
                bool ok=Vector3.Distance(start,app.Pilot.transform.position)>15&&app.SaveRoute();
                Debug.Log("ARK_SAVE_WITNESS "+(ok?"PASS seed ":"FAIL seed ")+app.Site.Id+" "+app.Pilot.transform.position.ToString("F4"));
                Application.Quit(ok?0:3);return;
            }
            bool valid=mode=="fallback"?app.Site.Id=="station-orbit"&&!app.FromCheckpoint:app.Site.Id==mode&&app.FromCheckpoint;
            Debug.Log("ARK_SAVE_WITNESS "+(valid?"PASS ":"FAIL ")+mode+" "+app.Site.Id+" "+app.Pilot.transform.position.ToString("F4"));
            Application.Quit(valid?0:3);
        }
    }
}

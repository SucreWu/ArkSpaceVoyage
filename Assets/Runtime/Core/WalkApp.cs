using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk
{
    public sealed class WalkApp : MonoBehaviour
    {
        public static readonly string[] Places = { "home", "commons", "ecology", "clinic", "dock", "bridge", "moon", "jupiter", "saturn", "proxima", "annex", "earth", "mars", "mercury", "venus", "uranus", "neptune", "sirius", "pleiades", "station-orbit", "orion" };
        public static readonly string[] Names = { "住宅街巷", "学校与公园", "晨林生态环", "医疗与冷冻舱", "穿梭机船坞", "远航舰桥", "月球轨道", "木星云带", "土星冰环", "比邻星邻域", "中庭·训练与实验翼", "地球近轨", "火星峡谷", "水星盆地", "金星云顶", "天王星侧环", "海王星风暴", "天狼双星", "昴星团", "近地空间站", "猎户座大星云" };
        public WalkSite Site { get; private set; }
        public WalkPilot Pilot { get; private set; }
        public WalkHud Hud { get; private set; }
        public Camera Eye => Pilot.Eye;
        public AudioSource Narrator { get; private set; }
        public bool Flying => Site && ArkVoyage.IsOrbit(Site.Id);
        public string Title => Names[Array.IndexOf(Places, Site.Id)];
        private readonly Dictionary<string, Pose> visits = new Dictionary<string, Pose>();
        private bool saveSuppressed;
        private Pose? dockReturn;
        internal bool FromCheckpoint { get; private set; }
        private string lastSpeech;
        private float travelAfter;
        private int stopIndex;
        private WalkMark back;
        private WalkMark next;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Boot()
        {
            if (FindAnyObjectByType<WalkApp>()) return;
            new GameObject("世代舰漫游").AddComponent<WalkApp>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.shadowDistance = 54;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            Input.simulateMouseWithTouches = false;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Narrator = gameObject.AddComponent<AudioSource>();
            Narrator.playOnAwake = false;
            Pilot = new GameObject("参观者").AddComponent<WalkPilot>();
            Pilot.transform.SetParent(transform, false);
            Pilot.Init(this);
            TravelTone=gameObject.AddComponent<ArkTravelTone>(); TravelTone.Setup();
            Hud = gameObject.AddComponent<WalkHud>();
            Hud.Init(this);
            var args = Environment.GetCommandLineArgs();
            saveSuppressed=Array.IndexOf(args,"-arkVerify")>=0;
            int saveCheck=Array.IndexOf(args,"-arkCheckSave");
            var saved=saveSuppressed?null:ArkCheckpoint.Read();
            FromCheckpoint=saved!=null;
            Go(saved?.site??"station-orbit");
            if(saved!=null)Pilot.Restore(saved.point,saved.yaw,saved.pitch);
            if(saveCheck>=0&&saveCheck+1<args.Length)ArkSaveWitness.Run(this,args[saveCheck+1]);
            int verify = Array.IndexOf(args, "-arkVerify");
            if (verify >= 0 && verify + 1 < args.Length) gameObject.AddComponent<ArkSpaceCheck>().Begin(this, args[verify + 1]);
        }

        ArkExteriorCutCue cutCue;
        internal ArkTravelTone TravelTone {get;private set;}
        bool cueReady;
        internal bool FlightCueBusy {get;private set;}
        internal float FlightCueStrength=>cutCue?cutCue.Strength:0;
        internal bool RequestFlight(string id){
            if(FlightCueBusy || !Flying || !ArkVoyage.IsOrbit(id) || Site.Id==id)return false;
            if(!cutCue){
                cutCue=Eye.gameObject.AddComponent<ArkExteriorCutCue>();
                cueReady=cutCue.Setup();
                if(!cueReady)cutCue.enabled=false;
            }
            Hud.Clear();
            Pilot.ResetInput();
            if(!cueReady){Go(id);return true;}
            FlightCueBusy=true;
            StartCoroutine(FlightCut(id));
            return true;
        }
        System.Collections.IEnumerator FlightCut(string id) {
            cutCue.Strength=.04f;cutCue.enabled=true;
            TravelTone.Play(false);
            const float outTime=.105f,inTime=.13f;
            double start=Time.realtimeSinceStartupAsDouble;
            try {
                while(Time.realtimeSinceStartupAsDouble-start<outTime){
                    float t=(float)((Time.realtimeSinceStartupAsDouble-start)/outTime);
                    cutCue.Strength=Mathf.Lerp(.04f,.87f,Mathf.SmoothStep(0,1,t));yield return null;
                }
                cutCue.Strength=.87f;
                Go(id);
                if(Site.Id==id)TravelTone.Play(true);
                double landed=Time.realtimeSinceStartupAsDouble;
                while(Time.realtimeSinceStartupAsDouble-landed<inTime){
                    float t=(float)((Time.realtimeSinceStartupAsDouble-landed)/inTime);
                    cutCue.Strength=.87f*(1-Mathf.SmoothStep(0,1,t));yield return null;
                }
            } finally {
                cutCue.Strength=0;cutCue.enabled=false;FlightCueBusy=false;
            }
        }
        public void Go(string id) { Enter(id, 0); }

        public void ReturnToDock(){if(Flying)Go("station-orbit");}
        internal bool SaveRoute()
        {
            if(saveSuppressed||!Site||!Pilot)return false;
            return ArkCheckpoint.Write(new ArkPose{schema=1,site=Site.Id,point=Pilot.CheckpointFeet,yaw=Pilot.Yaw,pitch=Pilot.Pitch});
        }
        private void OnApplicationQuit(){SaveRoute();}
        private void OnApplicationPause(bool paused){if(paused)SaveRoute();}

        private void Enter(string id, int direction)
        {
            long perfEnter=ArkRoutePerf.Tick();
            int index = Array.IndexOf(Places, id);
            if (index < 0 || !ArkVoyage.IsOrbit(id)) return;
            if (Site)
            {
                if(Site.Id=="dock"&&ArkVoyage.IsOrbit(id))
                {
                    var seat=Site.Marks.Find(m=>m.Title.Contains("驾驶台"));
                    if(seat&&Vector3.Distance(Pilot.transform.position,seat.Point)<3f)
                        dockReturn=new Pose(Pilot.transform.position,Quaternion.Euler(Pilot.Pitch,Pilot.Yaw,0));
                }
                if(Flying&&id=="dock"&&dockReturn.HasValue)visits["dock"]=dockReturn.Value;
                visits[Site.Id] = new Pose(Pilot.transform.position, Quaternion.Euler(Pilot.Pitch, Pilot.Yaw, 0));
                Site.gameObject.SetActive(false);
                Destroy(Site.gameObject);
            }
            Narrator.Stop();
            Narrator.clip = null;
            lastSpeech = null;
            Hud.Clear();
            RenderSettings.skybox = null;
            RenderSettings.sun = null;
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.69f, .77f, .86f);
            RenderSettings.ambientEquatorColor = new Color(.44f, .47f, .53f);
            RenderSettings.ambientGroundColor = new Color(.29f, .25f, .23f);
            Eye.clearFlags = CameraClearFlags.Skybox;
            Eye.backgroundColor = index < 6 ? new Color(.63f,.73f,.79f) : new Color(.012f,.018f,.046f);
            Site = new GameObject(Names[index]).AddComponent<WalkSite>();
            Site.transform.SetParent(transform, false);
            long perfBuild=ArkRoutePerf.Tick();
            Site.Open(this, id);
            ArkRoutePerf.Stop("build",perfBuild);
            stopIndex = index;
            back = null;
            next = null;
            Vector3 at = Site.Spawn;
            float yaw = Site.Yaw;
            float pitch = Site.Pitch;
            if (index < 6)
            {
                var facing = Quaternion.Euler(0, Site.Yaw, 0) * Vector3.forward;
                var backFoot = Site.Spawn - facing * 1.95f;
                back = Portal(backFoot, (index + 5) % 6, -1);
                next = Portal(Site.Exit, (index + 1) % 6, 1);
                if (direction < 0)
                {
                    var towards = (Site.Spawn - Site.Exit);
                    towards.y = 0;
                    towards.Normalize();
                    at = Site.Exit + towards * 1.8f;
                    yaw = Quaternion.LookRotation(towards).eulerAngles.y;
                    pitch = 0;
                }
                else if (direction == 0 && visits.TryGetValue(id, out var stored))
                {
                    at = stored.position;
                    yaw = stored.rotation.eulerAngles.y;
                    pitch = Mathf.DeltaAngle(0, stored.rotation.eulerAngles.x);
                }
            }
            Pilot.Place(at, yaw, pitch, ArkVoyage.IsOrbit(id));
            Physics.SyncTransforms();
            travelAfter = Time.time + 1.2f;
            Hud.Arrived();
            ArkRoutePerf.Stop("enter",perfEnter);
        }

        private WalkMark Portal(Vector3 foot, int to, int direction)
        {
            var sign = Site.Mark(foot + Vector3.up * 1.12f, .68f, "通往" + Names[to], "", () => Enter(Places[to], direction)).GetComponent<WalkMark>();
            sign.Portal = true;
            return sign;
        }

        private void LateUpdate() { CheckGate(); }

        internal void CheckGate()
        {
            if (Flying || !Site || Site.Id=="annex" || Hud.Modal || Time.time < travelAfter || !Pilot.IsWalking) return;
            Vector3 feet = Pilot.transform.position;
            if (AtGate(feet, next)) Enter(Places[(stopIndex + 1) % 6], 1);
            else if (AtGate(feet, back)) Enter(Places[(stopIndex + 5) % 6], -1);
        }

        private static bool AtGate(Vector3 feet, WalkMark gate)
        {
            if (!gate) return false;
            Vector3 offset = feet + Vector3.up * 1.12f - gate.Point;
            return Mathf.Abs(offset.y) < 1.1f && new Vector2(offset.x, offset.z).sqrMagnitude < .82f * .82f;
        }

        public bool Visible(WalkMark mark)
        {
            Vector3 delta = mark.Point - Eye.transform.position;
            float distance = delta.magnitude;
            if (distance > (Flying ? 480f : 15f)) return false;
            if (Vector3.Dot(delta, Eye.transform.forward) <= 0) return false;
            float allowance = Mathf.Max(.34f, mark.Radius * 1.18f);
            return !Physics.Raycast(Eye.transform.position, delta.normalized, Mathf.Max(0, distance - allowance),
                Physics.DefaultRaycastLayers & ~(1 << WalkPilot.PlayerLayer), QueryTriggerInteraction.Ignore);
        }

        public WalkMark Pick(Vector2 screenPoint)
        {
            if (Hud.Modal || !Site) return null;
            Ray ray = Eye.ScreenPointToRay(screenPoint);
            WalkMark best = null;
            float bestScore = float.MaxValue;
            foreach (var mark in Site.Marks)
            {
                if (!mark || !mark.isActiveAndEnabled) continue;
                Vector3 delta = mark.Point - ray.origin;
                float along = Vector3.Dot(ray.direction, delta);
                if (along <= 0) continue;
                float radius = Mathf.Max(mark.Radius, along * .018f);
                float miss = (delta - ray.direction * along).magnitude / radius;
                if (miss > 1f) continue;
                float score = miss + Mathf.Min(.3f, along * .008f) - (mark.Paper ? .4f : 0);
                if (score >= bestScore || !Visible(mark)) continue;
                bestScore = score;
                best = mark;
            }
            if (best) Activate(best);
            return best;
        }

        private void Activate(WalkMark mark)
        {
            if (!string.IsNullOrEmpty(mark.Speech)) Tell(mark.Title, mark.Speech);
            mark.OnTap?.Invoke();
        }

        public void Tell(string heading, string words)
        {
            if (string.IsNullOrEmpty(words)) return;
            lastSpeech = words;
            // Observation cards are text-only in the standalone voyage.
            Hud.ShowCaption(heading, words, Narrator.clip ? Narrator.clip.length : 4f);
        }

        public static string VoiceId(string words)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(words)), 0, 10).Replace("-", "").ToLowerInvariant();
        }

        public void Replay()
        {
            if (string.IsNullOrEmpty(lastSpeech)) return;
            Narrator.Stop();
            Narrator.clip = null;
            if (Narrator.clip) Narrator.Play();
            Hud.ExtendCaption(Narrator.clip ? Narrator.clip.length : 4f);
        }
    }
}

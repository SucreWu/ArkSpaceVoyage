using UnityEngine;

namespace ArkWalk
{
    public sealed class WalkPilot : MonoBehaviour
    {
        public const int PlayerLayer = 8;
        public Camera Eye { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool Cruise { get; private set; }
        public bool IsWalking { get; private set; }
        public bool InAir { get; private set; }
        public Vector2 Stick { get; private set; }
        public Vector2 StickOrigin { get; private set; }
        public bool UsingStick => walkFinger >= 0;
        private WalkApp app;
        private CharacterController body;
        private GameObject canopy;
        private int walkFinger = -1;
        private int lookFinger = -1;
        private bool lookDragged;
        private Vector2 lookOrigin;
        private Vector2 dragStart;
        private Vector2 lastMouse;
        private bool mouseUi;
        private bool mouseDragged;
        private float verticalSpeed;
        internal Vector3 CheckpointFeet => InAir||body.isGrounded?transform.position:safeFeet;
        internal void Restore(Vector3 point,float yaw,float pitch)
        {
            bool flying=InAir;
            Place(flying?point:point-Vector3.up*.035f,yaw,pitch,flying);
            if(flying)flightOrigin=app.Site.Spawn;
        }
        private Vector3 flightOrigin;
        private float flightSpeed;
        private Vector3 safeFeet;
        private float eyeHeight = 1.48f;
        private bool watchPaper;
        private Vector3 paperFocus;

        internal void Init(WalkApp owner)
        {
            app = owner;
            gameObject.layer = PlayerLayer;
            body = gameObject.AddComponent<CharacterController>();
            body.skinWidth = .025f;
            body.minMoveDistance = 0;
            body.slopeLimit = 52f;
            Eye = new GameObject("孩子的视线").AddComponent<Camera>();
            Eye.tag = "MainCamera";
            Eye.transform.SetParent(transform, false);
            Eye.nearClipPlane = .035f;
            Eye.farClipPlane = 12000f;
            Eye.allowMSAA = true;
            Eye.gameObject.AddComponent<AudioListener>();
            // Free-view camera: no cockpit geometry.
        }

        public void Place(Vector3 at, float yaw, float pitch, bool flying)
        {
            body.enabled = false;
            InAir = flying;
            Cruise = false;
            flightSpeed = 0;
            flightOrigin = at;
            safeFeet = at;
            watchPaper = false;
            eyeHeight = 1.48f;
            verticalSpeed = -1f;
            ResetInput();
            body.stepOffset = 0;
            body.radius = flying ? .3f : .22f;
            body.height = flying ? .64f : 1.58f;
            body.center = flying ? Vector3.zero : Vector3.up * .79f;
            body.stepOffset = flying ? 0 : .24f;
            transform.position = at + (flying ? Vector3.zero : Vector3.up * .035f);
            Eye.transform.localPosition = flying ? Vector3.zero : Vector3.up * 1.48f;
            Aim(yaw, pitch);
            Eye.fieldOfView = flying ? 62 : 54;
            // No vehicle visual attached to the camera.
            body.enabled = true;
        }

        public void Aim(float yaw, float pitch)
        {
            Yaw = Mathf.Repeat(yaw, 360f);
            Pitch = Mathf.Clamp(pitch, -78, 75);
            transform.rotation = Quaternion.Euler(0, Yaw, 0);
            Eye.transform.localRotation = Quaternion.Euler(Pitch, 0, 0);
        }

        public void ToggleCruise() { if (InAir) Cruise = !Cruise; }

        internal void LookAtPaper()
        {
            var person = app.Site.Marks.Find(m => m.Title == "阿禾与纸星");
            if (!person) return;
            paperFocus = person.Point;
            watchPaper = true;
        }

        internal void StandUp() { watchPaper = false; }

        public void ResetInput()
        {
            walkFinger = -1;
            lookFinger = -1;
            Stick = Vector2.zero;
            IsWalking = false;
            mouseUi = false;
            mouseDragged = false;
        }

        private void OnApplicationFocus(bool focus) { if (!focus) ResetInput(); }

        private void Update()
        {
            if (!app.Hud || !app.Site || app.FlightCueBusy) return;
            if (Input.GetKeyDown(KeyCode.Escape)) app.Hud.Back();
            if (Input.GetKeyDown(KeyCode.M)) app.Hud.OpenRoutes(false);
            if (Input.GetKeyDown(KeyCode.H)) app.Hud.ToggleHints();
            if (Input.GetKeyDown(KeyCode.F5)) app.SaveRoute();
            if (Input.GetKeyDown(KeyCode.F2)) app.Hud.Clean = !app.Hud.Clean;
            Touches();
            if (Input.touchCount == 0) Mouse();
            if (app.Hud.Modal) { IsWalking = false; return; }
            float strafe = Key(KeyCode.D, KeyCode.A);
            float walk = Key(KeyCode.W, KeyCode.S) + Key(KeyCode.UpArrow, KeyCode.DownArrow);
            float turn = Key(KeyCode.RightArrow, KeyCode.LeftArrow);
            if (turn != 0) Aim(Yaw + turn * 72 * Time.deltaTime, Pitch);
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
                app.Pick(new Vector2(Screen.width, Screen.height) * .5f);
            Move(Vector2.ClampMagnitude(new Vector2(strafe, walk) + Stick, 1f), Time.deltaTime);
        }

        private static float Key(KeyCode positive, KeyCode negative)
        {
            return (Input.GetKey(positive) ? 1 : 0) - (Input.GetKey(negative) ? 1 : 0);
        }

        public void Move(Vector2 intent, float elapsed)
        {
            if (elapsed <= 0) return;
            if (InAir)
            {
                float throttle = Mathf.Abs(intent.y) > .04f ? intent.y * 12f : Cruise ? 4.3f : 0;
                flightSpeed = Mathf.MoveTowards(flightSpeed, throttle, elapsed * 9);
                var velocity = Eye.transform.forward * flightSpeed + transform.right * (intent.x * 6f);
                Vector3 to = transform.position + velocity * elapsed;
                var bounded=ArkVoyage.Bound(app.Site.Id,flightOrigin,transform.position,to);
                body.Move(bounded-transform.position);
                return;
            }
            IsWalking = intent.sqrMagnitude > .01f;
            if (IsWalking) watchPaper = false;
            float desiredHeight = watchPaper ? 1.33f : 1.48f;
            if (!Mathf.Approximately(eyeHeight,desiredHeight))
            {
                eyeHeight = Mathf.MoveTowards(eyeHeight,desiredHeight,elapsed * .31f);
                Eye.transform.localPosition = Vector3.up * eyeHeight;
                if (watchPaper)
                {
                    Vector3 angles = Quaternion.LookRotation(paperFocus-Eye.transform.position).eulerAngles;
                    Aim(Yaw,Mathf.MoveTowardsAngle(Pitch,Mathf.DeltaAngle(0,angles.x),elapsed*48));
                }
            }
            if (body.isGrounded)
            {
                safeFeet = transform.position;
                verticalSpeed = -1.5f;
            }
            verticalSpeed = Mathf.Max(-16f, verticalSpeed - 13f * elapsed);
            var move = (transform.forward * intent.y + transform.right * intent.x) * 2.45f;
            body.Move((move + Vector3.up * verticalSpeed) * elapsed);
            if (transform.position.y < safeFeet.y - 6) Place(app.Site.Spawn, app.Site.Yaw, app.Site.Pitch, false);
        }

        private float DragThreshold => Mathf.Max(6, Screen.height * .009f);

        private void Mouse()
        {
            Vector2 pos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                dragStart = lastMouse = pos;
                mouseDragged = false;
                mouseUi = app.Hud.Catches(pos);
            }
            if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !mouseUi && !app.Hud.Modal)
            {
                mouseDragged |= (pos - dragStart).magnitude > DragThreshold;
                if (mouseDragged) Look(pos - lastMouse);
                lastMouse = pos;
            }
            if (Input.GetMouseButtonUp(0) && !mouseDragged && (pos - dragStart).magnitude < DragThreshold)
            {
                if (mouseUi) app.Hud.Tap(pos);
                else if (!app.Hud.Modal) TapScene(pos);
            }
        }

        private void Look(Vector2 delta)
        {
            if (delta.sqrMagnitude > 1f) watchPaper = false;
            float gain = 95f / Mathf.Max(1, Screen.height);
            Aim(Yaw + delta.x * gain, Pitch - delta.y * gain);
        }

        private void TapScene(Vector2 point)
        {
            if (!app.Pick(point) && app.Hud.Clean) app.Hud.Clean = false;
        }

        private void Touches()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began)
                {
                    if (!app.Hud.Catches(t.position) && !app.Hud.Modal && app.Hud.OnPad(t.position) && walkFinger < 0)
                    {
                        walkFinger = t.fingerId;
                        StickOrigin = t.position;
                        Stick = Vector2.zero;
                    }
                    else if (lookFinger < 0)
                    {
                        lookFinger = t.fingerId;
                        lookOrigin = t.position;
                        lookDragged = false;
                        mouseUi = app.Hud.Catches(t.position);
                    }
                }
                else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                {
                    if (walkFinger == t.fingerId)
                        Stick = Vector2.ClampMagnitude((t.position - StickOrigin) / Mathf.Max(35, Screen.height * .095f), 1);
                    else if (lookFinger == t.fingerId && !mouseUi && !app.Hud.Modal)
                    {
                        lookDragged |= (t.position - lookOrigin).magnitude > DragThreshold;
                        if (lookDragged) Look(t.deltaPosition);
                    }
                }
                else
                {
                    if (walkFinger == t.fingerId)
                    {
                        walkFinger = -1;
                        Stick = Vector2.zero;
                    }
                    if (lookFinger != t.fingerId) continue;
                    lookFinger = -1;
                    if (t.phase != TouchPhase.Ended || lookDragged || (lookOrigin - t.position).magnitude >= DragThreshold) continue;
                    if (mouseUi) app.Hud.Tap(t.position);
                    else if (!app.Hud.Modal) TapScene(t.position);
                }
            }
        }
    }

}

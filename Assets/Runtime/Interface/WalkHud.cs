using System;
using UnityEngine;

namespace ArkWalk
{
    public sealed class WalkHud : MonoBehaviour
    {
        private enum Page { None, Decks, Sky }
        private Page page;
        private WalkApp app;
        private Font font;
        private GUIStyle type;
        private string caption = "";
        private string speaker = "";
        private float until;
        private float hints;
        private float welcome;
        private bool first = true;
        public bool Modal => page != Page.None;
        public bool Clean;

        private static readonly Color Ink = new Color(.053f,.105f,.13f,.92f);
        private static readonly Color Card = new Color(.09f,.16f,.185f,.96f);
        private static readonly Color Cream = new Color(.98f,.964f,.89f);
        private static readonly Color Pale = new Color(.76f,.84f,.81f);
        private static readonly Color Brass = new Color(.94f,.75f,.43f);
        private static readonly string[] Details = { "看看阿禾手里的纸星", "去树下，听一听校园", "看看水怎样流回森林", "打开一扇科学小窗", "从尾舱走进驾驶室", "站在舷窗前看航图", "越过明暗交界的环形山", "沿着云带观察风暴", "飞近一块明亮的冰", "看红矮星照亮岩岸", "", "沿太阳翼飞向蓝色地球", "穿行于赤色峡谷岩壁之间", "绕飞撞击盆地，远眺太阳", "沿探针看见金色云纹", "沿侧倾窄环掠飞", "越过海卫一看风暴", "距地球约 8.6 光年", "距地球约 440 光年", "", "哈勃实拍 · 天球投影" };

        public void Init(WalkApp owner)
        {
            app = owner;
            font = Resources.Load<Font>("Interface/NotoSansSC-Regular");
            if (!font) font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC", "Droid Sans Fallback" }, 22);
        }

        private Rect Safe => Screen.safeArea.width > 0 ? Screen.safeArea : new Rect(0,0,Screen.width,Screen.height);
        private float Scale => Mathf.Max(.32f, Mathf.Min(Safe.height / 645f, Safe.width / 875f));
        private float W => Safe.width / Scale;
        private float H => Safe.height / Scale;
        private Vector2 ToGui(Vector2 screen) => new Vector2((screen.x - Safe.xMin) / Scale, (Safe.yMax - screen.y) / Scale);
        private Rect MapButton => new Rect(W-134, 16, 116, 44);
        private Rect HintButton => new Rect(W-254, 16, 108, 44);
        private Rect CruiseButton => new Rect(W-230, H-66, 117, 48);
        private Rect ReturnButton => new Rect(W-103, H-66, 85, 48);
        private Rect Panel => new Rect((W-680)/2,(H-470)/2,680,470);
        private Rect CaptionBox => new Rect((W-550)/2,H-(app.Flying ? 182 : 132),550,110);
        private Rect RepeatButton => new Rect(CaptionBox.xMax-82,CaptionBox.y+7,70,35);
        private Rect Jump(int column) => new Rect(Panel.x + 28 + 134*column, Panel.y+83,123,40);
        private Rect Close => new Rect(Panel.xMax-81, Panel.y+21,59,39);
        private Rect ViewOnly => new Rect(Panel.x+28,Panel.yMax-49,175,34);
        private Rect Slot(int i)
        {
            if(page!=Page.Sky)return new Rect(Panel.x+28+(i%2)*315,Panel.y+149+(i/2)*88,305,72);
            var id=ArkVoyage.Orbits[i].Id;
            int solar=System.Array.IndexOf(SolarOrder,id);
            if(solar>=0)return new Rect(Panel.x+28+(solar%5)*126,Panel.y+143+(solar/5)*58,119,52);
            return new Rect(Panel.x+28+(id=="sirius"||id=="orion"?158:0),Panel.y+(id=="pleiades"||id=="orion"?355:279),150,52);
        }
        static string SkySolarNote(string id) => id switch {
            "mercury"=>"撞击盆地","venus"=>"硫酸云顶","earth"=>"蓝色海洋","moon"=>"月面环形山","mars"=>"赤色峡谷",
            "jupiter"=>"气态云带","saturn"=>"冰环群","uranus"=>"侧倾窄环","neptune"=>"风暴与海卫一",_=>""
        };
        static readonly string[] SolarOrder={"mercury","venus","earth","moon","mars","jupiter","saturn","uranus","neptune"};
        private bool CaptionOpen => Time.unscaledTime < until && caption.Length > 0;

        public void Clear() { page = Page.None; caption = ""; speaker = ""; until = 0; }
        public void Arrived()
        {
            if (first) { welcome = Time.unscaledTime + 9; first = false; }
            else welcome = 0;
        }
        public void OpenRoutes(bool flight)
        {
            if(app.FlightCueBusy)return;
            Clean = false;
            page = Page.Sky;
            app.Pilot.ResetInput();
        }
        public void Back()
        {
            if(app.FlightCueBusy)return;
            if (Modal) page = Page.None;
            else if (CaptionOpen) until = 0;
            else OpenRoutes(false);
        }
        public void ToggleHints() { hints = Time.unscaledTime < hints ? 0 : Time.unscaledTime + 10; }
        public void ShowCaption(string title, string speech, float duration)
        {
            speaker = title;
            caption = speech;
            ExtendCaption(duration);
        }
        public void ExtendCaption(float duration) { until = Time.unscaledTime + Mathf.Max(5, duration+1.5f); }
        public bool OnPad(Vector2 point)
        {
            Vector2 p = ToGui(point);
            return p.x < 200 && p.y > H-206;
        }
        public bool Catches(Vector2 screen)
        {
            if(app.FlightCueBusy)return true;
            if (Clean) return false;
            if (Modal) return true;
            Vector2 p = ToGui(screen);
            return MapButton.Contains(p) || HintButton.Contains(p) || (CaptionOpen && CaptionBox.Contains(p))
                || (app.Flying && (CruiseButton.Contains(p) || ReturnButton.Contains(p)));
        }
        internal void TapQAFlightCard(int ordinal)
        {
            if(page!=Page.Sky)throw new System.InvalidOperationException("Expected actual flight modal");
            var c=Slot(ordinal).center;
            Tap(new Vector2(Safe.xMin+c.x*Scale,Safe.yMax-c.y*Scale));
        }
        internal void TapQAReturn(){var c=ReturnButton.center;Tap(new Vector2(Safe.xMin+c.x*Scale,Safe.yMax-c.y*Scale));}
        public void Tap(Vector2 screen)
        {
            if(app.FlightCueBusy)return;
            Vector2 p = ToGui(screen);
            if (Modal)
            {
                if (!Panel.Contains(p) || Close.Contains(p)) { page = Page.None; return; }
                // No deck tab.
                if (Jump(1).Contains(p)) { page = Page.Sky; return; }
                if (ViewOnly.Contains(p)) { page = Page.None; Clean = true; return; }
                int count = page == Page.Sky ? ArkVoyage.Orbits.Length : 6;
                for (int i = 0; i < count; i++)
                {
                    if (!Slot(i).Contains(p)) continue;
                    int index = page == Page.Sky ? System.Array.IndexOf(WalkApp.Places, ArkVoyage.Orbits[i].Id) : i;
                    app.RequestFlight(WalkApp.Places[index]);
                    return;
                }
                return;
            }
            if (MapButton.Contains(p)) OpenRoutes(false);
            else if (HintButton.Contains(p)) ToggleHints();
            // No audio replay.
            else if (app.Flying && CruiseButton.Contains(p)) app.Pilot.ToggleCruise();
            else if (app.Flying && ReturnButton.Contains(p)) app.RequestFlight("station-orbit");
        }

        private void OnGUI()
        {
            if (Clean || !app || !app.Site || Event.current.type != EventType.Repaint) return;
            if (type == null) type = new GUIStyle(GUI.skin.label) { font = font, wordWrap = true, padding = new RectOffset(0,0,0,0) };
            var before = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Safe.xMin, Screen.height-Safe.yMax,0), Quaternion.identity, Vector3.one * Scale);
            if (Modal) DrawMap();
            else DrawView();
            GUI.matrix = before;
        }
        private void Fill(Rect r, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(r,Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        private void Label(Rect r, string value, float size, Color color, TextAnchor align = TextAnchor.MiddleLeft)
        {
            type.alignment = align;
            type.fontSize = Mathf.RoundToInt(size);
            type.normal.textColor = color;
            GUI.Label(r,value,type);
        }
        private void Button(Rect r, string value, bool active = false)
        {
            Fill(r, active ? Brass : Card);
            if (!active) Fill(new Rect(r.x,r.yMax-1,r.width,1), new Color(.55f,.62f,.59f,.72f));
            Label(r, value, 17, active ? Ink : Cream, TextAnchor.MiddleCenter);
        }
        private void DrawView()
        {
            float width = 44 + app.Title.Length * 20;
            var tag = new Rect(18, 16, width,44);
            Fill(tag, Ink);
            Fill(new Rect(tag.x,tag.y+10,3,24),Brass);
            Label(new Rect(34,16,width-18,44),app.Title,20,Cream);
            Button(HintButton,Time.unscaledTime < hints ? "收起提示" : "看看哪里");
            Button(MapButton,"星图（M）");
            if (Time.unscaledTime < hints || !app.Flying) DrawLabels();
            if (Input.touchSupported)
            {
                var center = app.Pilot.UsingStick ? ToGui(app.Pilot.StickOrigin) : new Vector2(88,H-88);
                var thumb = center + Vector2.Scale(app.Pilot.Stick,new Vector2(40,-40));
                GUI.color = new Color(1,1,1,.30f);
                Circle(center,46);
                GUI.color = new Color(1,1,1,.55f);
                Circle(thumb,13);
                GUI.color = Color.white;
            }
            if (app.Flying)
            {
                Button(CruiseButton,app.Pilot.Cruise ? "结束巡航" : "平稳巡航",app.Pilot.Cruise);
                Button(ReturnButton,"回空间站");
            }
            if(app.Site.Id=="orion")
            {
                var credit=new Rect(18,H-108,Mathf.Min(W-260,770),79);Fill(credit,Ink);
                bool drawn=app.Site.GetComponentInChildren<ArkOrionVolume>();
                if(drawn)Label(new Rect(credit.x+8,credit.y+4,credit.width-16,25),"M42 · 程序绘制的三维星云，形态与色彩为艺术演绎",15,Cream);
                else                 Label(new Rect(credit.x+8,credit.y+4,credit.width-16,25),"猎户座大星云 · 哈勃观测合成，二维视向，不代表可飞进的云层",15,Cream);
                Label(new Rect(credit.x+8,credit.y+31,credit.width-16,43),"NASA, ESA, M. Robberto (Space Telescope Science Institute/ESA) and the Hubble Space Telescope Orion Treasury Project Team · CC BY 4.0",14,Cream);
            }
            if (CaptionOpen)
            {
                var r = CaptionBox;
                Fill(r,Ink);
                Fill(new Rect(r.x,r.y,4,r.height),Brass);
                Label(new Rect(r.x+18,r.y+7,r.width-120,32),speaker,18,Brass);
                Label(new Rect(r.x+18,r.y+43,r.width-36,58),caption,19,Cream);
                // Text-only observations.
            }
            else if (Time.unscaledTime < welcome)
            {
                var r = new Rect((W-444)/2,H-64,444,42);
                Fill(r, Ink);
                Label(r, Input.touchSupported ? "左手飞行 · 滑动转向 · 点击看观测信息" : "WASD 飞行 · 拖动转向 · M 星图 · F2 纯观测",17,Cream,TextAnchor.MiddleCenter);
            }
        }
        private Texture2D disk;
        private void Circle(Vector2 point,int radius)
        {
            if (!disk)
            {
                const int size = 96;
                disk = new Texture2D(size,size,TextureFormat.RGBA32,false);
                var pixels = new Color32[size*size];
                for (int y=0;y<size;y++)
                for (int x=0;x<size;x++)
                {
                    float d = Vector2.Distance(new Vector2(x+.5f,y+.5f),Vector2.one*(size*.5f));
                    float alpha = Mathf.Clamp01(size*.5f-d) * Mathf.Lerp(.22f,1,Mathf.InverseLerp(size*.45f,size*.47f,d));
                    pixels[x+y*size] = new Color(1,1,1,alpha);
                }
                disk.SetPixels32(pixels);
                disk.Apply(false,true);
            }
            GUI.DrawTexture(new Rect(point.x-radius,point.y-radius,radius*2,radius*2),disk);
        }
        private void DrawLabels()
        {
            bool hintsOpen = Time.unscaledTime < hints;
            foreach (var mark in app.Site.Marks)
            {
                if (!mark || !mark.isActiveAndEnabled) continue;
                if (!hintsOpen && !mark.Paper && !(mark.Portal && Vector3.Distance(mark.Point,app.Eye.transform.position)<5f)) continue;
                if (!app.Visible(mark)) continue;
                var projected = app.Eye.WorldToScreenPoint(mark.Point);
                if (projected.z < 0 || !Safe.Contains(projected)) continue;
                var p = ToGui(projected);
                float w = mark.Title.Length * 16 + 22;
                var r = new Rect(p.x-w/2,p.y-44,w,30);
                if (r.y < 68 || p.y > H-50) continue;
                Fill(r,Ink);
                Label(r,mark.Title,15,Cream,TextAnchor.MiddleCenter);
                Fill(new Rect(p.x-1,p.y-14,2,11),Brass);
            }
        }
        private void DrawMap()
        {
            Fill(new Rect(0,0,W,H),new Color(.015f,.041f,.055f,.64f));
            var r = Panel;
            Fill(r,Ink);
            Fill(new Rect(r.x,r.y,r.width,3),Brass);
            Label(new Rect(r.x+28,r.y+15,r.width-150,55),"星际观测航路",30,Cream);
            Button(Close,"收起");
            Label(Jump(0),app.Site.Id=="station-orbit"?"第一站：地球":"任意回访",17,Brass);
            Label(Jump(1),"位置 / 距离示意",14,Pale);
            if(page==Page.Sky){
                Label(new Rect(r.x+28,r.y+116,600,24),"太阳系",16,Brass);
                Label(new Rect(r.x+28,r.y+252,600,24),"近邻恒星 · 按距地球远近",16,Brass);
                Label(new Rect(r.x+28,r.y+329,600,24),"深空 · 银河系内",16,Brass);
            }
            int count = page == Page.Sky ? ArkVoyage.Orbits.Length : 6;
            for (int i=0;i<count;i++)
            {
                int n = page == Page.Sky ? System.Array.IndexOf(WalkApp.Places, ArkVoyage.Orbits[i].Id) : i;
                Rect tile = Slot(i);
                Fill(tile,Card);
                if (WalkApp.Places[n] == app.Site.Id) Fill(new Rect(tile.x,tile.y,3,tile.height),Brass);
                Label(new Rect(tile.x+15,tile.y+4,tile.width-30,32),WalkApp.Names[n],page==Page.Sky ? 17 : 21,Cream);
                Label(new Rect(tile.x+15,tile.y+(page==Page.Sky ? 29 : 38),tile.width-30,25),(page==Page.Sky && tile.width<130 ? SkySolarNote(WalkApp.Places[n]) : (page==Page.Sky && WalkApp.Places[n]=="proxima" ? "距地球约 4.24 光年" : Details[n])),page==Page.Sky ? 12 : 15,Pale);
            }
            Button(ViewOnly,"收起界面看风景");
            Label(new Rect(ViewOnly.xMax+14,ViewOnly.y,300,ViewOnly.height),"轻点空处可恢复按钮",15,Pale);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

// All geometry lives above the anchor plane. Does not take input, block movement or access the network.
public sealed class ArkLifeStation : MonoBehaviour {
    public enum Kind { Xenobiology, Cryogenics, Navigator }
    public Kind Type {get; private set;}
    static readonly Dictionary<string,Transform> rooms = new Dictionary<string,Transform>();
    static readonly Dictionary<Kind,ArkLifeStation> stations = new Dictionary<Kind,ArkLifeStation>();
    readonly Dictionary<Color,Material> paints = new Dictionary<Color,Material>();
    Transform body, sailA, sailB, cover, face, surface, pointer;
    Transform[] lights = new Transform[5];
    float phase, opening, stimulus;
    bool amber, wantOpen, night;
    int topic;
    string dest;
    public Transform Destination { get { Transform t; return dest!=null && rooms.TryGetValue(dest,out t) ? t:null; } }
    public static void RegisterRoom(string id, Transform point){if(!string.IsNullOrEmpty(id))rooms[id]=point;}
    public static ArkLifeStation Install(Transform anchor, Kind kind){
        var s=new GameObject("ArkLife_"+kind).AddComponent<ArkLifeStation>();
        s.transform.SetParent(anchor,false); s.Type=kind; s.Build(); stations[kind]=s; return s;
    }
    static readonly Color navy=new Color(.065f,.11f,.17f), cyan=new Color(.22f,.83f,.85f), gold=new Color(1f,.64f,.28f), pearl=new Color(.73f,.82f,.85f), coral=new Color(.81f,.38f,.47f);
    Transform Node(string n,Transform p,Vector3 v){var t=new GameObject(n).transform;t.SetParent(p,false);t.localPosition=v;return t;}
    Transform Part(string n,PrimitiveType shape,Transform p,float x,float y,float z,float a,float b,float c,Color color){
        var go=GameObject.CreatePrimitive(shape);go.name=n;var col=go.GetComponent<Collider>();col.enabled=false;Destroy(col);
        var t=go.transform;t.SetParent(p,false);t.localPosition=new Vector3(x,y,z);t.localScale=new Vector3(a,b,c);
        Material m;if(!paints.TryGetValue(color,out m)){m=new Material(Shader.Find("Standard"));m.color=color;m.SetFloat("_Glossiness",.55f);paints[color]=m;}
        go.GetComponent<Renderer>().sharedMaterial=m;return t;
    }
    void Build(){
        var t=transform;var box=PrimitiveType.Cube;var egg=PrimitiveType.Sphere;var drum=PrimitiveType.Cylinder;
        if(Type==Kind.Xenobiology){
            Part("潮汐观察池",drum,t,0,.11f,0,2,.11f,1.55f,navy);
            surface=Part("浅色反照池面",drum,t,0,.225f,0,1.81f,.012f,1.37f,cyan);
            body=Node("折帆游灵",t,new Vector3(0,.7f,0));
            Part("梭形浮囊",egg,body,0,0,0,.32f,.22f,.78f,pearl);
            for(int k=0;k<2;k++){
                int d=k==0?-1:1;var sail=Node("折扇",body,new Vector3(d*.1f,0,0));if(k==0)sailA=sail;else sailB=sail;
                for(int j=0;j<5;j++){var fin=Part("叶片",egg,sail,d*(.18f+j*.024f),.015f,j*.097f-.21f,.46f-j*.027f,.045f,.13f,j%2==0?coral:gold);fin.localRotation=Quaternion.Euler(0,d*(j*8-18),d*12);}
            }
            for(int i=0;i<5;i++)lights[i]=Part("光感珠",egg,body,0,.094f,i*.099f-.22f,.064f,.048f,.066f,gold);
            RegisterRoom("xeno",t);
        }else if(Type==Kind.Cryogenics){
            Part("绝热底座",box,t,0,.2f,0,1.05f,.4f,1.95f,navy);
            Part("空置模拟床",egg,t,0,.43f,0,.83f,.21f,1.72f,pearl);
            cover=Node("侧轴翻盖",t,new Vector3(-.48f,.48f,0));
            for(int i=0;i<7;i++)for(int j=0;j<8;j++){
                float a=(j+.5f)*Mathf.PI/8;
                var rib=Part("拱形隔热鳞",box,cover,.48f+.48f*Mathf.Cos(a),.39f*Mathf.Sin(a),i*.237f-.711f,.197f,.045f,.21f,(i==1||i==5)?cyan:pearl);
                rib.localEulerAngles=new Vector3(0,0,a*Mathf.Rad2Deg-90);
            }
            for(int i=0;i<5;i++)lights[i]=Part("模拟灯",box,t,.534f,.26f,i*.19f-.38f,.03f,.065f,.12f,cyan);
            RegisterRoom("cryo",t);
        }else{
            pointer=Node("舱室指向针",t,new Vector3(0,.29f,0));
            Part("针杆",box,pointer,0,0,.24f,.045f,.045f,.48f,gold);
            for(int i=0;i<2;i++)Part("箭头",box,pointer,(i==0?-1:1)*.052f,0,.40f,.04f,.05f,.17f,gold).localEulerAngles=new Vector3(0,(i==0?1:-1)*36,0);
            pointer.gameObject.SetActive(false);
            Part("投影底盘",drum,t,0,.12f,0,1.02f,.12f,1.02f,navy);
            body=Node("值班员投影",t,new Vector3(0,1,0));
            Part("肩部轮廓",egg,body,0,-.18f,0,.71f,.22f,.37f,cyan);
            Part("制服",egg,body,0,-.33f,0,.46f,.52f,.31f,navy);
            face=Part("面罩",egg,body,0,.12f,0,.42f,.5f,.37f,pearl);
            for(int i=0;i<2;i++)Part("双面显示带",box,body,0,.15f,(i==0?-1:1)*.175f,.29f,.055f,.026f,cyan);
        }
    }
    void Update(){
        float dt=Time.deltaTime;phase+=dt;stimulus=Mathf.Max(0,stimulus-dt);
        if(Type==Kind.Xenobiology && body){
            float beat=Mathf.Sin(phase*(stimulus>0?5:2));body.localPosition=new Vector3(Mathf.Sin(phase*.62f)*.32f,.68f+beat*.047f,Mathf.Cos(phase*.62f)*.18f);
            body.localRotation=Quaternion.Euler(0,phase*35,beat*6);sailA.localRotation=Quaternion.Euler(0,0,-beat*20);sailB.localRotation=Quaternion.Euler(0,0,beat*20);
            for(int i=0;i<5;i++)lights[i].gameObject.SetActive(!amber || Mathf.Sin(phase*4-i)>-.35f);
        }else if(Type==Kind.Cryogenics && cover){opening=Mathf.MoveTowards(opening,wantOpen?1:0,dt*.62f);cover.localRotation=Quaternion.Euler(0,0,opening*82);
            for(int i=0;i<5;i++)lights[i].gameObject.SetActive(!night || i==2);
        }else if(body){var target=Destination;pointer.gameObject.SetActive(target!=null);if(target){var d=transform.InverseTransformPoint(target.position)-pointer.localPosition; if(d.sqrMagnitude>.001f)pointer.localRotation=Quaternion.LookRotation(d,Mathf.Abs(d.normalized.y)>.99f?Vector3.forward:Vector3.up);}
            body.localPosition=new Vector3(0,1+Mathf.Sin(phase*1.4f)*.03f,0);body.localRotation=Quaternion.Euler(0,Mathf.Sin(phase*.6f)*13,0);}
    }
    public string Status {get{
        if(Type==Kind.Xenobiology)return amber?"琥珀光谱：背部五枚光感珠正在依次闪烁。":"日光光谱：光感珠常亮，折帆游灵绕池浮游。";
        if(Type==Kind.Cryogenics)return (opening<.01f?"舱盖闭合":opening>.99f?"舱盖打开":"舱盖移动中")+"，"+(night?"一盏夜航灯亮起":"五盏舷侧灯亮起")+"。空舱演示，无乘员。";
        return "离线问答 · 舰载值班员｜"+(Destination?"正在指向："+Destination.name:"请选择想了解的舱室。");
    }}
    public string[] Choices {get{
        if(Type==Kind.Xenobiology)return new[]{amber?"恢复日光":"切换琥珀光谱","轻敲观察池"};
        if(Type==Kind.Cryogenics)return new[]{wantOpen?"合上模拟舱盖":"打开模拟舱盖",night?"恢复工作灯":"切换夜航灯"};
        return topic==0?new[]{"观察外星生物","看看休眠舱"}:new[]{"读取舱室现况","指向这个舱室","换个主题"};
    }}
    public string Interact(int choice){
        if(choice<0||choice>=Choices.Length)return Status;
        if(Type==Kind.Xenobiology){if(choice==0){amber=!amber;surface.GetComponent<Renderer>().sharedMaterial=paints[amber?gold:cyan];}else{stimulus=4;return "池壁传来轻振。观察四秒：两侧折帆加快了扇动。";}return Status;}
        if(Type==Kind.Cryogenics){if(choice==0){wantOpen=!wantOpen;return wantOpen?"正在打开空置展示舱。":"正在合上展示舱。";}night=!night;return Status;}
        if(topic==0){topic=choice+1;dest=null;return topic==1?"折帆游灵靠两侧扇叶悬游。想看它此刻的光感反应，还是去池边观察？":"这是空置的休眠展示舱。你可以看舱盖如何沿侧轴翻起。想查看现况还是过去看看？";}
        if(choice==2){topic=0;dest=null;return Status;}
        if(choice==0){ArkLifeStation other;return stations.TryGetValue(topic==1?Kind.Xenobiology:Kind.Cryogenics,out other)&&other?other.Status:"该舱室尚未加载，没有可报告的现场状态。";}
        dest=topic==1?"xeno":"cryo";return Destination?(topic==1?"已指向异星观察池。":"已指向休眠展示舱。"):"该舱室尚未加载，目前不能定位。";
    }
    void OnDestroy(){foreach(var m in paints.Values)if(m)Destroy(m);}
}

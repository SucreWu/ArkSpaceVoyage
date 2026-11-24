using UnityEngine;
using System.Collections.Generic;

// Built-in RP. Adopt into a spare, empty area at floor height: ArkCabinWing.Build(anchor).
public sealed partial class ArkCabinWing : MonoBehaviour {
    public static ArkCabinWing Build(Transform parent) {
        var found=parent.Find("ArkCabinWing"); if(found!=null) return found.GetComponent<ArkCabinWing>();
        var root=new GameObject("ArkCabinWing");root.transform.SetParent(parent,false);
        var wing=root.AddComponent<ArkCabinWing>();wing.Construct();return wing;
    }
    readonly Dictionary<string,Material> palette=new Dictionary<string,Material>();
    Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    Material Paint(string hex) {
        if(palette.TryGetValue(hex,out var m)) return m;
        var shader=Shader.Find("Standard"); if(!shader) shader=Shader.Find("Diffuse");
        m=new Material(shader); ColorUtility.TryParseHtmlString("#"+hex.TrimStart('!'),out var c);m.color=c;
        if(hex.StartsWith("!") && m.HasProperty("_EmissionColor")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*.9f);}
        m.enableInstancing=true;palette[hex]=m;return m;
    }
    Transform Part(string n,PrimitiveType shape,Vector3 p,Vector3 s,string color,bool solid=false,Transform parent=null){
        var g=GameObject.CreatePrimitive(shape);g.name=n;g.transform.SetParent(parent?parent:transform,false);
        g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=Paint(color);
        if(!solid)g.GetComponent<Collider>().enabled=false;return g.transform;
    }
    Transform Box(string n,float x,float y,float z,float w,float h,float d,string c,bool hit=false)=>Part(n,PrimitiveType.Cube,V(x,y,z),V(w,h,d),c,hit);
    Transform Egg(string n,Vector3 p,Vector3 s,string c,Transform par=null)=>Part(n,PrimitiveType.Sphere,p,s,c,false,par);
    Transform Group(string n,Vector3 p){var t=new GameObject(n).transform;t.SetParent(transform,false);t.localPosition=p;return t;}
    void Strut(Vector3 a,Vector3 b,float r,string c){
        var t=Part("Formed rib",PrimitiveType.Cylinder,(a+b)/2,V(r,(b-a).magnitude/2,r),c);
        t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }
    void Sign(string words,Vector3 p,float yaw=0){
        var t=Group(words,p);t.localRotation=Quaternion.Euler(0,yaw,0);var m=t.gameObject.AddComponent<TextMesh>();
        m.text=words;m.anchor=TextAnchor.MiddleCenter;m.alignment=TextAlignment.Center;m.fontSize=54;m.characterSize=.062f;m.color=new Color(.98f,.91f,.74f);
    }
    void Lamp(Vector3 p,Color c,float range,float power){
        var l=Group("Local fill",p).gameObject.AddComponent<Light>();l.type=LightType.Point;l.color=c;l.range=range;l.intensity=power;l.shadows=LightShadows.None;
    }
    const string stone="B8A48A",brass="BC8653",frame="39474C",wood="996140",green="64834E",mint="!A1E8D6",purple="!CC8CF1";
    Transform canopy,rotor,flower; readonly Transform[] pads=new Transform[3]; readonly Renderer[] lamps=new Renderer[3];
    bool open,bloom,wasAtrium,wasLab;int steps;float spin;Camera eye;
    void Construct(){
        BuildAtriumHero();
        BuildAtriumThreshold();
        canopy=Group("Retractable sun sail",V(0,6.5f,15));
        for(int i=0;i<9;i++)Part("Linen pleat",PrimitiveType.Cube,V(0,Mathf.Abs(i-4)*.1f,i*.58f),V(10,.13f,.56f),i%2==0?"D5AA76":"E7C69A",false,canopy);
        Box("Sun sail brass treadle",0,.012f,17,1.8f,.024f,1.8f,"!F2B465");
        Sign("SUN SAIL  /  STEP",V(0,2.9f,21));
        Sign("01   /   GARDEN COURT",V(0,5,23.7f));
        Lamp(V(0,5,13),new Color(1,.78f,.49f),23,2.6f);
        // West: a long athletics lane. Three distinct walk-on hoops charge the flywheel.
        Box("Resilient gym track",-21,-.18f,12,24,.36f,9,"477678",true);
        Box("Track end",-33,2.6f,12,.3f,5.2f,9,"273F4C",true);
        foreach(int s in new[]{-1,1}){
            Box("Track wall",-21,2.6f,12+s*4.5f,24,5.2f,.3f,"354D59",true);
            Box("Pace line",-21,.012f,12+s*1.3f,22,.024f,.08f,"!EDB66A");
        }
        Box("Gym crown",-21,5.35f,12,24,.25f,9,"354D59");
        for(int i=0;i<3;i++){
            float x=-14-i*6; pads[i]=Box("Stride station "+(i+1),x,.02f,12,1.6f,.04f,2.45f,i==0?"!F6BF75":"A6BFC0");
            foreach(int s in new[]{-1,1}){
                Strut(V(x,0,12+s*2.3f),V(x,3.2f,12+s*1.85f),.2f,"DB8C5B");}
            lamps[i]=Box("Pace bar",x,3.2f,12,.25f,.18f,3.8f,"526771").GetComponent<Renderer>();
            Sign((i+1).ToString("00"),V(x,2.1f,14.7f),-90);
        }
        rotor=Group("Human powered flywheel",V(-31,2.55f,12));
        for(int i=0;i<8;i++){var t=Part("Flywheel vane",PrimitiveType.Cube,Vector3.zero,V(.22f,1.75f,.24f),i%2==0?mint:brass,false,rotor);t.localRotation=Quaternion.Euler(i*45,0,0);t.localPosition=t.localRotation*Vector3.up*.92f;}
        Sign("PACE  /  01 > 02 > 03",V(-32.5f,4.5f,12),-90);
        Lamp(V(-22,3.7f,12),new Color(.59f,.91f,1),19,2.3f);
        // East: four-sided research gallery around a radial, unfurling specimen.
        Box("Lab porcelain deck",17,-.18f,12,16,.36f,16,"ABACA3",true);
        foreach(int s in new[]{-1,1}){
            Box("Cabinet bulkhead",17,2.8f,12+s*8,16,5.6f,.32f,frame,true);
            Box("Return wall",9,2.8f,12+s*5,.3f,5.6f,6,frame,true);
            Box("Light trough",17,3.4f,12+s*7.78f,14,.09f,.07f,mint);
            Box("Archive bench",17,.62f,12+s*6.55f,10,1.24f,1.45f,"65777B",true);
            for(int i=0;i<6;i++){float x=12.8f+i*1.62f;Egg("Sample ampoule",V(x,1.62f,12+s*6.5f),V(.43f,.77f,.43f),i%2==0?"84B6AA":"BDA6BC");}
        }
        Box("Optical dark wall",25,2.8f,12,.3f,5.6f,16,frame,true);
        Box("Light canopy",17,5.7f,12,16,.24f,16,"667C80");
        Part("Culture island",PrimitiveType.Cylinder,V(18,.46f,12),V(4.3f,.46f,4.3f),brass,true);
        Part("Culture medium",PrimitiveType.Cylinder,V(18,.94f,12),V(4,.04f,4),"49697A");
        // Articulated sampling pipette: offset silhouette preserves the specimen sightline.
        Vector3 foot=V(19.25f,1.1f,12.95f), elbow=V(20,2.85f,13.25f), wrist=V(18.65f,3.32f,12.6f);
        Strut(foot,elbow,.23f,brass);Strut(elbow,wrist,.17f,brass);Strut(wrist,V(18.45f,2.55f,12.45f),.075f,mint);
        Egg("Silicone elbow",elbow,V(.37f,.37f,.37f),frame);Egg("Pipette joint",wrist,V(.25f,.25f,.25f),frame);
        flower=Group("Phototropic corolla",V(18,1.02f,12));
        for(int i=0;i<18;i++){float a=i*20*Mathf.Deg2Rad;var t=Egg("Veined petal",V(Mathf.Sin(a)*.75f,.45f,Mathf.Cos(a)*.75f),V(.33f,.22f,1.65f),i%2==0?purple:"!9076B1",flower);
            t.localRotation=Quaternion.Euler(-20,i*20,0);}
        Egg("Specimen core",V(0,.46f,0),V(.8f,.9f,.8f),"!F6C998",flower);
        flower.localScale=V(.5f,1,.5f);
        Box("Excitation footswitch",14.5f,.018f,12,1.7f,.036f,1.7f,mint);
        Sign("LIGHT CULTURE  /  STEP",V(19,3.2f,12),90);
        Lamp(V(18,4,12),new Color(.82f,.76f,1),16,2.4f);
    }
    bool Inside(Vector3 p,float x,float z,float dx,float dz)=>Mathf.Abs(p.x-x)<dx && Mathf.Abs(p.z-z)<dz && p.y>-.2f && p.y<3.6f;
    void Update(){
        if(!canopy)return;if(!eye||!eye.isActiveAndEnabled)eye=Camera.main;if(!eye)return;
        Vector3 p=transform.InverseTransformPoint(eye.transform.position);bool a=Inside(p,0,17,.9f,.9f),l=Inside(p,14.5f,12,.85f,.85f);
        if(a&&!wasAtrium)open=!open;if(l&&!wasLab)bloom=!bloom;wasAtrium=a;wasLab=l;
        float dt=Time.deltaTime;canopy.localScale=Vector3.MoveTowards(canopy.localScale,V(1,1,open?.12f:1f),dt);
        flower.localScale=Vector3.MoveTowards(flower.localScale,bloom?V(1.28f,.72f,1.28f):V(.5f,1,.5f),dt*.8f);
        if(steps<3 && Inside(p,pads[steps].localPosition.x,12,.8f,1.25f)){lamps[steps++].sharedMaterial=Paint(mint);spin=steps*55;}
        if(steps==3 && Inside(p,-14,12,.8f,1.25f)){foreach(var r in lamps)r.sharedMaterial=Paint("526771");steps=0;spin=0;}
        rotor.Rotate(Vector3.right,dt*spin,Space.Self);
    }
    void OnDestroy(){ReleaseAtriumHero();foreach(var m in palette.Values) if(m)Destroy(m);}
}


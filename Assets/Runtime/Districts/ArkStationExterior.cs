using System.Collections.Generic;
using UnityEngine;

// Pure render geometry. Local +Z = aft. No colliders, controls, camera, or global light.
public sealed class ArkStationExterior : MonoBehaviour {
    sealed class Piece {
        public readonly List<Vector3> xyz = new List<Vector3>();
        public readonly List<Vector3> normals = new List<Vector3>();
        public readonly List<int> index = new List<int>();
    }
    readonly Dictionary<string,Piece> buckets = new Dictionary<string,Piece>();
    readonly List<Mesh> meshes=new List<Mesh>();
    readonly List<Material> materials=new List<Material>();
    const string tile="D4D5C9",dark="343C44",steel="8B9CA3",gold="BE985D", blue="263759",glass="36556B",gasket="545B64", mark="AC6246";
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    public static Transform Build(Transform parent) {
        if(parent){
            var existing=parent.Find("ArkStationExterior");if(existing)return existing;
        }
        var root=new GameObject("ArkStationExterior");
        root.transform.SetParent(parent,false);
        var asset=root.AddComponent<ArkStationExterior>();
        asset.Author();asset.Commit();return root.transform;
    }
    void OnDestroy(){foreach(var m in meshes)if(m)Destroy(m);foreach(var m in materials)if(m)Destroy(m);}
    void Quad(string mat,Vector3 a,Vector3 b,Vector3 c,Vector3 d) {
        if(!buckets.TryGetValue(mat,out var p)){p=new Piece();buckets.Add(mat,p);}
        int v=p.xyz.Count;p.xyz.Add(a);p.xyz.Add(b);p.xyz.Add(c);p.xyz.Add(d);
        Vector3 n=Vector3.Cross(b-a,c-a).normalized; for(int k=0;k<4;k++)p.normals.Add(n);
        p.index.Add(v);p.index.Add(v+1);p.index.Add(v+2);p.index.Add(v);p.index.Add(v+2);p.index.Add(v+3);
    }
    void Panel(string mat,Vector3 p,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Quaternion rot){
        Quad(mat,p+rot*a,p+rot*b,p+rot*c,p+rot*d);
    }
    void Block(Vector3 p,Vector3 s,string mat,Quaternion rot) {
        var a=V(-s.x,-s.y,-s.z)*.5f; var b=V(s.x,s.y,s.z)*.5f;
        Vector3 p0=a,p1=V(a.x,b.y,a.z),p2=V(b.x,b.y,a.z),p3=V(b.x,a.y,a.z);
        Vector3 p4=V(a.x,a.y,b.z),p5=V(a.x,b.y,b.z),p6=b,p7=V(b.x,a.y,b.z);
        Panel(mat,p,p0,p1,p2,p3,rot);Panel(mat,p,p4,p7,p6,p5,rot);
        Panel(mat,p,p0,p4,p5,p1,rot);Panel(mat,p,p3,p2,p6,p7,rot);
        Panel(mat,p,p1,p5,p6,p2,rot);Panel(mat,p,p0,p3,p7,p4,rot);
    }
    void Block(Vector3 p,Vector3 s,string mat)=>Block(p,s,mat,Quaternion.identity);
    void Beam(Vector3 a,Vector3 b,float thickness,string mat) {
        Block((a+b)*.5f,V(thickness,(b-a).magnitude,thickness),mat,Quaternion.FromToRotation(Vector3.up,b-a));
    }
    void Hull(Vector3 p,Quaternion q,int count,float[] rings,float[] radii,string[] bands,bool lined=false) {
        // Ring profiles with per-sector thermal inserts.
        for(int step=0;step<rings.Length-1;step++){
            for(int j=0;j<count;j++){
                float t=j*2*Mathf.PI/count,u=(j+1)*2*Mathf.PI/count;
                Vector3 A(int ring,float a)=>p+q*V(Mathf.Cos(a)*radii[ring],Mathf.Sin(a)*radii[ring],rings[ring]);
                string mat=bands[step];
                if(mat==tile && step%2==0 && j%8==2)mat=steel;
                Quad(mat,A(step,t),A(step,u),A(step+1,u),A(step+1,t));
                var surface=buckets[mat]; int first=surface.normals.Count-4; float dz=rings[step+1]-rings[step], dr=radii[step+1]-radii[step];
                Vector3 N(float a)=>q*V(dz*Mathf.Cos(a),dz*Mathf.Sin(a),-dr).normalized;
                surface.normals[first]=surface.normals[first+3]=N(t); surface.normals[first+1]=surface.normals[first+2]=N(u);
                if(lined) Quad(tile,A(step,t),A(step+1,t),A(step+1,u),A(step,u));
            }
        }
    }
    void Habitat(Vector3 center,Quaternion q,float r,float l) {
        Hull(center,q,56,new[]{-l*.58f,-l*.58f,-l*.48f,-l*.35f,-l*.31f,l*.29f,l*.35f,l*.49f,l*.58f,l*.58f},
            new[]{.02f,r*.49f,r*.70f,r,r*1.027f,r*1.027f,r,r*.71f,r*.48f,.02f},
            new[]{dark,steel,tile,gasket,tile,gasket,tile,steel,dark});
        for(int j=0;j<9;j++){
            float a=(j+1)*2*Mathf.PI/9f;Vector3 radial=V(Mathf.Cos(a),Mathf.Sin(a),0);
            var frame=q*Quaternion.AngleAxis(a*Mathf.Rad2Deg-90,Vector3.forward);
            Block(center+q*(radial*r*1.034f+V(0,0,l*.038f)),V(.88f,.15f,l*.39f),j%3==0?gold:steel,frame);
            // Fasteners and orientation marks read near but don't clutter the silhouette.
            for(int k=-1;k<=1;k++)Block(center+q*(radial*r*1.053f+V(0,0,k*l*.10f)),V(.60f,.12f,.11f),dark,frame);
        }
        Hull(center+q*V(0,0,-l*.588f),q,20,new[]{-.13f,-.08f,0,.13f},new[]{r*.24f,r*.42f,r*.45f,r*.36f},new[]{dark,glass,steel});
    }
    void Truss(float start,float end,float y,float z,float half) {
        int sections=Mathf.CeilToInt((end-start)/4.6f);
        for(int i=0;i<sections;i++){
            float x=Mathf.Lerp(start,end,(float)i/sections), xn=Mathf.Lerp(start,end,(float)(i+1)/sections);
            foreach(int a in new[]{-1,1}) foreach(int b in new[]{-1,1}) {
                var left=V(x,y+half*a,z+half*b);var right=V(xn,y+half*a,z+half*b);
                Beam(left,right,.255f,"A0ABB2");
                Beam(left,V(x,y-half*a,z+half*b),.17f,gasket);
                Beam(left,V(x,y+half*a,z-half*b),.17f,gasket);
                if((a==1) == (i%2==0)) Beam(left,V(xn,y-half*a,z+half*b),.19f,gasket);
                if((b==1) == (i%2!=0)) Beam(left,V(xn,y+half*a,z-half*b),.19f,gasket);
            }
        }
    }
    void Solar(Vector3 p,float yaw,float pitch,float w,float d){
        var q=Quaternion.Euler(pitch,yaw,0);
        Vector3 P(float x,float y,float z)=>p+q*V(x,y,z);
        Block(p,V(w,.10f,d),"475977",q);
        // Individual connected panel cassettes with alternating bus bars and opposed back battens.
        for(int x=0;x<4;x++)for(int z=0;z<8;z++){
            float a=-w/2+(x+.5f)*w/4,b=-d/2+(z+.5f)*d/8;
            Block(P(a,.071f,b),V(w/4-.17f,.038f,d/8-.15f),(x+z)%5==0?"354971":blue,q);
            Block(P(a,.10f,b),V(w/4-.24f,.014f,.027f),gold,q);
            Block(P(a,-.057f,b),V(w/4-.21f,.012f,d/8-.20f),(x+z)%3==0?"587697":"4A6789",q);
            if(z%2==0)Block(P(a,-.067f,b),V(.12f,.054f,d/8-.1f),steel,q);
        }
        foreach(int sign in new[]{-1,1}){
            Block(P(sign*w*.5f,0,0),V(.13f,.16f,d+.20f),steel,q);
            Block(P(0,0,sign*d*.5f),V(w+.15f,.16f,.13f),steel,q);
        }
        Beam(P(-w*.39f,-.11f,-d*.49f),P(w*.39f,-.11f,d*.49f),.087f,gasket);
        Beam(P(w*.39f,-.11f,-d*.49f),P(-w*.39f,-.11f,d*.49f),.087f,gasket);
    }
    void Dish(Vector3 center,Vector3 facing,float r) {
        Quaternion rot=Quaternion.FromToRotation(Vector3.forward,facing.normalized);
        Hull(center,rot,36,new[]{-1.62f,-1.24f,-.48f,.04f},new[]{.06f,r*.43f,r*.76f,r},new[]{steel,steel,tile},true);
        // Feed support and rim are physical geometry, not decals.
        for(int i=0;i<6;i++){
            float a=i*Mathf.PI/3;
            Beam(center+rot*V(Mathf.Cos(a)*r,Mathf.Sin(a)*r,.03f),center+rot*V(0,0,1.55f),.095f,gasket);
        }
        Block(center+rot*V(0,0,1.56f),V(.37f,.37f,.39f),gold,rot);
    }
    void Author(){
        Habitat(V(0,0,-13),Quaternion.identity,5.5f,21f);
        Habitat(V(0,0,10.8f),Quaternion.identity,4.2f,22f);
        Hull(V(0,0,-28.8f),Quaternion.identity,64,new[]{-6,-5f,-3.2f,1.2f,4.1f},
            new[]{.02f,1.8f,3.55f,3.55f,2.84f},new[]{dark,glass,tile,steel});
        // Observation face lenslets avoid a perfect chrome sphere.
        for(int k=-2;k<=2;k++)Block(V(k*.73f,.67f,-33.1f+Mathf.Abs(k)*.36f),V(.59f,.34f,.10f),glass);
        var transverse=Quaternion.Euler(0,90,0);
        Habitat(V(-11.8f,1.7f,-6),transverse,2.96f,13f);
        Habitat(V(11.5f,1.7f,-6),transverse,3.4f,13.4f);
        var up=Quaternion.Euler(-90,0,0);
        Habitat(V(1.4f,7.4f,3.4f),up,2.66f,7);
        Hull(V(0,0,27.2f),Quaternion.identity,24,new[]{-5,-3.1f,1.1f,4.4f,5},
            new[]{2.39f,3.12f,3.12f,2.0f,.10f},new[]{tile,steel,gold,dark});
        Truss(-65,65,0,15,1.38f);
        Truss(-17,17,-.9f,-9,.75f);
        for(int side=-1;side<=1;side+=2) {
            foreach(float x in new[]{26f,42f,57f}){
                float px=x*side;
                Block(V(px,0,15),V(1.25f,3.22f,3.18f),mark);
                // Outward rotated and staggered in depth; yoke supports both hinged blankets.
                float pitch=(x==42?14f:24f)*side;
                Beam(V(px,0,15),V(px,0,32.3f),.47f,gasket);
                Beam(V(px,0,15),V(px,0,-1.6f),.47f,gasket);
                Solar(V(px,0,31.4f),side*(x==57?3:0),pitch,10.9f,15.9f);
                Solar(V(px,0,-.4f),side*(x==57?3:0),pitch,10.9f,15.9f);
            }
            Block(V(side*6,-5.95f,17.9f),V(2.4f,.25f,12.5f),steel);
            for(int i=0;i<8;i++)Block(V(side*6,-5.70f,12.45f+i*1.55f),V(2.25f,.13f,.12f),gasket);
            Beam(V(1.15f*side,-3.2f,14),V(6*side,-6,17),.36f,steel);
            Beam(V(side*3.6f,0,-23),V(side*6.95f,0,-26),.26f,steel);
            Block(V(side*7.1f,0,-26.1f),V(.83f,.46f,.69f),side==-1?"!DC7364":"!79CCA9");
        }
        Beam(V(3.3f,4.6f,6.2f),V(7.5f,12.6f,10),.39f,steel);
        Dish(V(7.5f,12.6f,10),V(.49f,.74f,-.37f),3.05f);
        Beam(V(-3,4.5f,-12),V(-8.3f,9.5f,-15.2f),.18f,steel);
        Dish(V(-8.3f,9.5f,-15.2f),V(-.6f,.41f,-.4f),2.2f);
        for(int i=0;i<4;i++){
            float x=-1.45f+i*.88f;
            Beam(V(x,-2.6f,-32.1f),V(x*1.3f,-4.0f,-44.4f+i*2.8f),.080f,gasket);
        }
    }
    void Commit(){
        var shader=Shader.Find("Standard");
        if(!shader)shader=Shader.Find("Diffuse");
        foreach(var kv in buckets) {
            var mk=kv.Key;var part=kv.Value;
            var mesh=new Mesh{name="Exterior / "+mk};
            if(part.xyz.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(part.xyz);mesh.SetTriangles(part.index,0);mesh.RecalculateBounds();mesh.SetNormals(part.normals);meshes.Add(mesh);
            var material=new Material(shader);materials.Add(material);
            ColorUtility.TryParseHtmlString("#"+mk.TrimStart('!'),out var c);material.color=c;
            if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",mk==steel?.45f:mk==gold?.55f:.13f);
            if(mk=="A0ABB2"){
                if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",.24f);
                if(material.HasProperty("_EmissionColor")){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",c*.075f);}
            }
            if(material.HasProperty("_Glossiness"))material.SetFloat("_Glossiness",mk==blue||mk==glass?.64f:.27f);
            if((mk=="587697" || mk=="4A6789" || mk==blue || mk=="354971") && material.HasProperty("_EmissionColor")){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",c*.23f);}
            if(mk.StartsWith("!")&&material.HasProperty("_EmissionColor")){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",c*1.15f);}
            var child=new GameObject("Hull mesh "+mk);child.transform.SetParent(transform,false);
            child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        buckets.Clear();
    }
}

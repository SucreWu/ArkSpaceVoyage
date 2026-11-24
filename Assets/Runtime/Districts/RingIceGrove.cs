using System.Collections.Generic;
using UnityEngine;

namespace ArkWalk.Art {
public static class RingIceGrove {
    // Ring frame: local X tangential, Z direction of traversal, Y normal.
    public static Transform Build(Transform parent,Vector3 center,Quaternion ringFrame,Vector3 localSun,int seed=167,Material shared=null) {
        var shader=Resources.Load<Shader>("ArkRingIce");
        if(!shader){Debug.LogError("ArkRingIce resource not found");return null;}
        var root=new GameObject("Ring ice / sculpted traversal volume").transform;
        root.SetParent(parent,false);root.localPosition=center;root.localRotation=ringFrame;
        var m=shared ? shared : new Material(shader);m.SetVector("_LightDir",root.TransformDirection(localSun.normalized));
        var rng=new System.Random(seed);
        float R()=> (float)rng.NextDouble();
        int triangles=0;
        for(int lane=0;lane<7;lane++) {
            var v=new List<Vector3>(); var colors=new List<Color>(); var uv=new List<Vector2>();
            float bias=(lane-3)*11f;
            for(int j=0;j<126;j++){
                var pos=new Vector3(bias+(R()-.5f)*9f,-.9f+(R()-.52f)*3.3f,-19f+R()*211f);
                float size=.075f+Mathf.Pow(R(),4.8f)*1.64f;
                // Never place geometric ice directly across the intended centreline.
                if(Mathf.Abs(pos.x)<2.5f){pos.x=pos.x<0?-2.5f:2.5f;size=Mathf.Min(.46f,size);}
                if(j<4&&lane>=4){pos=new Vector3(3.4f+3.15f*(lane-4)+R(),-1.1f+R()*.7f,12f+7.5f*j+R()*3f);size=.73f+R()*1.5f;}
                int n=5+rng.Next(4);
                var a=new Vector3[n];var b=new Vector3[n];var c=new Vector3[n];
                float flatten=j%3==0?.29f: j%3==1?.68f:1.08f;
                var stretch=new Vector3(.8f+R()*.85f,1f,.82f+R()*.56f);
                var rotate=Quaternion.Euler(R()*120f,R()*360f,R()*100f);
                float az=R()*6.28f,grain=R();
                for(int k=0;k<n;k++){
                    float theta=az+k*Mathf.PI*2/n+(R()-.5f)*.22f;
                    var ground=new Vector3(Mathf.Cos(theta),0,Mathf.Sin(theta));
                    a[k]=Map(ground*(.33f+R()*.19f)+new Vector3(.09f,flatten*.64f+(R()-.5f)*.18f,0));
                    b[k]=Map(ground*(.71f+R()*.27f)+new Vector3(0,(R()-.5f)*flatten*.44f,0));
                    c[k]=Map(ground*(.40f+R()*.24f)+new Vector3(-.08f,-flatten*.56f+(R()-.5f)*.17f,.04f));
                }
                Vector3 Map(Vector3 q)=>pos+rotate*Vector3.Scale(q,stretch)*size;
                void Tri(Vector3 p,Vector3 q,Vector3 s){
                    if(Vector3.Dot(Vector3.Cross(q-p,s-p),(p+q+s)/3-pos)<0){var swap=q;q=s;s=swap;}
                    v.Add(p);v.Add(q);v.Add(s);uv.Add(new Vector2(0,0));uv.Add(new Vector2(1,0));uv.Add(new Vector2(0,1));
                    var tint=new Color(grain,R(),Mathf.Clamp01(size*.69f),1);
                    colors.Add(tint);colors.Add(tint);colors.Add(tint);triangles++;
                }
                var top=Map(new Vector3(.09f,flatten*.71f,0));var low=Map(new Vector3(-.08f,-flatten*.59f,.04f));
                for(int k=0;k<n;k++){
                    int l=(k+1)%n;
                    Tri(a[k],a[l],top);Tri(c[k],low,c[l]);Tri(a[k],b[k],a[l]);Tri(a[l],b[k],b[l]);Tri(b[k],c[k],b[l]);Tri(b[l],c[k],c[l]);
                }
            }
            var mesh=new Mesh{name="Ice lane "+lane};mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetUVs(0,uv);
            var idx=new int[v.Count];for(int i=0;i<idx.Length;i++)idx[i]=i;
            mesh.SetTriangles(idx,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(mesh.name);go.transform.SetParent(root,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;var render=go.AddComponent<MeshRenderer>();render.sharedMaterial=m;render.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Debug.Log("ARK ICE: 7 parallax lanes, "+triangles+" triangles, no colliders");
        return root;
    }
}
}

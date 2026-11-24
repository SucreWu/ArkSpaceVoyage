using UnityEngine;
namespace ArkWalk {
internal static class ArkEarthGlobe {
    internal static Mesh Map(DistrictSet owner, Mesh reference) {
        var p=reference.vertices;var refUV=reference.uv;
        var cosine=Vector3.zero;var sine=Vector3.zero;float latitude=0;
        for(int i=0;i<p.Length;i++){
            float y=p[i].normalized.y;
            if(Mathf.Abs(y)>.90f)continue;
            var eq=new Vector3(p[i].x,0,p[i].z).normalized;
            float a=refUV[i].x*2*Mathf.PI;
            cosine+=eq*Mathf.Cos(a);sine+=eq*Mathf.Sin(a);
            latitude+=y*(refUV[i].y-.5f);
        }
        if(cosine.sqrMagnitude<.1f || sine.sqrMagnitude<.1f)return reference;
        cosine.Normalize();
        sine=(sine-cosine*Vector3.Dot(cosine,sine)).normalized;
        if(sine.sqrMagnitude<.1f)return reference;
        float sign=latitude<0?-1:1;
        bool invert=Vector3.Dot(Vector3.Cross(Vector3.up*sign,sine),cosine)<0;
        const int slices=256,bands=128,stride=slices+1;
        var vertices=new Vector3[stride*(bands+1)];var norms=new Vector3[vertices.Length];var uv=new Vector2[vertices.Length];var tris=new int[slices*bands*6];
        int n=0;
        for(int j=0;j<=bands;j++)for(int i=0;i<=slices;i++){
            float u=(float)i/slices,v=(float)j/bands;
            float a=(i==slices?0:u)*2*Mathf.PI,h=sign*(v-.5f)*Mathf.PI;
            int k=i+j*stride;
            Vector3 radial=(cosine*Mathf.Cos(a)+sine*Mathf.Sin(a))*Mathf.Cos(h)+Vector3.up*Mathf.Sin(h);
            norms[k]=radial;vertices[k]=radial*.5f;uv[k]=new Vector2(u,v);
            if(j==bands||i==slices)continue;
            tris[n++]=k;tris[n++]=invert?k+1:k+stride;tris[n++]=invert?k+stride:k+1;
            tris[n++]=k+1;tris[n++]=invert?k+stride+1:k+stride;tris[n++]=invert?k+stride:k+stride+1;
        }
        var mesh=owner.Own(new Mesh{name="Blue Marble equirectangular Earth"});mesh.vertices=vertices;mesh.normals=norms;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateBounds();
        Debug.Log($"EARTH MAPPING basis {cosine:F4}, {sine:F4}; latitude {sign}; vertices {vertices.Length}");
        return mesh;
    }
}}

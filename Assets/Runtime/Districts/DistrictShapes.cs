using System.Collections.Generic;
using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk
{
    internal static class DistrictShapes
    {
        internal static Mesh Panel(DistrictSet d, params Vector3[] points)
        {
            var vs=new List<Vector3>(); var uv=new List<Vector2>(); var idx=new List<int>();
            for(int side=0;side<2;side++)
            {
                int offset=vs.Count;
                foreach(var p in points){vs.Add(p); uv.Add(new Vector2(.20f*p.x+.061f*p.z,.23f*p.z+.18f*p.y));}
                for(int k=1;k<points.Length-1;k++){idx.Add(offset); idx.Add(offset+(side==0?k:k+1)); idx.Add(offset+(side==0?k+1:k));}
            }
            var m=new Mesh(); m.SetVertices(vs); m.SetUVs(0,uv); m.SetTriangles(idx,0); m.RecalculateNormals(); m.RecalculateBounds(); return d.Own(m);
        }
        internal static Mesh Canopy(DistrictSet d)
        {
            var v=new List<Vector3>(); var tris=new List<int>(); var uv=new List<Vector2>();
            for(int i=0;i<=20;i++){float a=i*Mathf.PI/20; for(int j=0;j<2;j++){v.Add(V(-Mathf.Cos(a),Mathf.Sin(a),2*j-1)); uv.Add(new Vector2(i/20f,j));} if(i<20){int k=i*2; tris.AddRange(new[]{k,k+1,k+3,k,k+3,k+2});}}
            var m=new Mesh();m.SetVertices(v); m.SetUVs(0,uv); m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateBounds();return d.Own(m);
        }
        internal static Mesh Quilt(DistrictSet d)
        {
            const int across=23, length=26;
            var v=new Vector3[across*length];var uv=new Vector2[v.Length];var idx=new int[(across-1)*(length-1)*6];int n=0;
            for(int row=0;row<length;row++) for(int col=0;col<across;col++)
            {
                float along=row/(float)(length-1), cross=2*col/(float)(across-1)-1;
                float shape=Mathf.Pow(Mathf.Max(0,1-cross*cross),.63f);
                float width=Mathf.Lerp(.345f,.446f,Mathf.SmoothStep(0,1,along));
                float tuck=Mathf.SmoothStep(.035f,1,Mathf.Clamp01(along/.18f));
                float folds=tuck*(.017f*Mathf.Cos(along*35+cross*2.4f)+.011f*Mathf.Cos(cross*15-along*7));
                float lift=tuck*(.22f+.09f*Mathf.Sin(along*2.15f));
                int k=row*across+col;v[k]=V(cross*width,.018f+(lift+folds)*shape,-.99f+1.37f*along+.048f*Mathf.Sin(cross*3+along));uv[k]=new Vector2(cross*.84f,along*2.20f);
                if(col<across-1 && row<length-1){idx[n++]=k;idx[n++]=k+across;idx[n++]=k+1;idx[n++]=k+1;idx[n++]=k+across;idx[n++]=k+across+1;}
            }
            var m=d.Own(new Mesh { name="保温被面与褶皱" });m.vertices=v;m.uv=uv;m.triangles=idx;m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        internal static Mesh Rock(DistrictSet d,int n)
        {
            var v=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();
            const int slices=9, rows=6;
            for(int j=0;j<=rows;j++) for(int k=0;k<=slices;k++)
            {
                float a=2*Mathf.PI*(k%slices)/slices; float b=Mathf.PI*j/rows; float wobble=1+.16f*Mathf.Sin(a*3+1.2f*n)*Mathf.Sin(b)+.09f*Mathf.Sin(a*2-b*3+n)*Mathf.Sin(b);
                v.Add(V(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a))*wobble); uv.Add(new Vector2(k/(float)slices,j/(float)rows));
                if(j<rows && k<slices){int q=j*(slices+1)+k; tris.AddRange(new[]{q,q+1,q+slices+1,q+1,q+slices+2,q+slices+1});}
            }
            var m=new Mesh();m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateBounds();return d.Own(m);
        }
    }
}

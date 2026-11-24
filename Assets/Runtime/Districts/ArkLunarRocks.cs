using System.Collections.Generic;
using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkLunarRocks : MonoBehaviour {
        internal Mesh Previous {get;private set;}
        internal int Count {get;private set;}
        internal static void Build(DistrictSet d,Material material,System.Func<float,float,float> height){
            var variants=new Mesh[12];
            for(int i=0;i<variants.Length;i++)variants[i]=Chunk(d,i);
            string[] args=System.Environment.GetCommandLineArgs();
            bool proof=System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkMoonAB")>=0;
            var old=proof?new[]{DistrictShapes.Rock(d,0),DistrictShapes.Rock(d,1),DistrictShapes.Rock(d,2)}:null;
            var parts=new CombineInstance[145];var before=proof?new CombineInstance[145]:null;
            for(int i=0;i<parts.Length;i++){
                // Same RNG reads/order, transforms and sizes as the original scatter.
                float x=d.Random(-50,50),z=d.Random(-10,109),r=d.Random(.22f,1.6f);
                var matrix=Matrix4x4.TRS(new Vector3(x,height(x,z)+r*.22f,z),Quaternion.Euler(d.Random(-16,16),d.Random(0,360),d.Random(-11,11)),new Vector3(r*1.32f,r*.68f,r));
                parts[i]=new CombineInstance{mesh=variants[(i*7+i/12*5)%variants.Length],transform=matrix};
                if(proof)before[i]=new CombineInstance{mesh=old[i%3],transform=matrix};
            }
            Mesh Merge(CombineInstance[] batch,string label){
                var m=d.Own(new Mesh{name=label});m.CombineMeshes(batch,true,true);m.RecalculateBounds();return m;
            }
            var visible=d.Visual("Moon existing boulders",Merge(parts,"145 framed lunar fragments"),material,d.transform,Vector3.zero,Vector3.one,Quaternion.identity);
            var proofState=visible.gameObject.AddComponent<ArkLunarRocks>();proofState.Count=parts.Length;
            if(proof)proofState.Previous=Merge(before,"145 source rounded lunar boulders");
        }
        static Mesh Chunk(DistrictSet d,int variant){
            var rng=new System.Random(608713+variant*1039);
            float Range(float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
            const int h=9,w=15,stride=w+1;
            var v=new Vector3[(h+1)*stride];var norms=new Vector3[v.Length];var indices=new List<int>();
            float a0=Range(0,6.283185f),a1=a0+Range(1.45f,3.05f),offset0=Range(.51f,.78f),offset1=Range(.68f,.88f);
            float depth=Range(.79f,1),slab=Range(.81f,.99f),height=Range(.77f,.99f);
            Vector3 chipA=new Vector3(Mathf.Cos(a0),Range(.15f,.88f),Mathf.Sin(a0)).normalized;
            Vector3 chipB=new Vector3(Mathf.Cos(a1),Range(.21f,.71f),Mathf.Sin(a1)).normalized;
            for(int j=0;j<=h;j++)for(int i=0;i<=w;i++){
                float b=Mathf.PI*j/h,a=6.283185f*(i%w)/w, sin=Mathf.Sin(b),cos=Mathf.Cos(b);
                var axis=new Vector3(Mathf.Cos(a)*sin,cos,Mathf.Sin(a)*sin);
                float r=1+sin*(.10f*Mathf.Sin(3*a+1.3f*b+a0)+.077f*Mathf.Sin(2*a-2.4f*b+a1)+.048f*Mathf.Sin(5*a+b+variant));
                Vector3 p=Vector3.Scale(axis*r,new Vector3(depth,height,slab));
                Vector3 n=new Vector3(axis.x/depth,axis.y/height,axis.z/slab).normalized;
                void Chip(Vector3 face,float at){
                    float gap=Vector3.Dot(p,face)-at;
                    if(gap>0)p-=face*gap;
                    n=Vector3.Lerp(n,face,Mathf.SmoothStep(0,1,(gap+.07f)/.12f));
                }
                Chip(chipA,offset0);
                if(variant%4!=1)Chip(chipB,offset1);
                int k=j*stride+i;v[k]=p;norms[k]=n.normalized;
                if(j<h&&i<w)indices.AddRange(new[]{k,k+1,k+stride,k+1,k+stride+1,k+stride});
            }
            var m=d.Own(new Mesh{name="Weathered lunar chunk with local fractures "+variant});
            m.vertices=v;m.normals=norms;m.SetTriangles(indices,0);m.RecalculateBounds();return m;
        }
    }
}
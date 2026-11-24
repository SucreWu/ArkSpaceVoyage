using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk {
internal sealed class BinaryPreview : MonoBehaviour {
    internal Transform A,B;
    internal Vector3 Center;
    void Update(){
        float a=Time.time*.11f;
        var offset=V(Mathf.Cos(a)*27,Mathf.Sin(a)*6,Mathf.Sin(a)*12);
        if(A&&B){A.localPosition=Center-offset*.48f;B.localPosition=Center+offset;}
    }
}
internal static class ArkStellarRoutes {
    static Transform Star(DistrictSet d,Mesh sphere,string name,Vector3 p,float r,string tint,float brightness,float level){
        var m=d.Mat(name,tint,"star",0,.13f,brightness);
        m=ArkStellarMaterialComparison.Bright(d,name,tint,level,m);
        var t=d.Visual(name,sphere,m,d.transform,p,Vector3.one*r,Quaternion.identity);
        t.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        return t;
    }
    internal static void Sirius(DistrictSet d,Mesh sphere){
        var center=V(-2,10,119);var q=d.Group("Sirius compressed orbital animation").gameObject.AddComponent<BinaryPreview>();
        q.Center=center;
        q.A=Star(d,sphere,"Sirius A blue-white photosphere",center+V(-13,0,0),13,"CFE3FF",2.6f,.98f);
        q.B=Star(d,sphere,"Sirius B white dwarf",center+V(27,0,0),2.3f,"ECF7FF",1.65f,.90f);
        // One broad native flight exclusion region covers both changing stellar positions.
        d.Mark(V(0,9,70),6,"天狼星双星视窗","天狼星系统距地球约八点六光年。明亮的蓝白主星旁，较小的天狼星乙是一颗白矮星。");
        d.Mark(V(23,8,62),4,"双星视差","向侧方飞，再回头观察两颗星的相对位置。公转速度、尺寸与间距均为演示而压缩或放大。");
    }
    internal static void Pleiades(DistrictSet d,Mesh sphere){
        var near=new[]{V(-24,10,40),V(24,15,59),V(-31,-3,79),V(21,-6,93),V(6,22,103),V(-19,17,125),V(29,8,139)};
        var styleArgs=System.Environment.GetCommandLineArgs();
        ArkPleiadesLandmarkComparison landmarkAB=null;
        if(System.Array.IndexOf(styleArgs,"-arkVerify")>=0&&System.Array.IndexOf(styleArgs,"-arkPleiadesLandmarkAB")>=0)
            landmarkAB=d.gameObject.AddComponent<ArkPleiadesLandmarkComparison>();
        for(int i=0;i<near.Length;i++){
            float originalRadius=i==1?2.8f:1.35f+i*.12f;
            // The mesh and its local solid sphere share this scale; do not detach hazards.
            var t=Star(d,sphere,"Pleiades landmark "+i,near[i],originalRadius*ArkPleiadesLandmarkComparison.PairedScale,i%3==0?"ECF4FF":"B7D6FF",2,.90f-i*.025f);
            if(landmarkAB)landmarkAB.Landmarks.Add((t,Vector3.one*originalRadius));
            t.gameObject.AddComponent<SphereCollider>().radius=1.15f;
        }
        var random=new System.Random(46045);
        var cold=d.Mat("Distant young blue members","AFC9FF","star",0,.1f,1.6f);
        var cool=d.Mat("Faint cluster members","DDE9FF","star",0,.1f,.9f);
        cold=ArkStellarMaterialComparison.Bright(d,"Distant young blue members","AFC9FF",.68f,cold);
        cool=ArkStellarMaterialComparison.Bright(d,"Faint cluster members","DDE9FF",.55f,cool);
        var args=System.Environment.GetCommandLineArgs();
        ArkPleiadesScaleComparison scaleProof=null;
        if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkPleiadesScaleAB")>=0){
            scaleProof=d.gameObject.AddComponent<ArkPleiadesScaleComparison>();
            scaleProof.ColdCandidate=cold;scaleProof.CoolCandidate=cool;
            scaleProof.ColdPrior=d.Mat("Pleiades QA old distant blue scale","FFFFFF");
            scaleProof.CoolPrior=d.Mat("Pleiades QA old faint scale","FFFFFF");
            scaleProof.ColdPrior.shader=cold.shader;scaleProof.ColdPrior.CopyPropertiesFromMaterial(cold);
            scaleProof.CoolPrior.shader=cool.shader;scaleProof.CoolPrior.CopyPropertiesFromMaterial(cool);
        }
        for(int i=0;i<114;i++){
            float z=50+(float)random.NextDouble()*110;
            float x=((float)random.NextDouble()-.5f)*100,y=((float)random.NextDouble()-.45f)*65;
            if(Mathf.Abs(x)<16 && y>-2 && y<11)continue;
            float r=.13f+(float)random.NextDouble()*.42f;
            // Seven collider-bearing navigation landmarks remain exact.
            // Reduce only collision-free dotted members and preserve their seeded centers.
            d.Add(sphere,V(x,y,z),Vector3.one*(r*(scaleProof?.64f:1f)),i%3==0?cold:cool,Quaternion.identity);
            if(scaleProof)d.Add(sphere,V(x,y,z),Vector3.one*r,i%3==0?scaleProof.ColdPrior:scaleProof.CoolPrior,Quaternion.identity);
        }
        d.Mark(V(-19,9,35),4,"近层星光","昴星团是一个年轻的疏散星团，距地球约四百四十光年，不是银河系外的天体。");
        d.Mark(V(14,7,73),5,"远层对照","向左右飞，近处的星点移动得更快。星团成员的位置、发光体大小和星间距离都作了艺术化处理，并非恒星测绘。");
    }
}
}

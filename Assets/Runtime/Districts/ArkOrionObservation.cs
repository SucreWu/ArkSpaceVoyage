using UnityEngine;
using static ArkWalk.DistrictSet;
namespace ArkWalk
{
    internal static class ArkOrionObservation
    {
        internal static void Build(DistrictSet d)
        {
            var raw=Resources.Load<Texture2D>("Districts/orion_hubble_ccby_4k");
            if(!raw)throw new System.IO.IOException("Official Orion observing texture missing");
            var mat=d.Mat("Hubble angular observation","FFFFFF"); mat.shader=Resources.Load<Shader>("Districts/OrionApertureMatte"); mat.mainTexture=raw;
            var p=d.Visual("M42 telescope plate; angular projection only",Primitive(PrimitiveType.Quad),mat,d.transform,V(0,8,202),V(112,112,1),Quaternion.identity);
            p.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            // Native world ornament only; no extra view, chart action or viewer behavior.
            var font=Resources.Load<Font>("Interface/NotoSansSC-Regular");
            if(font){
                Caption(d,font,"Orion observation title","M42  /  HUBBLE OPTICAL COMPOSITE",V(0,-47,201.9f),92,new Color(.74f,.83f,.91f));
                Caption(d,font,"Orion observation credit","2D observation - resized, no depth\nNASA, ESA, M. Robberto (Space Telescope Science Institute/ESA)\nand the Hubble Space Telescope Orion Treasury Project Team",
                    V(0,-53,201.9f),112,new Color(.63f,.73f,.84f));
            }
            d.Site.Pitch=-1.7f;
        }
        static void Caption(DistrictSet d,Font font,string name,string value,Vector3 at,float width,Color ink)
        {
            var g=new GameObject(name);
            g.transform.SetParent(d.transform,false);
            g.transform.localPosition=at;
            var t=g.AddComponent<TextMesh>();
            t.font=font;t.text=value;t.fontSize=40;t.characterSize=.18f;
            t.anchor=TextAnchor.UpperCenter;t.alignment=TextAlignment.Center;t.color=ink;
            var r=g.GetComponent<MeshRenderer>();
            r.sharedMaterial=font.material;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows=false;
            if(r.bounds.size.x>.01f)g.transform.localScale=Vector3.one*(width/r.bounds.size.x);
        }
    }
}

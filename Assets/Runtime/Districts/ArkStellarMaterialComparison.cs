using System.Collections.Generic;
using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkStellarMaterialComparison : MonoBehaviour {
        internal readonly Dictionary<Material,Material> Previous=new Dictionary<Material,Material>();
        internal static Material Bright(DistrictSet d,string name,string hex,float level,Material original){
            var shader=Resources.Load<Shader>("ArkStellarLuminosity");
            if(!shader||!shader.isSupported)return original;
            var candidate=d.Mat(name+" intrinsic","FFFFFF");
            candidate.shader=shader;candidate.color=DistrictSet.C(hex);candidate.SetFloat("_Radiance",level);
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkStarAB")>=0){
                var compare=d.GetComponent<ArkStellarMaterialComparison>();
                if(!compare)compare=d.gameObject.AddComponent<ArkStellarMaterialComparison>();
                compare.Previous[candidate]=original;
            }
            return candidate;
        }
    }
}

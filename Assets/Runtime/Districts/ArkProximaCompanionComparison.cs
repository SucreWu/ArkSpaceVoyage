using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkProximaCompanionComparison : MonoBehaviour {
        internal Material Previous {get;private set;}
        internal static void Apply(DistrictSet d,Transform globe){
            var shader=Resources.Load<Shader>("ArkProximaMutedRock");
            if(!shader||!shader.isSupported)return;
            var display=globe.GetComponent<MeshRenderer>();
            var old=display.sharedMaterial;
            var material=d.Mat("Proxima hypothetical muted companion","FFFFFF");
            material.shader=shader;material.mainTexture=old.mainTexture;
            material.color=new Color(.78f,.76f,.73f);
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkProximaRockAB")>=0)
                globe.gameObject.AddComponent<ArkProximaCompanionComparison>().Previous=old;
            display.sharedMaterial=material;
        }
    }
}

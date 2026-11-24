using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkMarsStrataComparison : MonoBehaviour {
        internal Material[] Prior {get;private set;}
        internal static void Apply(DistrictSet d,Transform rock){
            var shader=Resources.Load<Shader>("ArkMarsContinuousStrata");
            if(!shader||!shader.isSupported)return;
            var renderer=rock.GetComponent<MeshRenderer>();var old=renderer.sharedMaterials;
            var look=d.Mat("Mars continuous strata","FFFFFF");
            look.shader=shader;
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkMarsColorAB")>=0){
                rock.gameObject.AddComponent<ArkMarsStrataComparison>().Prior=old;
            }
            renderer.sharedMaterials=new[]{look,look,look};
        }
    }
}

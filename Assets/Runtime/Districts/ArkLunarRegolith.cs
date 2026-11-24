using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkLunarRegolith : MonoBehaviour {
        internal Material Original {get;private set;}
        Material owned;
        void OnDestroy(){if(owned)Destroy(owned);}
        internal static void Apply(Transform ground){
            var shader=Resources.Load<Shader>("ArkLunarRegolith");if(!shader||!shader.isSupported)return;
            var renderer=ground.GetComponent<MeshRenderer>();
            var original=renderer.sharedMaterial;
            var material=new Material(shader){mainTexture=original.mainTexture,color=original.color};
            var v=ground.gameObject.AddComponent<ArkLunarRegolith>();v.Original=original;v.owned=material;
            renderer.sharedMaterial=material;
        }
    }
}

using UnityEngine;
namespace ArkWalk {
    [RequireComponent(typeof(Camera))]
    internal sealed class ArkExteriorCutCue : MonoBehaviour {
        Material material;
        internal float Strength {get;set;}
        internal bool Setup(){
            var shader=Resources.Load<Shader>("ArkExteriorCutCue");
            if(!shader||!shader.isSupported)return false;
            material=new Material(shader);
            enabled=false;
            return true;
        }
        void OnRenderImage(RenderTexture source, RenderTexture dest){
            if(!material||Strength<=0){Graphics.Blit(source,dest);return;}
            material.SetFloat("_Strength",Strength);
            Graphics.Blit(source,dest,material,0);
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}

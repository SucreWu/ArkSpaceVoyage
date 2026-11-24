using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkEarthOceanTrial : MonoBehaviour {
        Material owned;
        internal Material Original {get;private set;}
        void OnDestroy(){if(owned)Destroy(owned);}
        internal static void Build(Transform globe) {
            var shader=Resources.Load<Shader>("ArkEarthOceanTrial");
            var mask=Resources.Load<Texture2D>("Districts/earth_ocean_core_mask");
            if(!shader||!shader.isSupported||!mask)return;
            var render=globe.GetComponent<MeshRenderer>();
            var original=render.sharedMaterial;
            var art=globe.gameObject.AddComponent<ArkEarthOceanTrial>();art.Original=original;
            var mat=new Material(shader){name="Earth trial / open-ocean response",mainTexture=original.mainTexture};
            mat.SetTexture("_OceanMask",mask);mat.SetFloat("_BaseGloss",original.GetFloat("_Glossiness"));
            art.owned=mat;render.sharedMaterial=mat;
        }
    }
}

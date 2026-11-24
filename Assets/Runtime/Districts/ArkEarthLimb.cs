using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkEarthLimb : MonoBehaviour {
        Material owned;
        void OnDestroy(){if(owned)Destroy(owned);}
        internal static void Build(DistrictSet d, Transform globe) {
            var shader=Resources.Load<Shader>("ArkEarthLimb");
            if(!shader || !shader.isSupported){Debug.LogWarning("Earth limb shader unavailable");return;}
            var mesh=globe.GetComponent<MeshFilter>().sharedMesh;
            float radius=mesh.bounds.extents.x*globe.lossyScale.x;
            var center=globe.position;
            var material=new Material(shader){name="Sunlit atmosphere only"};
            material.SetVector("_Body",new Vector4(center.x,center.y,center.z,radius));
            material.SetVector("_Sun",new Vector4(-.9f,.34f,-.46f,0));
            var shell=d.Visual("Earth thin rim atmosphere",mesh,material,globe,Vector3.zero,Vector3.one*1.018f,Quaternion.identity);
            shell.gameObject.AddComponent<ArkEarthLimb>().owned=material;
        }
    }
}

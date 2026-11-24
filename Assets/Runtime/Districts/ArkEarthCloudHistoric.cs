using UnityEngine;
namespace ArkWalk {
    internal sealed class ArkEarthCloudHistoric : MonoBehaviour {
        Material owned;
        void OnDestroy(){if(owned)Destroy(owned);}
        internal static void Build(DistrictSet d,Transform globe) {
            var shader=Resources.Load<Shader>("ArkEarthCloudHistoric");
            var alpha=Resources.Load<Texture2D>("Districts/earth_clouds_cpc_20140602_alpha");
            if(!shader||!shader.isSupported||!alpha)return;
            var mesh=globe.GetComponent<MeshFilter>().sharedMesh;
            var material=new Material(shader){name="NOAA CPC dated cloud alpha",mainTexture=alpha};
            var c=globe.position;
            material.SetVector("_Body",new Vector4(c.x,c.y,c.z,mesh.bounds.extents.x*globe.lossyScale.x));
            material.SetVector("_Sun",new Vector4(-.9f,.34f,-.46f,0));
            var shell=d.Visual("Dated cloud cover layer",mesh,material,globe,Vector3.zero,Vector3.one*1.0024f,Quaternion.identity);
            shell.gameObject.AddComponent<ArkEarthCloudHistoric>().owned=material;
        }
    }
}

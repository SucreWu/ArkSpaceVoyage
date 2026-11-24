using UnityEngine;
using static ArkWalk.DistrictSet;
namespace ArkWalk {
internal sealed class ArkOrionVolume:MonoBehaviour {
    internal static readonly Vector3 Center=V(0,11,136),Scale=V(180,152,94);
    // These centers describe the authored artist fields, not observed depths.
    internal static readonly Vector3[] Sites={V(-.17f,.11f,-.19f),V(-.050f,.032f,.072f),V(.32f,-.10f,.222f)};
    Texture3D noise;
    internal static bool Build(DistrictSet d){
        var shader=Resources.Load<Shader>("ArkOrionGasVolume");
        if(!shader||!shader.isSupported||!SystemInfo.supports3DTextures||!SystemInfo.SupportsTextureFormat(TextureFormat.R8))return false;
        var look=d.Mat("M42 procedural reconstruction","FFFFFF");
        look.shader=shader;
        var body=d.Visual("M42 drawn gas and dust volume",Primitive(PrimitiveType.Cube),look,d.transform,Center,Scale,Quaternion.identity);
        var owned=body.gameObject.AddComponent<ArkOrionVolume>();
        const int side=32;
        var texels=new byte[side*side*side]; var random=new System.Random(42043);
        random.NextBytes(texels);
        var seed=new Texture3D(side,side,side,TextureFormat.R8,false){
            name="M42 procedural density seed (non-photographic)",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear
        };
        seed.SetPixelData(texels,0);seed.Apply(false,true);
        owned.noise=seed;look.SetTexture("_Noise",seed);
        d.Site.Pitch=-1.7f;
        return true;
    }
    void OnDestroy(){if(noise)Destroy(noise);}
}
}

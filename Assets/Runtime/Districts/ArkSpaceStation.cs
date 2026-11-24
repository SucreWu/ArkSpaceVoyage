using UnityEngine;
using static ArkWalk.DistrictSet;
namespace ArkWalk
{
    internal static class ArkSpaceStation
    {
        internal static void Build(DistrictSet d,Mesh sphere)
        {
            var earth=d.Mat("Earth disk", "FFFFFF","earth_nasa_bmng_8k",0,.15f);
            var planet=d.Visual("Earth behind external station",sphere,earth,d.transform,V(-10,19,142),V(57,57,57),Quaternion.Euler(6,38,0));
            ArkEarthLimb.Build(d,planet);
            ArkEarthOceanTrial.Build(planet);
            ArkEarthCloudHistoric.Build(d,planet);
            var core=V(6.0f,3.4f,48f);
            var exterior=global::ArkStationExterior.Build(d.transform);
            exterior.localPosition=core;
            exterior.localRotation=Quaternion.Euler(24,-24,-13);
            exterior.localScale=Vector3.one*.254f;
            void BlockFlight(Vector3 location,Vector3 size)
            {
                var obstacle=new GameObject("Solid station section");
                obstacle.transform.SetParent(exterior,false);
                obstacle.transform.localPosition=location;
                obstacle.AddComponent<BoxCollider>().size=size;
            }
            BlockFlight(V(0,0,-13),V(9,9,19));
            BlockFlight(V(0,0,10.8f),V(7,7,20));
            BlockFlight(V(0,0,-29),V(5.5f,5.5f,7.5f));
            BlockFlight(V(-11.8f,1.7f,-6),V(12,4.8f,4.8f));
            BlockFlight(V(11.5f,1.7f,-6),V(12,5.2f,5.2f));
            BlockFlight(V(0,0,15),V(127,2.4f,2.4f));
            d.Mark(core+V(0,2.8f,-2.85f),1.2f,"近地空间站外观","这是空间站的外部观测起点。可从星图前往任意航点，或回到这里。场景尺度为示意。");
            d.Site.Spawn=V(0,2,0);d.Site.Yaw=0;d.Site.Pitch=-4;
        }
    }
}

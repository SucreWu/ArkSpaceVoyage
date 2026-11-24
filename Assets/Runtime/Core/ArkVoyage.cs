using System;
using UnityEngine;
namespace ArkWalk
{
    internal static class ArkVoyage
    {
        internal sealed class Orbit
        {
            internal readonly string Id, Dock;
            internal readonly Vector3 Hazard;
            internal readonly float Radius;
            internal Orbit(string id,Vector3 hazard,float radius,string dock="dock"){Id=id;Dock=dock;Hazard=hazard;Radius=radius;}
        }
        internal static readonly Orbit[] Orbits={
            new Orbit("moon",Vector3.zero,0),
            new Orbit("jupiter",new Vector3(13,8,120),39),
            new Orbit("saturn",new Vector3(6,10,126),30),
            new Orbit("proxima",new Vector3(20,5,86),17),
            new Orbit("earth",new Vector3(-12,10,114),36.5f),
            new Orbit("mars",Vector3.zero,0),
            new Orbit("mercury",new Vector3(5,6,105),35),
            new Orbit("venus",new Vector3(10,6,111),42),
            new Orbit("uranus",new Vector3(1,7,116),35),
            new Orbit("neptune",new Vector3(16,9,116),36),
            new Orbit("sirius",new Vector3(-2,10,119),33),
            new Orbit("pleiades",Vector3.zero,0), new Orbit("orion",Vector3.zero,0)
        };
        internal static Orbit Get(string id){foreach(var orbit in Orbits)if(orbit.Id==id)return orbit;return null;}
        internal static bool IsOrbit(string id)=>id=="station-orbit"||Get(id)!=null;
        internal static Vector3 Bound(string id,Vector3 spawn,Vector3 current,Vector3 target)
        {
            var local=Vector3.ClampMagnitude(target-spawn,160);
            local.y=Mathf.Clamp(local.y,-12,62);
            var next=spawn+local;var o=Get(id);
            if(o==null||o.Radius<=0||(next-o.Hazard).sqrMagnitude>=o.Radius*o.Radius)return next;
            var near=current;var far=next;
            if((near-o.Hazard).sqrMagnitude<o.Radius*o.Radius)return spawn;
            for(int i=0;i<13;i++)
            {var mid=(near+far)*.5f;if((mid-o.Hazard).sqrMagnitude<o.Radius*o.Radius)far=mid;else near=mid;}
            return near;
        }
    }
}

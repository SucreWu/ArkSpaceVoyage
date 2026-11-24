using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArkWalk
{
    public static class ArkWingRegistry
    {
        private static readonly Dictionary<string,List<Action<WalkSite>>> mounts=new Dictionary<string,List<Action<WalkSite>>>(StringComparer.Ordinal);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()=>mounts.Clear();
        public static void Register(string siteId,Action<WalkSite> attach)
        {
            if(string.IsNullOrWhiteSpace(siteId)||attach==null)throw new ArgumentException("Provide stable site ID and mount logic");
            if(!mounts.TryGetValue(siteId,out var hooks))mounts[siteId]=hooks=new List<Action<WalkSite>>();
            if(!hooks.Contains(attach))hooks.Add(attach);
        }
        internal static void Attach(WalkSite site)
        {
            if(!mounts.TryGetValue(site.Id,out var hooks))return;
            foreach(var hook in hooks)hook(site);
        }
    }
}

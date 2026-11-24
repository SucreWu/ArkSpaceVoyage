using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkWalk
{
    internal static class ResidentialPeople
    {
        internal static AheGesture Girl(Transform parent)
        {
            var root=ResidentialKit.Group(parent,"阿禾",Vector3.zero);
            root.localRotation=Quaternion.Euler(0,180,0);
            var animation=Actor(root,"Ahe",1.62f,true);
            var gesture=root.gameObject.AddComponent<AheGesture>();
            gesture.Bind(animation,Find(root,"J_Bip_L_Index2"),Find(root,"J_Bip_R_Index2"));
            var body=root.gameObject.AddComponent<CapsuleCollider>();
            body.center=Vector3.up*.80f;
            body.radius=.19f;
            body.height=1.59f;
            return gesture;
        }

        internal static void Neighbor(Transform parent, string label, Vector3 from, Vector3 to, float height)
        {
            var root=ResidentialKit.Group(parent,label,from);
            root.rotation=Quaternion.LookRotation((to-from).normalized);
            var animation=Actor(root,"Homecomer",height,false);
            var walk=root.gameObject.AddComponent<ResidentialWalker>();
            walk.StartRoute(animation,from,to);
        }

        internal static void StandingNeighbor(Transform parent,string label,Vector3 pos,float height,float yaw)
        {
            var root=ResidentialKit.Group(parent,label,pos);
            root.localRotation=Quaternion.Euler(0,yaw,0);
            var motion=Actor(root,"Homecomer",height,false);
            motion.Play("HomecomerIdle");
            motion["HomecomerIdle"].normalizedTime=Mathf.Repeat(yaw/180f,1f);
        }

        private static Animation Actor(Transform parent,string who,float height,bool near)
        {
            var asset=Resources.Load<GameObject>("Residence/Actors/"+who);
            if(!asset) throw new InvalidOperationException("未找到角色 FBX "+who);
            var model=UnityEngine.Object.Instantiate(asset,parent,false);
            model.name=who;
            Transform ground=Find(model.transform,"HeroGround"), front=Find(model.transform,"HeroForward"), top=Find(model.transform,"HeroTop");
            if(ground && front && top)
            {
                var facing=Vector3.ProjectOnPlane(parent.InverseTransformDirection(front.position-ground.position),Vector3.up);
                if(facing.sqrMagnitude>.000001f) model.transform.localRotation=Quaternion.Inverse(Quaternion.LookRotation(facing))*model.transform.localRotation;
                float h=Vector3.Distance(top.position,ground.position);
                model.transform.localScale*=height/Mathf.Max(.001f,h);
                model.transform.position+=parent.position-ground.position;
            }
            var material=ResidentialKit.MaterialFor(who+"衣料",near?new Color(.94f,.90f,.84f):new Color(.88f,.88f,.85f),near?"ahe_color":"neighbor_color",near?"ahe_relief":"",0,.34f,.16f,true);
            material.SetFloat("_Cutoff",.37f);
            if(material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip",1);
            if(material.HasProperty("_Coverage")) material.SetFloat("_Coverage",1);
            material.EnableKeyword("_ALPHATEST_ON");
            if(near && material.HasProperty("_Finish"))
            {
                material.SetTexture("_Finish",ResidentialKit.Texture("ahe_finish"));
                material.SetFloat("_UseFinish",1);
                material.SetFloat("_SoftPortrait",1);
            }
            material.renderQueue=2450;
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                bool portrait=skin.name.IndexOf("Portrait",StringComparison.OrdinalIgnoreCase)>=0;
                skin.enabled=portrait==near;
                skin.sharedMaterial=material;
                var bounds=skin.localBounds;
                bounds.Expand(new Vector3(.12f,.12f,.55f));
                skin.localBounds=bounds;
                skin.updateWhenOffscreen=false;
            }
            var motion=model.GetComponent<Animation>();
            if(!motion) motion=model.AddComponent<Animation>();
            motion.playAutomatically=false;
            motion.cullingType=near?AnimationCullingType.AlwaysAnimate:AnimationCullingType.BasedOnRenderers;
            foreach(string clipName in new[]{who+"Idle",who+(near?"Offer":"Walk")})
            {
                var clip=Resources.Load<AnimationClip>("Residence/Actors/"+clipName);
                if(!clip) throw new InvalidOperationException("未找到角色动作 "+clipName);
                motion.AddClip(clip,clipName);
                motion[clipName].wrapMode=clipName.EndsWith("Offer",StringComparison.Ordinal)?WrapMode.ClampForever:WrapMode.Loop;
            }
            motion.Play(who+"Idle");
            motion.Sample();
            return motion;
        }

        private static Transform Find(Transform root,string label)
        {
            foreach(var tr in root.GetComponentsInChildren<Transform>(true)) if(tr.name==label)return tr;
            return null;
        }
    }

}

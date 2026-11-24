using System.Collections.Generic;
using UnityEngine;

namespace ArkWalk
{
    internal sealed class AheGesture:MonoBehaviour
    {
        private Animation motion;
        private Transform left,right;
        private readonly List<Finger> fingers = new List<Finger>();
        private bool presenting;

        private sealed class Finger
        {
            public Transform first, middle, tip;
            public Quaternion originalFirst, originalMiddle;
            public float inward, rise, bend;
            public int side;
        }

        private bool shaped;
        internal Transform Paper {get;private set;}
        internal void Bind(Animation animation,Transform leftFinger,Transform rightFinger)
        {
            motion=animation;
            left=leftFinger;
            right=rightFinger;
            RegisterHand(left,1);
            RegisterHand(right,-1);
            Paper=ResidentialKit.Group(transform,"折纸星",Vector3.zero);
            var mesh=Paper.gameObject.AddComponent<MeshFilter>();
            mesh.sharedMesh=FoldedPaper();
            var renderer=Paper.gameObject.AddComponent<MeshRenderer>();
            var tint=new Color(.94f,.94f,.92f);
            var mat=ResidentialKit.MaterialFor("象牙色折纸",tint,"folded_paper","",0,.09f,0,true);
            if(mat.HasProperty("_VertexColor"))mat.SetFloat("_VertexColor",1);
            mat.SetColor("_EmissionColor",new Color(.245f,.213f,.178f));
            renderer.sharedMaterial=mat;
            Align();
        }
        internal void Offer()
        {
            presenting=true;
            Paper.gameObject.SetActive(true);
            motion["AheOffer"].time=0f;
            motion.CrossFade("AheOffer",.15f);
        }
        internal void Accept()
        {
            presenting=false;
            Paper.gameObject.SetActive(false);
            motion.CrossFade("AheIdle",.26f);
        }
        private void LateUpdate()
        {
            if(presenting && left && right) CupHands();
            if(Paper && Paper.gameObject.activeSelf)Align();
        }

        private void RegisterHand(Transform index,int side)
        {
            if(!index || !index.parent || !index.parent.parent)return;
            var wrist=index.parent.parent;
            var prefix=side==1?"J_Bip_L_":"J_Bip_R_";
            RegisterFinger(wrist,prefix+"Thumb",side,.83f,.55f,.34f);
            RegisterFinger(wrist,prefix+"Index",side,.49f,.18f,.40f);
            RegisterFinger(wrist,prefix+"Middle",side,.40f,.13f,.31f);
            RegisterFinger(wrist,prefix+"Ring",side,.29f,.12f,.25f);
            RegisterFinger(wrist,prefix+"Little",side,.18f,.12f,.22f);
        }

        private void RegisterFinger(Transform wrist,string name,int side,float inward,float rise,float bend)
        {
            var a=wrist.Find(name+"1");
            var b=a?a.Find(name+"2"):null;
            var c=b?b.Find(name+"3"):null;
            if(a && b && c) fingers.Add(new Finger {first=a,middle=b,tip=c,side=side,inward=inward,rise=rise,bend=bend});
        }

        private void Update()
        {
            if(!shaped)return;
            foreach(var finger in fingers)
            {
                finger.first.localRotation=finger.originalFirst;
                finger.middle.localRotation=finger.originalMiddle;
            }
            shaped=false;
        }

        private void CupHands()
        {
            float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.28f,.87f,motion["AheOffer"].normalizedTime));
            if(blend<=0)return;
            var across=(right.position-left.position).normalized;
            foreach(var finger in fingers)
            {
                finger.originalFirst=finger.first.localRotation;
                finger.originalMiddle=finger.middle.localRotation;
                var toward=across*finger.side*finger.inward+transform.forward;
                AimJoint(finger.first,finger.middle,toward+transform.up*finger.rise,blend);
                AimJoint(finger.middle,finger.tip,toward+transform.up*finger.bend,blend);
            }
            shaped=true;
        }

        private static void AimJoint(Transform joint,Transform next,Vector3 direction,float blend)
        {
            var delta=Quaternion.FromToRotation(next.position-joint.position,direction);
            joint.rotation=Quaternion.Slerp(Quaternion.identity,delta,blend)*joint.rotation;
        }
        private void Align()
        {
            if(!left || !right)
            {
                Paper.localPosition=new Vector3(0,1.02f,.23f);
                return;
            }
            Vector3 side=right.position-left.position;
            float distance=side.magnitude;
            if(distance<.005f)return;
            side/=distance;
            if(Vector3.Dot(side,transform.right)<0)side=-side;
            Vector3 normal=Vector3.Cross(side,Vector3.up).normalized;
            Vector3 up=Vector3.Cross(normal,side).normalized;
            // 纸的尺寸固定，掌面升降与手臂动画同步。
            Paper.SetPositionAndRotation((left.position+right.position)*.5f+up*.027f-normal*.009f,Quaternion.LookRotation(normal,up));
            Paper.localScale=Vector3.one*.90f;
        }
        private static Mesh foldedMesh;
        private static Mesh FoldedPaper()
        {
            if(foldedMesh)return foldedMesh;
            var v=new List<Vector3>();var uv=new List<Vector2>();var ix=new List<int>();var color=new List<Color>();
            for(int i=0;i<10;i++)
            {
                var a=Point(i);var b=Point(i+1);
                for(int back=0;back<2;back++)
                {
                    var center=new Vector3(0,0,back==0?.0084f:.00765f);
                    var ca=(i%2==0?new Color(.99f,.98f,.95f):new Color(.88f,.86f,.81f));
                    foreach(var p in new[]{center,back==0?a:b,back==0?b:a})
                    {
                        ix.Add(v.Count);v.Add(p);uv.Add(new Vector2(.50f+p.x*4.4f,.50f+p.y*4.4f));color.Add(ca);
                    }
                }
            }
            foldedMesh=new Mesh {name="手折五星"};
            foldedMesh.SetVertices(v);foldedMesh.SetUVs(0,uv);foldedMesh.SetColors(color);
            foldedMesh.SetTriangles(ix,0);foldedMesh.RecalculateNormals();foldedMesh.RecalculateBounds();
            return foldedMesh;
        }
        private static Vector3 Point(int i)
        {
            float a=Mathf.PI*.5f+i*Mathf.PI*.2f;
            float r=i%2==0?.049f:.023f;
            return new Vector3(r*Mathf.Cos(a),r*Mathf.Sin(a),i%2==0?.0014f:-.0019f);
        }
    }
}

using UnityEngine;

namespace ArkWalk
{
    internal sealed class ResidentialWalker:MonoBehaviour
    {
        private Animation motion;
        private Rigidbody body;
        private Vector3 start, finish;
        private float clock, duration;
        private bool walking;
        internal void StartRoute(Animation animation,Vector3 from,Vector3 to)
        {
            motion=animation;
            start=from;
            finish=to;
            duration=Mathf.Max(4f,Vector3.Distance(from,to)/.46f);
            motion["HomecomerWalk"].speed=.73f;
            motion.Play("HomecomerWalk");
            walking=true;
            if (from.y < 1f)
            {
                var shape=gameObject.AddComponent<CapsuleCollider>();
                shape.height=1.55f;
                shape.center=Vector3.up*.775f;
                shape.radius=.19f;
                body=gameObject.AddComponent<Rigidbody>();
                body.isKinematic=true;
                body.useGravity=false;
                body.interpolation=RigidbodyInterpolation.Interpolate;
            }
        }
        private void FixedUpdate()
        {
            clock+=Time.fixedDeltaTime;
            float half=duration+2.65f, elapsed=Mathf.Repeat(clock,half*2f);
            bool outward=elapsed<half;
            float t=Mathf.Repeat(elapsed,half), step=Mathf.Clamp01(t/duration);
            Vector3 position=Vector3.Lerp(start,finish,outward?step:1f-step);
            var route=Quaternion.LookRotation((finish-start).normalized);
            float rotation=(outward?0f:180f)+180f*Mathf.SmoothStep(0,1,(t-duration)/2.5f);
            var orientation=Quaternion.Euler(0,rotation,0)*route;
            if(body)
            {
                body.MovePosition(transform.parent.TransformPoint(position));
                body.MoveRotation(transform.parent.rotation*orientation);
            }
            else
            {
                transform.SetLocalPositionAndRotation(position,orientation);
            }
            bool moving=t<duration;
            if(moving==walking)return;
            walking=moving;
            motion.CrossFade(moving?"HomecomerWalk":"HomecomerIdle",.23f);
        }
    }

}

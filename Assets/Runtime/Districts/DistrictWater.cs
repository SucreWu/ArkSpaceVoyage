using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk
{
    public sealed class DistrictWater : MonoBehaviour
    {
        Transform wheel; Transform[] bubbles; Material ripples; float boost,phase;
        internal void Build(DistrictSet d,Vector3 window)
        {
            d.Box(window,V(.96f,.88f,.14f),d.Dark); d.Box(window+V(0,0,-.085f),V(.77f,.71f,.07f),d.Mat("滤水槽","438583",null,.12f,.64f));
            wheel=d.Group("可见净水叶轮");wheel.localPosition=window+V(-.09f,0,-.15f);
            for(int i=0;i<6;i++)
            {
                var a=Quaternion.AngleAxis(60f*i,Vector3.forward);
                d.Visual("泵叶",Primitive(PrimitiveType.Cube),d.Gold,wheel,a*V(.115f,0,0),V(.22f,.063f,.043f),a*Quaternion.AngleAxis(21,Vector3.forward));
            }
            bubbles=new Transform[5];for(int i=0;i<bubbles.Length;i++)bubbles[i]=d.Visual("水泡",Primitive(PrimitiveType.Sphere),d.Aqua,wheel.parent,window+V(.19f+.035f*(i%2),-.23f+i*.095f,-.16f),Vector3.one*(.038f+.009f*(i%3)),Quaternion.identity);
            for(int s=-1;s<=1;s+=2){d.Box(window+V(s*.446f,0,-.13f),V(.061f,.80f,.047f),d.Steel);d.Box(window+V(0,s*.377f,-.13f),V(.91f,.053f,.047f),d.Steel);}
            var shader=Resources.Load<Shader>("Astra/ControlledCanalFlow");
            if(!shader)throw new System.InvalidOperationException("Missing controlled canal surface shader");
            ripples=new Material(shader);
            for(int i=0;i<2;i++)
            {
                float x=i==0?-2.65f:7.45f;
                var m=d.Own(new Mesh());var v=new[]{V(x-.61f,.159f,-6.02f),V(x-.61f,.159f,12.98f),V(x+.61f,.159f,12.98f),V(x+.61f,.159f,-6.02f)};
                m.vertices=v; float flip=i==0?1:-1; m.uv=new[]{new Vector2(0,0),new Vector2(0,8*flip),new Vector2(1,8*flip),new Vector2(1,0)};m.triangles=new[]{0,1,2,0,2,3};m.RecalculateNormals();m.RecalculateBounds();d.Add(m,Vector3.zero,Vector3.one,ripples,Quaternion.identity);
            }
        }
        void OnDestroy(){if(ripples)Destroy(ripples);}
        internal float FlowPhase => phase;
        public void Demonstrate() { boost=6.2f; }
        void Update()
        {
            if(!wheel) return;
            float dt=Time.deltaTime;boost=Mathf.Max(0,boost-dt);float speed=boost>0?1.30f:.315f;phase+=dt*speed;
            ripples.SetFloat("_FlowPhase",phase);
            wheel.Rotate(Vector3.forward,(boost>0?140:38)*dt,Space.Self);
            var p=wheel.localPosition;for(int i=0;i<bubbles.Length;i++){var t=bubbles[i];var v=t.localPosition;v.y=p.y-.27f+Mathf.Repeat(phase*.44f+.113f*i,.53f);t.localPosition=v;}
        }
    }
}


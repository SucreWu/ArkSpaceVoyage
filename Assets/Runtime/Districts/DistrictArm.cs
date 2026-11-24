using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk
{
    public sealed class DistrictArm : MonoBehaviour
    {
        Transform shoulder, elbow, wrist;
        float elapsed=-1;
        internal void Build(DistrictSet d,Vector3 p)
        {
            d.Rod(p+V(0,.05f,0),p+V(0,.29f,0),.95f,d.Dark); d.Rod(p+V(0,.29f,0),p+V(0,.77f,0),.70f,d.Gold);
            d.Block(p+V(0,.42f,0),V(1.58f,.84f,1.58f),Quaternion.identity);
            shoulder=d.Group("肩轴"); shoulder.localPosition=p+V(0,.80f,0);
            var fore=V(.22f,1.57f,.03f); var reach=V(1.56f,.49f,.12f); var tip=V(.57f,-1.0f,.05f);
            Link(d,shoulder,fore); elbow=d.Group("肘轴",shoulder);elbow.localPosition=fore;
            Link(d,elbow,reach); wrist=d.Group("腕轴",elbow);wrist.localPosition=reach;
            Link(d,wrist,tip);d.Limb(wrist,tip+V(-.15f,-.02f,0),tip+V(-.19f,-.32f,0),.045f,d.Steel);
            d.Limb(wrist,tip+V(.15f,-.02f,0),tip+V(.19f,-.32f,0),.045f,d.Steel);
            d.Visual("夹持零件",Primitive(PrimitiveType.Cube),d.Panels,wrist,tip+V(0,-.32f,0),V(.34f,.23f,.41f),Quaternion.identity);
        }
        static void Link(DistrictSet d,Transform t,Vector3 end)
        {
            d.Limb(t,Vector3.zero,end,.148f,d.Gold);
            d.Limb(t,V(0,0,.13f),end+V(0,0,.13f),.055f,d.Steel);
            d.Limb(t,V(0,0,-.17f),end+V(0,0,-.17f),.042f,d.Dark);
            d.Limb(t,V(0,0,-.21f),V(0,0,.21f),.212f,d.Dark);
            d.Limb(t,V(0,0,-.234f),V(0,0,-.212f),.148f,d.Steel);
        }
        public void Demonstrate(){if(elapsed<0) elapsed=0;}
        void Update()
        {
            if(elapsed<0) return;
            elapsed+=Time.deltaTime;
            float t=elapsed<1.8f?Mathf.SmoothStep(0,1,elapsed/1.8f):elapsed<=2.7f?1:1-Mathf.SmoothStep(0,1,(elapsed-2.7f)/1.9f);
            shoulder.localRotation=Quaternion.AngleAxis(13*t,Vector3.forward);elbow.localRotation=Quaternion.AngleAxis(11*t,Vector3.forward);wrist.localRotation=Quaternion.AngleAxis(-12*t,Vector3.forward);
            if(elapsed>=4.6f) elapsed=-1;
        }
    }
}

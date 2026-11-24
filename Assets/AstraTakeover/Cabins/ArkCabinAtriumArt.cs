using UnityEngine;
using System.Collections.Generic;

public sealed partial class ArkCabinWing {
    // All new ground obstacles stay outside the entrance-to-crossroads and side-portal axes.
    void BuildAtriumArt(){
        const string limestone="BAAA8C", bronze="856A4F", ivory="E3D3B3", roof="!DFCBB0", sky="!B8D6DE";
        // Cold clerestory to starboard, carved stone terraces to port.
        var sections = new[]{V(-9,6.3f,0),V(-6.6f,8.4f,0),V(-2.2f,10.3f,0),V(3.8f,10.7f,0),V(8.2f,9.2f,0),V(9,6.1f,0)};
        for(int j=0;j<5;j++){
            var a=sections[j];var b=sections[j+1];var d=b-a;
            var t=Part("Daylight vault / bay "+j,PrimitiveType.Cube,(a+b)*.5f+V(0,0,12),V(d.magnitude+.06f,.12f,24),j==1||j==2||j==3?sky:roof);
            t.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);
            for(int bay=0;bay<=8;bay++) Strut(a+V(0,0,bay*3),b+V(0,0,bay*3),.23f,bronze);
            Strut(a+V(0,0,0),a+V(0,0,24),.13f,ivory);
        }
        Box("High west masonry",-8.89f,6.15f,12,.18f,1.45f,24,limestone);
        Box("High east masonry",8.89f,6.01f,12,.18f,1.42f,24,limestone);
        Box("South clerestory header",0,8.7f,.1f,18,4.2f,.2f,ivory);
        Box("North mosaic gable",0,9.2f,23.95f,18,2.4f,.23f,ivory);
        Box("Wide west balcony",-5.6f,3.37f,19.4f,3.9f,.4f,8.9f,limestone,true);
        Box("Upper west terrace",-7.25f,6.05f,17.8f,3.25f,.38f,11.5f,limestone);
        Box("East elevator landing",7.5f,3.37f,18.35f,2.7f,.37f,5f,limestone,true);
        Box("Crossing / masonry bridge",.55f,3.36f,19.8f,13.65f,.36f,2.4f,limestone,true);
        for(int i=0;i<18;i++){
            var z=22.99f-i*.46f;
            Box("West market stairs",-8.01f,(i+1)*.197f/2,z,2.04f,(i+1)*.197f,.47f,ivory,true);
        }
        // Upper circulation retains a stone curb plus real parapet collider.
        foreach(int side in new[]{-1,1}){
            Box("Bridge parapet",.55f,4.02f,19.8f+side*1.17f,13.65f,.96f,.12f,bronze,true);
            for(int i=0;i<17;i++)Box("Bridge inlaid pale panel",-5.82f+i*.79f,4.02f,19.8f+side*1.09f,.61f,.68f,.05f,ivory);
        }
        Box("West balcony inner parapet",-3.68f,4.02f,17.05f,.12f,.96f,3.8f,bronze,true);
        Box("West balcony outer parapet",-3.68f,4.02f,22.29f,.12f,.96f,2.55f,bronze,true);
        Box("East landing end stop",7.5f,4.02f,20.8f,2.7f,.96f,.12f,bronze,true);
        Box("East landing end stop",7.5f,4.02f,15.95f,2.7f,.96f,.12f,bronze,true);
        // Visible landmark: brass panoramic lift cage. Decorative: its balcony is reached via the bridge.
        for(int i=0;i<8;i++){
            float a=i*Mathf.PI/4;
            Strut(V(6.15f+Mathf.Cos(a)*1.1f,0,22+Mathf.Sin(a)*1.1f),V(6.15f+Mathf.Cos(a)*1.1f,8.85f,22+Mathf.Sin(a)*1.1f),.113f,bronze);
        }
        foreach(float h in new[]{.19f,2.4f,5.05f,8.6f}){
            Part("Lift crown",PrimitiveType.Cylinder,V(6.15f,h,22),V(2.28f,.11f,2.28f),brass,h==.19f);
        }
        for(int i=0;i<18;i++){
            var z=1.25f+i*1.26f;
            Box("Stoneway cross seam",-1.05f,.004f,z,4.4f,.008f,.025f,"918D83");
            Box("Brass utility ribbon",1.45f,.006f,z,.15f,.009f,1.22f,bronze);
        }
        for(int i=0;i<4;i++){
            float z=2.4f+i*.99f;
            Box("Market stall canopy",-7.1f,2.56f,z,2.8f,.13f,1.02f,i%2==0?"BE774D":ivory);
        }
        Box("Market ledge",-7.7f,.48f,4.1f,2,.96f,4.9f,limestone,true);
        Box("East parterre",6.45f,.43f,5.85f,3.05f,.86f,4.3f,limestone,true);
        Box("West balcony garden",-6.5f,3.84f,21.69f,1.6f,.53f,3.2f,limestone);
        Box("West terrace garden",-7.07f,6.52f,16.1f,2.8f,.55f,2.8f,limestone);
        Tree(V(6.4f,.89f,5.1f),2.6f,71);Tree(V(7.08f,.89f,6.52f),1.9f,915);
        Tree(V(-6.5f,4.1f,21.69f),1.45f,214);Tree(V(-7.07f,6.8f,16.1f),1.6f,625);
        Lamp(V(2,7.8f,8),new Color(.89f,.96f,1),17,2.1f);
        Lamp(V(-5.1f,2.6f,4),new Color(1,.73f,.43f),9,1.3f);
    }
    void Tree(Vector3 p,float h,int seed){
        var rand=new System.Random(seed);var o=Group("Orchard specimen "+seed,p);var vertices=new List<Vector3>();var tris=new List<int>();
        Vector3 fork=V(.10f,h*.64f,.04f);
        Strut(p,p+fork,.17f,"675444");
        for(int branch=0;branch<7;branch++){
            float a=branch*2.39996f;Vector3 radial=V(Mathf.Cos(a),0,Mathf.Sin(a));
            Vector3 basePt=fork*(.64f+branch*.055f);
            Vector3 tip=fork+radial*h*(.24f+(branch%3)*.04f)+V(0,h*(.15f+branch*.032f),0);
            Strut(p+basePt,p+tip,.067f,"675444");
            for(int twig=0;twig<4;twig++){
                Vector3 from=Vector3.Lerp(basePt,tip,.53f+twig*.13f);
                Vector3 side=V(-radial.z,0,radial.x)*(twig%2==0?1:-1);
                Vector3 to=from+(side*.15f+radial*.075f+Vector3.up*.1f)*h;
                Strut(p+from,p+to,.027f,"746049");
                for(int leaf=0;leaf<14;leaf++){
                    Vector3 c=Vector3.Lerp(from,to,(float)rand.NextDouble())+V((float)rand.NextDouble()-.5f,(float)rand.NextDouble()-.5f,(float)rand.NextDouble()-.5f)*h*.31f;
                    var axis=(radial+side*((float)rand.NextDouble()-.5f)*2+Vector3.up*((float)rand.NextDouble()-.5f)).normalized*h*.095f;
                    var perpendicular=Vector3.Cross(axis,V(.15f,1,.1f)).normalized*h*.047f;
                    int n=vertices.Count;vertices.Add(c-axis);vertices.Add(c-perpendicular);vertices.Add(c+axis);vertices.Add(c+perpendicular);
                    vertices.Add(c+Vector3.up*h*.02f);
                    for(int k=0;k<4;k++){
                        int x=n+k,y=n+(k+1)%4;tris.Add(x);tris.Add(y);tris.Add(n+4);tris.Add(y);tris.Add(x);tris.Add(n+4);
                    }
                }
            }
        }
        var mesh=new Mesh{name="Folded orchard leaves"};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        o.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        var mr=o.gameObject.AddComponent<MeshRenderer>();mr.sharedMaterial=Paint(seed%2==0?"759458":"587342");
        mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        artMeshes.Add(mesh);
    }
    readonly List<Mesh> artMeshes=new List<Mesh>();
    void ReleaseAtriumArt(){foreach(var mesh in artMeshes)if(mesh)Destroy(mesh);}
}

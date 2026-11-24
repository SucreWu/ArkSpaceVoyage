using UnityEngine;
public sealed partial class ArkCabinWing {
    void BuildAtriumThreshold(){
        const string trav="BDAC92", pale="D8C7A8", copper="A47E55", dark="51554D", face="796544", teal="677D79", amber="!E9C491";
        // Existing residence action at z=.22 is intentionally unchanged.
        Box("Transfer vestibule floor",0,-.18f,-2.1f,4.1f,.36f,4.25f,trav,true);
        Box("Transfer vestibule lid",0,3.27f,-2.08f,4.32f,.35f,4.26f,trav,true);
        Box("Residential lift bulkhead",0,1.58f,-4.23f,4.32f,3.32f,.2f,dark,true);
        Box("Recessed warm lintel",0,3.67f,.19f,4.5f,.95f,.47f,trav,true);
        Box("Lift brass case",0,1.46f,-4.064f,2.85f,2.93f,.12f,copper);
        Box("Lift enamel leaves",0,1.34f,-3.984f,2.50f,2.65f,.06f,teal);
        Box("Lift central joint",0,1.34f,-3.943f,.035f,2.64f,.02f,dark);
        Box("Arrival indicator",0,2.80f,-3.975f,.44f,.10f,.05f,amber);
        Sign("RESIDENTIAL DECK  /  01",V(0,3.68f,.442f),180);
        for(int side=-1;side<=1;side+=2){
            Box("Vestibule limestone wall",side*2.08f,1.57f,-2.08f,.25f,3.15f,4.22f,trav,true);
            Box("Main moulded pilaster",side*2.17f,1.70f,.20f,.48f,3.4f,.56f,pale,true);
            Box("Stone plinth apron",side*5.53f,.40f,.24f,6.43f,.80f,.25f,trav);
            for(int bay=0;bay<3;bay++){
                float x=side*(3.03f+bay*1.82f);
                Box("Wall mullion",x,.0f+2.23f,.25f,.23f,3.02f,.25f,copper);
                Box("Porous stone dado",x+side*.82f,1.49f,.205f,1.37f,.97f,.125f,bay==1?trav:pale);
                Box("Walnut spandrel",x+side*.82f,2.65f,.212f,1.37f,1.18f,.13f,bay==1?dark:face);
            }
            Box("Wall rail",side*5.54f,3.38f,.26f,6.5f,.12f,.23f,copper);
            for(int rib=0;rib<3;rib++){
                float z=-.62f-rib*1.24f;
                Box("Cast portal rib",side*1.91f,1.56f,z,.16f,3.12f,.17f,copper,true);
                Box("Copper ceiling tie",0,3.04f,z,3.76f,.16f,.17f,copper);
                Box("Recessed milk glass",side*1.55f,2.94f,z,.42f,.048f,.19f,amber);
            }
        }
        // Drawn over the deck in thin noncolliding inlays; surface itself stays flush.
        for(int row=0;row<12;row++){
            float z=-3.82f+row*.58f;
            for(int col=0;col<5;col++){
                float x=(col-2)*.673f;
                string stoneCol=(row+col*3)%5==0?"AF9A80":((row+col)%3==0?"C6B091":pale);
                Box("Honed stone tessera",x,.016f,z,.658f,.018f,.565f,stoneCol);
            }
        }
        Lamp(V(0,2.65f,-2.65f),new Color(1,.86f,.67f),6.8f,1.75f);
        Lamp(V(-3.15f,2.8f,1.55f),new Color(1,.79f,.57f),7,1.1f);
    }
}

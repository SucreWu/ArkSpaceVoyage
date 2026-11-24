using UnityEditor;
using UnityEngine;
using System.IO;

namespace ArkWalk.Art {
    // Local remap only: never touch project-wide material import settings.
    public sealed class DewfallImporter : AssetPostprocessor {
        const string Dir = "Assets/AstraTakeover/Art/ecology/";
        static readonly string[] Names = { "SpunCeramic","BrushedAlloy","GraphiteIron","Substrate","Sage","MatureLeaf","SilverLeaf","CopperReturn" };
        static readonly Color[] Albedos = { new Color(.69f,.67f,.57f),new Color(.49f,.50f,.47f),new Color(.16f,.18f,.17f),new Color(.14f,.10f,.07f),new Color(.29f,.39f,.19f),new Color(.15f,.27f,.14f),new Color(.41f,.48f,.27f),new Color(.56f,.30f,.16f) };
        static readonly float[] Metals = {0,.78f,.65f,0,0,0,0,.67f};
        static readonly float[] Smooth = {.36f,.42f,.31f,.12f,.33f,.38f,.34f,.39f};
        public override uint GetVersion() => 2;
        void OnPreprocessModel() {
            if(assetPath!=Dir+"Dewfall.obj")return;
            var mi=(ModelImporter)assetImporter;
            mi.globalScale=1; mi.useFileScale=false;
            mi.importNormals=ModelImporterNormals.Calculate;
            mi.normalSmoothingAngle=54;mi.importTangents=ModelImporterTangents.None;
            mi.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            mi.materialLocation=ModelImporterMaterialLocation.InPrefab;
        }
        void OnPostprocessMaterial(Material mat){
            if(assetPath!=Dir+"Dewfall.obj")return;
            int i=System.Array.IndexOf(Names,mat.name);if(i<0)return;
            var shader=Shader.Find("Standard"); if(!shader)return;
            mat.shader=shader;mat.color=Albedos[i];mat.SetFloat("_Metallic",Metals[i]); mat.SetFloat("_Glossiness",Smooth[i]);
        }
        [MenuItem("Ark Walk/Art/Verify Dewfall (no scene changes)")]
        public static void Verify() {
            var obj=AssetDatabase.LoadAssetAtPath<GameObject>(Dir+"Dewfall.obj");
            if(!obj)throw new FileNotFoundException(Dir+"Dewfall.obj");
            var hit=new System.Collections.Generic.HashSet<string>();
            foreach(var r in obj.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials){
                if(!m||m.shader.name!="Standard")throw new System.Exception("Expected Built-in Standard");
                int i=System.Array.IndexOf(Names,m.name);
                if(i<0||Mathf.Abs(m.GetFloat("_Metallic")-Metals[i])>.01f||Mathf.Abs(m.GetFloat("_Glossiness")-Smooth[i])>.01f)
                    throw new System.Exception("Dewfall remap missing: "+m.name);
                hit.Add(m.name);
            }
            if(hit.Count!=Names.Length)throw new System.Exception("Incorrect material count: "+hit.Count);
            Debug.Log("Dewfall: eight Standard materials verified");
        }
    }
}

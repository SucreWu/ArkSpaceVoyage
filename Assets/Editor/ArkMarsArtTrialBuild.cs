using UnityEditor;
using UnityEngine;
public static class ArkMarsArtTrialBuild {
    public static void Windows() {
        const string prefix="Assets/Resources/Districts/mars_trial_";
        var shader=Shader.Find("Standard");
        var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"atlas.png");
        var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"normal.png");
        if(!shader || !atlas || !normal || ((TextureImporter)AssetImporter.GetAtPath(prefix+"normal.png")).textureType!=TextureImporterType.NormalMap)
            throw new System.InvalidOperationException("Mars configured texture imports required");
        var path=prefix+"lit.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
        material.mainTexture=atlas;
        material.SetFloat("_Glossiness",.10f);
        material.SetTexture("_BumpMap",normal);material.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
        ArkWalk.Editor.ArkBuild.Windows();
    }
}

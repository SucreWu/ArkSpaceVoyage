using System;
using UnityEditor;
using UnityEngine;
namespace ArkWalk.Editor
{
    internal static class ArkTakeoverArt
    {
        internal static void BuildPrefabs()
        {
            const string model="Assets/AstraTakeover/Art/ecology/Dewfall.obj";
            AssetDatabase.ImportAsset(model,ImportAssetOptions.ForceSynchronousImport);
            ArkWalk.Art.DewfallImporter.Verify();
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(model);
            if(!source)throw new InvalidOperationException("Dewfall model missing");
            if(!AssetDatabase.IsValidFolder("Assets/Resources/Astra")) AssetDatabase.CreateFolder("Assets/Resources","Astra");
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(source);
            try { PrefabUtility.SaveAsPrefabAsset(instance,"Assets/Resources/Astra/Dewfall.prefab"); }
            finally {UnityEngine.Object.DestroyImmediate(instance);}
            AssetDatabase.SaveAssets();
        }
    }
}

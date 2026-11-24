using System;
using System.IO;
using System.Reflection;
using UnityEditor;
public static class ArkBakeNeighborAtlases {
    public static void NeptuneWindows() {
        var type=typeof(ArkWalk.WalkApp).Assembly.GetType("ArkWalk.ArkIceGiants",true);
        var method=type.GetMethod("BakeAcceptedNeptune",BindingFlags.Static|BindingFlags.NonPublic);
        if(method==null)throw new InvalidOperationException("Accepted Neptune atlas algorithm not found");
        var bytes=(byte[])method.Invoke(null,null);
        if(bytes.Length!=768*384*4)throw new InvalidOperationException("Bad Neptune atlas byte count");
        const string path="Assets/Resources/Districts/neptune_storm_accepted_rgba32.bytes";
        if(File.Exists(path)){
            byte[] prior=File.ReadAllBytes(path);
            if(prior.Length!=bytes.Length)throw new InvalidOperationException("Existing Neptune atlas has another size");
            for(int i=0;i<bytes.Length;i++)if(prior[i]!=bytes[i])
                throw new InvalidOperationException("Existing Neptune atlas bytes differ; not overwritten");
        }else using(var stream=new FileStream(path,FileMode.CreateNew))stream.Write(bytes,0,bytes.Length);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        ArkWalk.Editor.ArkBuild.Windows();
    }
    public static void Windows() {
        var type=typeof(ArkWalk.WalkApp).Assembly.GetType("ArkWalk.ArkSolarNeighbors",true);
        var method=type.GetMethod("BakeAcceptedSurface",BindingFlags.Static|BindingFlags.NonPublic);
        if(method==null)throw new InvalidOperationException("Production atlas algorithm not found");
        foreach(bool venus in new[]{false,true}){
            var data=(byte[])method.Invoke(null,new object[]{venus});
            if(data.Length!=768*384*4)throw new InvalidOperationException("Bad atlas byte count");
            var name="Assets/Resources/Districts/"+(venus?"venus":"mercury")+"_surface_accepted_rgba32.bytes";
            if(File.Exists(name)){
                byte[] before=File.ReadAllBytes(name);
                if(before.Length!=data.Length)throw new InvalidOperationException("Atlas already exists with different bytes: "+name);
                for(int i=0;i<data.Length;i++)if(data[i]!=before[i])throw new InvalidOperationException("Atlas already exists with different bytes: "+name);
            }else{
                using(var file=new FileStream(name,FileMode.CreateNew))file.Write(data,0,data.Length);
            }
            AssetDatabase.ImportAsset(name,ImportAssetOptions.ForceSynchronousImport);
        }
        ArkWalk.Editor.ArkBuild.Windows();
    }
}

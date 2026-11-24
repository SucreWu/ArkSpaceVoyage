using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk.Editor
{
    public static class ArkBuild
    {
        private const string Scene = "Assets/Scenes/OrbitalStart.unity";
        private const string Package = "cn.orbit.arkwalk.unity";

        [MenuItem("世代舰/构建 Android 试玩包")]
        public static void Android()
        {
            RequireTarget(BuildTarget.Android);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, Package);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.Android.bundleVersionCode = 2;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            Build(BuildTarget.Android, "世代舰漫游.apk");
        }

        [MenuItem("世代舰/构建 Windows 试玩包")]
        public static void Windows()
        {
            RequireTarget(BuildTarget.StandaloneWindows64);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            // No interior prefabs generated.
            PlayerSettings.companyName="ArkWalkAstra";
            PlayerSettings.productName="ArkWalk Takeover";
            Build(BuildTarget.StandaloneWindows64, Path.Combine("Windows", "ArkSpaceVoyage.exe"));
        }

        private static void RequireTarget(BuildTarget target)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
                throw new BuildFailedException("请先在构建配置中切换到 " + target + "，或从 Tools/构建试玩包.ps1 启动。");
        }

        private static void Build(BuildTarget target, string filename)
        {
            PlayerSettings.productName = "ArkSpace Voyage";
            PlayerSettings.companyName = "世代舰工作室";
            PlayerSettings.bundleVersion = "1.0.1";
            PlayerSettings.runInBackground = true;
            GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.renderPipeline = null;
            IncludeStandardShader();
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene))
                throw new BuildFailedException("缺少漫游入口场景。");
            var voices = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Resources/Voices" });
            AssetDatabase.SaveAssets();
            string output = Environment.GetEnvironmentVariable("ARKWALK_BUILD_DIR");
            if (string.IsNullOrWhiteSpace(output))
                output = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
            string path = Path.GetFullPath(Path.Combine(output, filename));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            BuildReport report = BuildPipeline.BuildPlayer(new[] { Scene }, path, target, BuildOptions.None);
            var result = report.summary;
            string detail = $"引擎 Unity {Application.unityVersion}\n目标 {target}\n时间 {DateTimeOffset.Now:O}\n结果 {result.result}\n错误 {result.totalErrors}\n警告 {result.totalWarnings}\n构建耗时 {result.totalTime}\n输出体积 {result.totalSize}\n离线语音 {voices.Length}\n";
            if (target == BuildTarget.Android) detail += $"应用标识 {Package}\n脚本后端 IL2CPP\n处理器 ARM64\n";
            File.WriteAllText(Path.Combine(output, target+"构建记录.txt"), detail, new UTF8Encoding(false));
            if (result.result != BuildResult.Succeeded) throw new BuildFailedException("正式构建失败，请查看 Editor 构建日志。");
            Debug.Log("Unity 试玩包生成成功 " + path);
        }

        private static void IncludeStandardShader()
        {
            var standard = Shader.Find("Standard");
            if (!standard) throw new BuildFailedException("缺少内置标准材质着色器。");
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").First();
            var serialized = new SerializedObject(asset);
            var shaders = serialized.FindProperty("m_AlwaysIncludedShaders");
            for (int i = 0; i < shaders.arraySize; i++)
                if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == standard) return;
            int at = shaders.arraySize;
            shaders.InsertArrayElementAtIndex(at);
            shaders.GetArrayElementAtIndex(at).objectReferenceValue = standard;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

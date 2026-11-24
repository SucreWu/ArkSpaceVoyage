# 世代舰漫游（ArkSpace Voyage）

一款 Unity 3D 太空探索游戏。玩家从近地空间站出发，以第一人称飞行探索土星冰环、火星峡谷和猎户座大星云。

## 项目目录

- `Assets/` 包含游戏源码、场景和资源。APK 的入口场景是 `Assets/Scenes/OrbitalStart.unity`。
- `Packages/` 和 `ProjectSettings/` 分别保存 Unity 依赖和项目设置。
- `Builds/` 存放本地打包产物。按下文步骤可生成 Android 试玩包。

## Android APK 打包

### 构建环境

- Unity Editor **6000.6.10f1**，以 `ProjectSettings/ProjectVersion.txt` 为准。
- 通过 Unity Hub 安装 Android Build Support、Android SDK & NDK Tools 和 OpenJDK，并确认 SDK 中已安装 Android API 36。参见 [Unity Android 环境设置](https://docs.unity3d.com/cn/current/Manual/android-sdksetup.html)。
- 确认 Unity 许可证可用。首次打开工程时，等待 Unity 导入资源并生成 `Library/`。

### 在 Unity 编辑器中打包

1. 使用 Unity Hub 打开本项目目录。
2. 打开 **File → Build Profiles**，选择 Android 并将其切换为当前构建平台。参见 [Unity Android 构建说明](https://docs.unity3d.com/cn/current/Manual/android-BuildProcess.html)。
3. 在 Unity 顶部菜单选择 **世代舰 → 构建 Android 试玩包**。此入口会选用 `OrbitalStart.unity` 并设置 Android 打包参数。
4. 默认产物为 `Builds/世代舰漫游.apk`，同目录会生成 `Android构建记录.txt`。设置 `ARKWALK_BUILD_DIR` 可指定输出目录。

### 在 Windows PowerShell 中打包

关闭当前项目的 Unity 编辑器窗口，保持 Unity Hub 已登录。在项目根目录执行以下命令。如果 Unity 安装在其他目录，修改 `$unity`。

```powershell
$project = (Get-Location).Path
$unity = 'D:\program\game\Unity\6000.6.10f1\Editor\Unity.exe'
$output = Join-Path $project 'Builds\Android'
$log = Join-Path $output 'Unity构建日志.log'
New-Item -ItemType Directory -Path $output -Force | Out-Null

$previousOutput = $env:ARKWALK_BUILD_DIR
try {
    $env:ARKWALK_BUILD_DIR = $output
    $buildArgs = @(
        '-batchmode', '-quit',
        '-projectPath', ('"{0}"' -f $project),
        '-buildTarget', 'Android',
        '-executeMethod', 'ArkWalk.Editor.ArkBuild.Android',
        '-logFile', ('"{0}"' -f $log)
    )
    $build = Start-Process -FilePath $unity -ArgumentList $buildArgs -WindowStyle Hidden -PassThru
    $build.WaitForExit()
    if ($build.ExitCode -ne 0) {
        throw "打包失败，退出码为 $($build.ExitCode)。请查看 $log"
    }
}
finally {
    [Environment]::SetEnvironmentVariable('ARKWALK_BUILD_DIR', $previousOutput, 'Process')
}
```

命令会将 APK、`Android构建记录.txt` 和 `Unity构建日志.log` 写入 `Builds/Android/`。确认构建记录显示 `结果 Succeeded`、`错误 0` 后，可将 APK 安装到 Android 8.0 或更高版本的 ARM64 设备。

### 当前打包参数

| 项目 | 值 |
| --- | --- |
| 应用标识 | `cn.orbit.arkwalk.unity` |
| 版本 | `1.0.1`（版本号 `2`） |
| 脚本后端与架构 | IL2CPP，ARM64（`arm64-v8a`） |
| Android SDK | 最低 API 26，目标 API 36 |
| 图形接口 | OpenGLES 3 |
| 签名 | Unity 默认调试签名，用于本地试玩 |

构建参数由 [ArkBuild.cs](Assets/Editor/ArkBuild.cs) 中的 `ArkWalk.Editor.ArkBuild.Android` 设置。

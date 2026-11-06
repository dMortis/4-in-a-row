using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class WebReleaseSettings
{
    [MenuItem("Tools/Apply itch.io Web Release Settings")]
    public static void Apply()
    {
        var target = NamedBuildTarget.WebGL;
        PlayerSettings.productName = "4 in a Row";
        PlayerSettings.defaultWebScreenWidth = 1920;
        PlayerSettings.defaultWebScreenHeight = 1080;
        // itch.io applies HTTP compression on its CDN; plain files also work on local hosts.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.initialMemorySize = 256;
        PlayerSettings.WebGL.maximumMemorySize = 1024;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.powerPreference = WebGLPowerPreference.HighPerformance;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.stripUnusedMeshComponents = true;
        PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Medium);
        PlayerSettings.SetIl2CppCodeGeneration(target, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetApiCompatibilityLevel(target, ApiCompatibilityLevel.NET_Standard);
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
        EditorUserBuildSettings.development = false;
        int previous = QualitySettings.GetQualityLevel();
        QualitySettings.SetQualityLevel(3, false);
        QualitySettings.antiAliasing = 4;
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.SetQualityLevel(previous, false);
        foreach (string path in new[]{"Assets/Art/Front Part.png","Assets/Art/Circle Outlines.png","Assets/Art/Stand.png","Assets/Art/Backgroud.png"})
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("itch.io release: 1920x1080, High textures, 4x AA, target 60 fps, CDN compression, 256 MB initial heap.");
    }
}


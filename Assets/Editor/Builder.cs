using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

public class MobileBuild
{
    static string[] GetEnabledScenes()
    {
        return (
                   from scene in EditorBuildSettings.scenes
                   where scene.enabled
                   where !string.IsNullOrEmpty(scene.path)
                   select scene.path
               ).ToArray();
    }

    private static void BuildAndroid()
    {
        // Setting for Android
        EditorPrefs.SetBool("NdkUseEmbedded", true);
        EditorPrefs.SetBool("SdkUseEmbedded", true);
        EditorPrefs.SetBool("JdkUseEmbedded", true);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Build
        bool result = Build(BuildTarget.Android);

        // Exit Editor
        EditorApplication.Exit(result ? 0 : 1);
    }

    private static void BuildIOS()
    {
        // Setting for iOS
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Debug;

        // Build
        bool result = Build(BuildTarget.iOS);

        // Exit Editor
        EditorApplication.Exit(result ? 0 : 1);
    }

    private static bool Build(BuildTarget buildTarget)
    {
        // Get Env
        string   outputPath   = GetEnvVar("OUTPUT_PATH");               // Output path
        string   bundleId     = GetEnvVar("BUNDLE_ID");                 // Bundle Identifier
        string   productName  = GetEnvVar("PRODUCT_NAME");              // Product Name
        string   companyName  = GetEnvVar("COMPANY_NAME");              // Company Name

        outputPath = AddExpand(buildTarget, outputPath);

        Debug.Log("[MobileBuild] Build OUTPUT_PATH :" + outputPath);
        Debug.Log("[MobileBuild] Build BUILD_SCENES :" + String.Join("", GetEnabledScenes()));

        // Player Settings
        BuildOptions buildOptions;
        buildOptions = BuildOptions.Development | BuildOptions.CompressWithLz4;

        if (!string.IsNullOrEmpty(companyName)) { PlayerSettings.companyName = companyName; }

        if (!string.IsNullOrEmpty(productName)) { PlayerSettings.productName = productName; }

        if (!string.IsNullOrEmpty(bundleId)) { PlayerSettings.applicationIdentifier = bundleId; }

        // Build
        var report = BuildPipeline.BuildPlayer(GetEnabledScenes(), outputPath, buildTarget, buildOptions);
        var summary = report.summary;

        // Build Report
        for (int i = 0; i < report.steps.Length; ++i)
        {
            var step = report.steps[i];
            Debug.Log($"{step.name} Depth:{step.depth} Duration:{step.duration}");

            for (int d = 0; d < step.messages.Length; ++d)
            {
                Debug.Log($"{step.messages[d].content}");
            }
        }

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("<color=white>[MobileBuild] Build Success : " + outputPath + "</color>");
            return true;
        }
        else
        {
            Debug.Assert(false, "[MobileBuild] Build Error : " + report.name);
            return false;
        }
    }

    private static string GetEnvVar(string pKey)
    {
        return Environment.GetEnvironmentVariable(pKey);
    }

    private static string AddExpand(BuildTarget buildTarget, string outputPath)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android :
                outputPath += ".apk";
                break;
        }

        return outputPath;
    }
}

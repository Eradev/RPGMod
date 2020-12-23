using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    public static string bundleDir = "Assets/RPGMod";

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string[] assets = new string[]
        {
            "Assets/UIBorder.png"
        };

        AssetBundleBuild bundle = new AssetBundleBuild
        {
            assetBundleName = "assetBundle",
            assetNames = assets
        };

        AssetBundleBuild[] buildMap = new AssetBundleBuild[]
        {
            bundle
        };

        if (!Directory.Exists(bundleDir))
        {
            Directory.CreateDirectory(bundleDir);
        }

        BuildPipeline.BuildAssetBundles(bundleDir, buildMap, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);
    }
}
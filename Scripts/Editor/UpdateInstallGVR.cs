using FVTC.LearningInnovations.Unity.Editor.GitHub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace FVTC.LearningInnovations.Unity.MixedReality.Editor
{

    public class UpdateInstallGVR
    {
        [MenuItem("Learning Innovations/Mixed Reality/Google VR/Install Google VR")]
        private static void InstallGoogleVR()
        {
            const string releaseUrl = "https://api.github.com/repos/googlevr/gvr-unity-sdk/releases/latest";

            try
            {
                EditorUtility.DisplayProgressBar("Downloading", "Downloading Release Info from GitHub", 0.0f);

                GitHubRelease release = GitHubRelease.Download(releaseUrl, p => EditorUtility.DisplayProgressBar("Downloading", "Downloading Release Info from GitHub", p));

                EditorUtility.ClearProgressBar();
                
                if (release != null && release.assets != null)
                {
                    var asset = release.assets.Where(x => x.browser_download_url.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (asset != null && asset.browser_download_url != null)
                    {
                        WWW unityPackageDownload = new WWW(asset.browser_download_url);

                        EditorUtility.DisplayProgressBar("Downloading", "Downloading Google VR Unity Package from GitHub", 0.0f);

                        while (!unityPackageDownload.isDone)
                        {
                            EditorUtility.DisplayProgressBar("Downloading", "Downloading Google VR Unity Package from GitHub", unityPackageDownload.progress);
                        }

                        EditorUtility.ClearProgressBar();

                        if (unityPackageDownload.error != null)
                        {
                            Debug.LogError(unityPackageDownload.error);
                        }
                        else
                        {
                            string folderPath = FileUtil.GetUniqueTempPathInProject();

                            if (!System.IO.Directory.Exists(folderPath))
                            {
                                System.IO.Directory.CreateDirectory(folderPath);
                            }

                            string filePath = System.IO.Path.Combine(folderPath, asset.name);

                            System.IO.File.WriteAllBytes(filePath, unityPackageDownload.bytes);

                            AssetDatabase.ImportPackage(filePath, true);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
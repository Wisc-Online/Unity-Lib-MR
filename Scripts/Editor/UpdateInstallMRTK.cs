using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using FVTC.LearningInnovations.Unity.Helpers;

namespace FVTC.LearningInnovations.Unity.MixedReality.Editor
{
    public class UpdateInstallMRTKMenu
    {

        const string MRTK_GITHUB_URL = "https://github.com/Microsoft/MixedRealityToolkit-Unity.git";

        [MenuItem("FVTC/Learning Innovations/Mixed Reality/Install MRTK from GitHub")]
        private static void UpdateInstallMRTK()
        {
            UpdateMRTK("HoloToolkit");
        }

        [MenuItem("FVTC/Learning Innovations/Mixed Reality/Install MRTK from GitHub (include examples)")]
        private static void UpdateInstallMRTKWithExamples()
        {
            UpdateMRTK("HoloToolkit", "HoloToolkit-Examples", "HoloToolkit-Preview");
        }

        const string CHECKING_OUT_PREFIX = "Checking out files: ";
        const string RECEIVING_OBJECTS_PREFIX = "Receiving objects: ";
        const string RESOLVING_DELTAS_PREFIX = "Resolving deltas: ";

        static readonly string[] PREFIXES = new string[] { CHECKING_OUT_PREFIX, RECEIVING_OBJECTS_PREFIX, RESOLVING_DELTAS_PREFIX };

        private static void UpdateMRTK(params string[] folders)
        {
            

            if (GitHelper.PromptUserToDownloadGitIfNotInstalled())
            {
                // create temp dir
                DirectoryInfo tempDir = CreateNewTempDirectory();

                try
                {
                    DirectoryInfo assetsDir = new DirectoryInfo(Application.dataPath);

                    string gitExePath = GitHelper.GetGitPath();

                    GitClone(gitExePath, MRTK_GITHUB_URL, tempDir.FullName);

                    DirectoryInfo subDir;
                    DirectoryInfo mrtkSubDir;

                    try
                    {

                        for (int i = 0; i < folders.Length; ++i)
                        {
                            EditorUtility.DisplayProgressBar("Updating Folders", folders[i], (i + 1) / ((float)folders.Length));

                            subDir = new DirectoryInfo(Path.Combine(assetsDir.FullName, folders[i]));
                            mrtkSubDir = new DirectoryInfo(Path.Combine(tempDir.FullName, "Assets/" + folders[i]));

                            if (mrtkSubDir.Exists)
                            {
                                if (subDir.Exists)
                                {
                                    // remove existing MRTK installation from assets
                                    Delete(subDir);
                                }

                                // move from temp to assets
                                mrtkSubDir.MoveTo(subDir.FullName);
                            }
                        }
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }

                }
                finally
                {
                    // delete temp directory
                    tempDir.Refresh();
                    if (tempDir.Exists)
                    {
                        Delete(tempDir);
                    }
                }

                PlayerSettings.allowUnsafeCode = true;

                AssetDatabase.Refresh();
            }
        }

        private static void GitClone(string gitExePath, string repositoryUrl, string targetPath)
        {
            // clone MRTK to temp dir
            ProcessStartInfo gitClone = new ProcessStartInfo(gitExePath);

            gitClone.UseShellExecute = false;
            gitClone.Arguments = string.Format("clone --progress --depth 1 {0} {1}", repositoryUrl, targetPath);
            gitClone.RedirectStandardError = true;
            gitClone.CreateNoWindow = true;

            try
            {
                EditorUtility.DisplayProgressBar("Cloning MRTK from GitHub", "Starting Clone Process", 0.0f);

                gitClone.WindowStyle = ProcessWindowStyle.Hidden;

                using (var cloneProcess = Process.Start(gitClone))
                {
                    string cloneProgress;
                    float clonePercent;
                    bool showGeneralProgress;

                    while (true)
                    {
                        cloneProgress = cloneProcess.StandardError.ReadLine();

                        if (string.IsNullOrEmpty(cloneProgress))
                        {
                            break;
                        }
                        else
                        {
                            showGeneralProgress = true;

                            foreach (var prefix in PREFIXES)
                            {
                                if (cloneProgress.StartsWith(prefix))
                                {
                                    if (float.TryParse(cloneProgress.Substring(prefix.Length).Trim().Split(' ')[0].TrimEnd('%'), out clonePercent))
                                    {
                                        clonePercent = clonePercent / 100f;

                                        EditorUtility.DisplayProgressBar("Cloning MRTK from GitHub", cloneProgress, clonePercent);
                                        showGeneralProgress = false;
                                        break;
                                    }
                                }
                            }

                            if (showGeneralProgress)
                            {
                                EditorUtility.DisplayProgressBar("Cloning MRTK from GitHub", cloneProgress, 0f);
                            }
                        }
                    }

                    cloneProcess.WaitForExit();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static DirectoryInfo CreateNewTempDirectory()
        {
            string tempDirPath;

            do
            {
                tempDirPath = System.IO.Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(tempDirPath) || Directory.Exists(tempDirPath));

            var tempDir = new DirectoryInfo(tempDirPath);
            return tempDir;
        }

        static void Delete(FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo.Exists)
            {
                fileSystemInfo.Attributes = FileAttributes.Normal;

                DirectoryInfo dir = fileSystemInfo as DirectoryInfo;

                if (dir != null)
                {
                    foreach (var child in dir.GetFileSystemInfos("*"))
                    {
                        Delete(child);
                    }
                }

                fileSystemInfo.Delete();
            }
        }
    }
}
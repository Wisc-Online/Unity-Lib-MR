using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using FVTC.LearningInnovations.Unity.Helpers;
using FVTC.LearningInnovations.Unity.Editor;

namespace FVTC.LearningInnovations.Unity.MixedReality.Editor
{

    public class UpdateInstallMRTKMenu
    {

        const string MRTK_GITHUB_URL = "https://github.com/Microsoft/MixedRealityToolkit-Unity.git";
        const string FVTC_LEARNING_INNOVATIONS_PROGRAMDATA_DIR_PATH = "FVTC\\LearningInnovations";
        const string LOCAL_MRTK_REPO_FILE_NAME = "MixedRealityToolkit-Unity.git";

      

        [MenuItem("Learning Innovations/Mixed Reality/MixedRealityToolkit/Install HoloToolkit from GitHub")]
        private static void UpdateInstallHoloToolkit()
        {
            UpdateMRTK("master", "HoloToolkit");

            PlayerSettings.allowUnsafeCode = true;
            
            AssetDatabase.Refresh();
        }

        [MenuItem("Learning Innovations/Mixed Reality/MixedRealityToolkit/Install HoloToolkit from GitHub (include examples)")]
        private static void UpdateInstallHoloToolkitWithExamples()
        {
            UpdateMRTK("master", "HoloToolkit", "HoloToolkit-Examples", "HoloToolkit-Preview");

            PlayerSettings.allowUnsafeCode = true;
            
            AssetDatabase.Refresh();
        }

        [MenuItem("Learning Innovations/Mixed Reality/MixedRealityToolkit/Install MixedRealityToolkit from GitHub")]
        private static void UpdateInstallMixedRealityToolkit()
        {
            UpdateMRTK("mrtk_release", "MixedRealityToolkit");

            PlayerSettings.allowUnsafeCode = true;
            PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WSA, ApiCompatibilityLevel.NET_4_6);

            AssetDatabase.Refresh();
        }

        [MenuItem("Learning Innovations/Mixed Reality/MixedRealityToolkit/Install MixedRealityToolkit from GitHub (include examples)")]
        private static void UpdateInstallMixedRealityToolkitWithExamples()
        {
            UpdateMRTK("mrtk_release", "MixedRealityToolkit", "MixedRealityToolkit-Examples");

            PlayerSettings.allowUnsafeCode = true;

            PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WSA, ApiCompatibilityLevel.NET_4_6);

            AssetDatabase.Refresh();
        }

        const string CHECKING_OUT_PREFIX = "Checking out files: ";
        const string RECEIVING_OBJECTS_PREFIX = "Receiving objects: ";
        const string RESOLVING_DELTAS_PREFIX = "Resolving deltas: ";

        static readonly string[] PREFIXES = new string[] { CHECKING_OUT_PREFIX, RECEIVING_OBJECTS_PREFIX, RESOLVING_DELTAS_PREFIX };

        private static void UpdateMRTK(string branch, params string[] folders)
        {
            if (GitHelper.PromptUserToDownloadGitIfNotInstalled())
            {
                string gitExePath = GitHelper.GetGitPath();

                string programDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                string fvtcLIProgramDataDir = Path.Combine(programDataDir, FVTC_LEARNING_INNOVATIONS_PROGRAMDATA_DIR_PATH);

                var fvtcDir = new DirectoryInfo(fvtcLIProgramDataDir);

                if (!fvtcDir.Exists)
                {
                    fvtcDir.Create();
                }

                var localMrtkRepo = new DirectoryInfo(Path.Combine(fvtcDir.FullName, LOCAL_MRTK_REPO_FILE_NAME));

                if (localMrtkRepo.Exists)
                {
                    // do a git-fetch to update the repo from remote
                    GitFetch(gitExePath, localMrtkRepo);
                }
                else
                {
                    // do a git clone --bare to the file
                    GitCloneBare(gitExePath, MRTK_GITHUB_URL, localMrtkRepo.FullName);
                }
                
                var unityAssetsDir = new DirectoryInfo(Application.dataPath);

                GitCheckoutBranchToDir(gitExePath, branch, localMrtkRepo, unityAssetsDir.Parent, folders.Select(f => "Assets/" + f).ToArray());
                
            }
        }

        private static void GitFetch(string gitExePath, DirectoryInfo repo)
        {
            // clone MRTK to temp dir
            ProcessStartInfo gitFetch = new ProcessStartInfo(gitExePath);

            gitFetch.UseShellExecute = false;
            gitFetch.Arguments = string.Format("fetch");
            gitFetch.RedirectStandardError = true;
            gitFetch.CreateNoWindow = true;
            gitFetch.WorkingDirectory = repo.FullName;

            try
            {
                EditorUtility.DisplayProgressBar("Updating from GitHub", "Updating from GitHub", 0.0f);

                gitFetch.WindowStyle = ProcessWindowStyle.Hidden;

                using (var cloneProcess = Process.Start(gitFetch))
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

                                        EditorUtility.DisplayProgressBar("Updating from GitHub", cloneProgress, clonePercent);
                                        showGeneralProgress = false;
                                        break;
                                    }
                                }
                            }

                            if (showGeneralProgress)
                            {
                                EditorUtility.DisplayProgressBar("Updating from GitHub", cloneProgress, 0f);
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

        private static void GitCheckoutBranchToDir(string gitExePath, string branch, DirectoryInfo sourceDir, DirectoryInfo targetDir, string[] folders)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Updating from GitHub", "Updating from GitHub", 0.0f);

                for(int i = 0; i < folders.Length; ++i)
                {
                    EditorUtility.DisplayProgressBar(string.Format("Updating {0}", folders[i]), string.Format("Updating {0}", folders[i]), i / (float)folders.Length);

                    // clone MRTK to temp dir
                    ProcessStartInfo gitFetch = new ProcessStartInfo(gitExePath);

                    gitFetch.UseShellExecute = false;
                    gitFetch.Arguments = string.Format("--work-tree=\"{0}\" checkout \"{1}\" -- \"{2}\"", targetDir.FullName, branch, folders[i]);
                    gitFetch.RedirectStandardError = true;
                    gitFetch.CreateNoWindow = true;
                    gitFetch.WorkingDirectory = sourceDir.FullName;

                    gitFetch.WindowStyle = ProcessWindowStyle.Hidden;

                    using (var gitProcess = Process.Start(gitFetch))
                    {
                        string cloneProgress;
                        
                        while (true)
                        {
                            cloneProgress = gitProcess.StandardError.ReadLine();

                            if (string.IsNullOrEmpty(cloneProgress))
                            {
                                break;
                            }
                            else
                            {
                            }
                        }

                        gitProcess.WaitForExit();
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void GitCloneBare(string gitExePath, string remoteUrl, string targetPath)
        {
            // clone MRTK to temp dir
            ProcessStartInfo gitClone = new ProcessStartInfo(gitExePath);

            gitClone.UseShellExecute = false;
            gitClone.Arguments = string.Format("clone --bare --progress {0} {1}", remoteUrl, targetPath);
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
using System;
using UnityEditor;
using System.Linq;
using FVTC.LearningInnovations.Unity.Editor;

namespace FVTC.LearningInnovations.Unity.MixedReality.Editor
{
    public class MixedRealityUpdater
    {
        const string UNITY_LIB_MR_GITHUB_URL = "https://github.com/Wisc-Online/Unity-Lib-MR.git";

        [MenuItem("Learning Innovations/Mixed Reality/Update from GitHub")]
        private static void Update()
        {
            if (GitHelper.UpdateSubmodule(UNITY_LIB_MR_GITHUB_URL))
            {
                AssetDatabase.Refresh();
            }
            else
            {
                Dialog.Close("Update Failed", "Updating from GitHub failed.  See console for details.");
            }
        }

        [MenuItem("Learning Innovations/Mixed Reality/Update from GitHub", true)]
        private static bool ValidateUpdate()
        {
            return GitHelper.GetModules().Where(x => UNITY_LIB_MR_GITHUB_URL.Equals(x.Url.AbsoluteUri, StringComparison.OrdinalIgnoreCase)).Any();
        }

    }
}
using UnityEditor;
using UnityEngine;

namespace FVTC.LearningInnovations.Unity.MixedReality.Editor
{
    public class UpdateInstallOculus
    {
        [MenuItem("Learning Innovations/Mixed Reality/Oculus/Install Oculus Integration")]
        public static void InstallOculusIntegration()
        {
            const string url = "https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022";

            Application.OpenURL(url);
        }
    }
}
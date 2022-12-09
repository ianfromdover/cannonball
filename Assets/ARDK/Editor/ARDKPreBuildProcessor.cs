using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_ANDROID && UNITY_EDITOR_OSX

class ARDKPreBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
         UnityEngine.Rendering.GraphicsDeviceType[] array = PlayerSettings.GetGraphicsAPIs(report.summary.platform);
        if (array[0] != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
        {
            throw new UnityEditor.Build.BuildFailedException("Not using OpenGLES3 as the preferred graphics api. The " +
                                                            "ARDK builds with OpenGLES3, please go to Player Settings " +
                                                            "and set OpenGLES3 as the first option in the graphics " +
                                                            "API list. You may need to uncheck the Auto Graphics API " +
                                                            "in order to see this list.");
        }   
    }
}
#endif

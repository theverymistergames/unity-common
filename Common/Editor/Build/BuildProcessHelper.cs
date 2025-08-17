using MisterGames.Common.Build;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MisterGames.Common.Editor.Build {
    
    internal sealed class BuildProcessHelper : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) {
            BuildInfo.IsBuildProcessing = true;
        }
        
        public void OnPostprocessBuild(BuildReport report) {
            BuildInfo.IsBuildProcessing = false;
        }
    }
    
}
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Build {
    
    public interface IPreprocessBuildWithReportPerScene {
    
        void OnPreprocessBuildForScene(BuildReport report, Scene scene);
        
    }
    
}
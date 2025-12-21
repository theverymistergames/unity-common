using Cysharp.Threading.Tasks;
using MisterGames.Common.Build;
using MisterGames.Common.Maths;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MisterGames.Common.Editor.Build {
    
    internal sealed class BuildProcessHelper : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        public int callbackOrder => 0;
        private byte _operationId;

        public void OnPreprocessBuild(BuildReport report) {
            _operationId.IncrementUncheckedRef();
            
            BuildInfo.IsBuildProcessing = true;
        }
        
        public void OnPostprocessBuild(BuildReport report) {
            StopBuildProcess().Forget();
        }

        private  async UniTask StopBuildProcess() {
            byte id = _operationId.IncrementUncheckedRef();
            
            await UniTask.Yield();
            if (id != _operationId) return;
            
            BuildInfo.IsBuildProcessing = false;
        }
    }
    
}
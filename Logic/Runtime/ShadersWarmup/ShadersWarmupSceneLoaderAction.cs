using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Logic.ShadersWarmup {
    
    [Serializable]
    public sealed class ShadersWarmupSceneLoaderAction : ISceneLoaderAction {
        
#if UNITY_EDITOR
        [SerializeField] private bool _bypassIfPlaymodeStartSceneOverriden = true;  
#endif
        [SerializeField] [Min(0f)] private float _startDelay = 0.25f;
        
        public async UniTask Apply(CancellationToken cancellationToken) {
            if (!CanApply()) return;

            await AsyncExt.DelayUnscaled(_startDelay, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            await ShaderWarmupService.Instance.Load();
        }
        
        private bool CanApply() {
            bool show = true;
            
#if UNITY_EDITOR
            show &= !_bypassIfPlaymodeStartSceneOverriden || !PlaymodeStartScenesUtils.IsPlaymodeStartScenesOverrideEnabled(out _);
#endif

            return show;
        }
    }
    
}
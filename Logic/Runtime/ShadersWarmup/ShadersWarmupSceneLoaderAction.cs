using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Loading;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Logic.ShadersWarmup {
    
    [Serializable]
    public sealed class ShadersWarmupSceneLoaderAction : ISceneLoaderAction {
        
#if UNITY_EDITOR
        [SerializeField] private bool _bypassIfPlaymodeStartSceneOverriden = true;  
#endif
        [SerializeField] [Min(-1f)] private float _fadeOutOnStart = -1f;
        [SerializeField] [Min(-1f)] private float _fadeInOnFinish = -1f;
        
        public async UniTask Apply(CancellationToken cancellationToken) {
            if (!CanApply()) return;

            await Fader.Main.FadeOutAsync(_fadeOutOnStart);
            await ShaderWarmupService.Instance.Load();
            await Fader.Main.FadeInAsync(_fadeOutOnStart);
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
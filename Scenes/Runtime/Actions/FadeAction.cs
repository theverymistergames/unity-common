using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Loading;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Scenes.Actions {
    
    [Serializable]
    public sealed class FadeAction : ISceneLoaderAction {

        [Header("Fade")]
#if UNITY_EDITOR
        [SerializeField] private bool _bypassIfPlaymodeStartSceneOverriden = true;  
#endif
        [SerializeField] private FadeMode _fadeMode;
        [SerializeField] [Min(-1f)] private float _duration = -1f;

        public async UniTask Apply(CancellationToken cancellationToken) {
            if (!CanApply()) return;

            await Fader.Main.FadeAsync(_fadeMode, _duration);
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
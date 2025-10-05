using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Labels;

namespace MisterGames.ActionLib.Libs {
    
    [Serializable]
    public sealed class EnableLibraryObjectSetAction : IActorAction {
        
        public LabelValue<HashSet<UnityEngine.Object>>[] enable;
        public LabelValue<HashSet<UnityEngine.Object>>[] disable;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < enable.Length; i++) {
                ref var value = ref enable[i];
                value.GetData()?.SetEnabled(true);
            }

            for (int i = 0; i < disable.Length; i++) {
                ref var value = ref disable[i];
                value.GetData()?.SetEnabled(false);
            }
            
            return default;
        }
    }
    
}
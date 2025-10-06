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
        
        public LabelValueMap<HashSet<UnityEngine.Object>, bool> valueMap;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < valueMap.values.Length; i++) {
                ref var data = ref valueMap.values[i];
                if (data.value.HasValue) data.label.GetData()?.SetEnabled(data.value.Value);
            }
            
            return default;
        }
    }
    
}
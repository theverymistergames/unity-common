using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using MisterGames.Common.Stats;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class RemoveAudioMixerModifierAction : IActorAction {

        public Object source;
        public string param;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Services.Get<IAudioMixerService>().RemoveModifier(source, param);
            return default;
        }
    }
    
}
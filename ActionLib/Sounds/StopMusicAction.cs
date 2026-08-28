using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Audio;
using MisterGames.Common.Service;
using UnityEngine.Audio;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class StopMusicAction : IActorAction {

        public AudioMixerGroup[] mixerGroups;
        public bool immediate;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!Services.TryGet(out IMusicService musicService)) return default;

            if (mixerGroups is { Length: > 0 }) {
                for (int index = 0; index < mixerGroups.Length; index++) {
                    musicService.StopMusic(mixerGroups[index], immediate);
                }
            }
            else {
                musicService.StopAllMusic(immediate);
            }

            return default;
        }
    }

}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Audio;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class ReleaseSoundsAction : IActorAction {

        public Mode mode;
        public bool immediate;

        public enum Mode {
            ReleaseAll,
            ReleaseTimescaled,
        }

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (AudioPool.Main is not { } pool) return default;

            switch (mode) {
                case Mode.ReleaseAll:
                    pool.ReleaseAll(immediate);
                    break;

                case Mode.ReleaseTimescaled:
                    pool.ReleaseTimescaled(immediate);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return default;
        }
    }

}

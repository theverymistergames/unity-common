using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Logic.Phys;

namespace MisterGames.ActionLib.Phys {

    [Serializable]
    public sealed class PhysExplosionAction : IActorAction {

        public PhysExplosion explosion;
        public Mode mode;

        public enum Mode {
            Explode,
            Rollback,
        }

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            switch (mode) {
                case Mode.Explode:
                    explosion.Explode();
                    break;

                case Mode.Rollback:
                    explosion.Rollback();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return UniTask.CompletedTask;
        }
    }

}

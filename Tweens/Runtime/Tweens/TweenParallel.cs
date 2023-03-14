using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenParallel : ITween {

        [SerializeReference] [SubclassSelector] public List<ITween> tweens;

        private UniTask[] _tasks;

        public void Initialize(MonoBehaviour owner) {
            int count = tweens.Count;

            for (int i = 0; i < count; i++) {
                tweens[i].Initialize(owner);
            }

            if (count > 1) _tasks = new UniTask[count];
        }

        public void DeInitialize() {
            for (int i = 0; i < tweens.Count; i++) {
                tweens[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            int count = tweens.Count;
            if (count == 0) return;

            if (count == 1) {
                await tweens[0].Play(token);
                return;
            }

            for (int i = 0; i < count; i++) {
                _tasks[i] = tweens[i].Play(token);
            }

            await UniTask.WhenAll(_tasks);
        }

        public void Wind(bool reportProgress = true) {
            for (int i = 0; i < tweens.Count; i++) {
                tweens[i].Wind(reportProgress);
            }
        }

        public void Rewind(bool reportProgress = true) {
            for (int i = 0; i < tweens.Count; i++) {
                tweens[i].Rewind(reportProgress);
            }
        }

        public void Invert(bool isInverted) {
            for (int i = 0; i < tweens.Count; i++) {
                tweens[i].Invert(isInverted);
            }
        }
    }

}

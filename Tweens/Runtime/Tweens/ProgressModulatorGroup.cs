using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class ProgressModulatorGroup : IProgressModulator {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public IProgressModulator[] modulators;

        public enum Mode {
            Sequence,
            Average,
        }
        
        public float Modulate(float progress) {
            return mode switch {
                Mode.Sequence => ModulateSequence(progress),
                Mode.Average => ModulateAverage(progress),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private float ModulateSequence(float progress) {
            for (int i = 0; i < modulators?.Length; i++) {
                progress = modulators[i].Modulate(progress);
            }

            return progress;
        }
        
        private float ModulateAverage(float progress) {
            float sum = 0f;
            int length = modulators?.Length ?? 0;

            for (int i = 0; i < length; i++) {
                sum += modulators![i].Modulate(progress);
            }

            return length > 0 ? sum / length : progress;
        }
    }
}
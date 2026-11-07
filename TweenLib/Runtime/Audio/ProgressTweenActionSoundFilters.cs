using System;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Animations {

    [Serializable]
    public sealed class ProgressTweenActionSoundFilters : ITweenProgressAction {

        public Transform transform;
        public HashId attachId;
        [SerializeField] [Range(AudioPool.HpCutoffLowerBound, AudioPool.LpCutoffUpperBound)] private float _lpCutoff0 = AudioPool.LpCutoffUpperBound;
        [SerializeField] [Range(AudioPool.HpCutoffLowerBound, AudioPool.LpCutoffUpperBound)] private float _lpCutoff1 = AudioPool.HpCutoffLowerBound;
        [SerializeField] [Range(AudioPool.HpCutoffLowerBound, AudioPool.LpCutoffUpperBound)] private float _hpCutoff0 = AudioPool.HpCutoffLowerBound;
        [SerializeField] [Range(AudioPool.HpCutoffLowerBound, AudioPool.LpCutoffUpperBound)] private float _hpCutoff1 = AudioPool.LpCutoffUpperBound;

        public void OnProgressUpdate(float progress) {
            var handle = AudioPool.Main?.GetAudioHandle(transform, attachId) ?? default;

            handle.LpCutoffLerp = Mathf.Lerp(
                NumberExtensions.InverseLerpLog(AudioPool.HpCutoffLowerBound, AudioPool.LpCutoffUpperBound, _lpCutoff0),
                NumberExtensions.InverseLerpLog(AudioPool.HpCutoffLowerBound, AudioPool.LpCutoffUpperBound, _lpCutoff1),
                progress
            );

            handle.HpCutoffLerp = Mathf.Lerp(
                NumberExtensions.InverseLerpLog(AudioPool.LpCutoffUpperBound, AudioPool.HpCutoffLowerBound, _hpCutoff0),
                NumberExtensions.InverseLerpLog(AudioPool.LpCutoffUpperBound, AudioPool.HpCutoffLowerBound, _hpCutoff1),
                progress
            );
        }
    }

}

using MisterGames.Common.Attributes;
using MisterGames.Tick.TimeProviders;
using UnityEngine;

namespace MisterGames.Tick.Core {

    [CreateAssetMenu(fileName = nameof(TimeDomain), menuName = "MisterGames/" + nameof(TimeDomain))]
    public class TimeDomain : ScriptableObject {

        [ReadOnly(ReadOnlyMode.PlayModeOnly)]
        [SerializeField] private TimerProviderType _timeProviderType;

        public ITimeSource Source => _timeSource;
        internal ITimeSourceApi SourceApi => _timeSource;
        internal TimerProviderType TimerProviderType => _timeProviderType;

        private readonly TimeSource _timeSource = new TimeSource();
    }
}

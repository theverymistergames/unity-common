using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Feedback.Perf {

    [DefaultExecutionOrder(-100_000)]
    public sealed class PerformanceLogServiceLauncher : MonoBehaviour {

        [SerializeField] private PerformanceLogServiceConfig _config;

        private readonly PerformanceLogService _performanceLogService = new();

        private void Awake() {
            _performanceLogService.Initialize(_config);
            Services.Register<IPerformanceLogService>(_performanceLogService);
        }

        private void OnDestroy() {
            Services.Unregister(_performanceLogService);
            _performanceLogService.Dispose();
        }
    }

}

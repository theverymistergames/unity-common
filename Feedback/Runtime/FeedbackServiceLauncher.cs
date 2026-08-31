using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Feedback {

    [DefaultExecutionOrder(-100_000)]
    public sealed class FeedbackServiceLauncher : MonoBehaviour {

        [SerializeField] private FeedbackServiceConfig _config;

        private readonly FeedbackService _feedbackService = new();

        private void Awake() {
            _feedbackService.Initialize(_config);
            Services.Register<IFeedbackService>(_feedbackService);
        }

        private void OnDestroy() {
            Services.Unregister(_feedbackService);
            _feedbackService.Dispose();
        }
    }

}

using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Actions {

    public static class AsyncActionExtensions {

        public static UniTask TryApply(this IAsyncAction action, object source, CancellationToken cancellationToken) {
            return action?.Apply(source, cancellationToken) ?? default;
        }
    }

}

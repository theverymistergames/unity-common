using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Actions {

    public interface IAsyncAction<in TContext> {
        UniTask Apply(TContext context, CancellationToken cancellationToken = default);
    }

}

using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Actions {

    public interface IAsyncAction<in C> {
        UniTask Apply(C context, CancellationToken cancellationToken = default);
    }

}

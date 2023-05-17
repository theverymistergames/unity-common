using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Actions {

    public interface IAsyncAction {

        void Initialize();
        void DeInitialize();

        UniTask Apply(object source, CancellationToken cancellationToken = default);
    }

}

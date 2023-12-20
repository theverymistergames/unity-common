using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;

namespace MisterGames.Character.Actions {

    public interface ICharacterAction {
        UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default);
    }

}

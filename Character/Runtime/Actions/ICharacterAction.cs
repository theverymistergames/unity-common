using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;

namespace MisterGames.Character.Actions {

    public interface ICharacterAction {
        UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default);
    }

}

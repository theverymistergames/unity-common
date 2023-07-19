using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Common.Actions {

    [CreateAssetMenu(fileName = nameof(AsyncActionAsset), menuName = "MisterGames/" + nameof(AsyncActionAsset))]
    public sealed class AsyncActionAsset : ScriptableObject, IAsyncAction, IDependency {

        [SerializeReference] [SubclassSelector] private IAsyncAction _action;

        public void OnSetupDependencies(IDependencyContainer container) {
            if (_action is IDependency dep) dep.OnSetupDependencies(container);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            if (_action is IDependency dep) dep.OnResolveDependencies(resolver);
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            return _action?.Apply(source, cancellationToken) ?? default;
        }
    }

}

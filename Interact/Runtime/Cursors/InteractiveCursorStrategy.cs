using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    [CreateAssetMenu(fileName = nameof(InteractiveCursorStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractiveCursorStrategy))]
    public sealed class InteractiveCursorStrategy : ScriptableObject, IDependency {

        [SerializeField] private Case[] _cases;

        [Serializable]
        private struct Case {
            public CursorIcon cursorIcon;
            [SerializeReference] [SubclassSelector] public ICondition constraint;
        }

        public void OnAddDependencies(IDependencyContainer container) {
            for (int i = 0; i < _cases.Length; i++) {
                if (_cases[i].constraint is IDependency dep) dep.OnAddDependencies(container);
            }
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            for (int i = 0; i < _cases.Length; i++) {
                if (_cases[i].constraint is IDependency dep) dep.OnResolveDependencies(resolver);
            }
        }

        public bool TryGetCursorIcon(out CursorIcon cursorIcon) {
            for (int i = 0; i < _cases.Length; i++) {
                var c = _cases[i];
                if (!c.constraint.IsMatched) continue;

                cursorIcon = c.cursorIcon;
                return true;
            }

            cursorIcon = default;
            return false;
        }
    }

}

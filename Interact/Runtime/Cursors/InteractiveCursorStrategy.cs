using System;
using MisterGames.Common.Attributes;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    [CreateAssetMenu(fileName = nameof(InteractiveCursorStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractiveCursorStrategy))]
    public class InteractiveCursorStrategy : ScriptableObject {

        [SerializeField] private Case[] _cases;

        [Serializable]
        private struct Case {
            public CursorIcon cursorIcon;
            [SerializeReference] [SubclassSelector] public IInteractionConstraint constraint;
        }

        public bool TryGetCursorIcon(IInteractiveUser user, IInteractive interactive, out CursorIcon cursorIcon) {
            for (int i = 0; i < _cases.Length; i++) {
                var c = _cases[i];
                if (!c.constraint.IsSatisfied(user, interactive)) continue;

                cursorIcon = c.cursorIcon;
                return true;
            }

            cursorIcon = default;
            return false;
        }
    }

}

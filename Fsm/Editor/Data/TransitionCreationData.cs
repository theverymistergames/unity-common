using MisterGames.Fsm.Core;

namespace MisterGames.Fsm.Editor.Data {

    internal class TransitionCreationData {

        public FsmState source;
        public FsmState target;
        public PendingStage pendingStage;

        internal enum PendingStage {
            None,
            PendingTarget,
            PendingTransition
        }

        public override string ToString() {
            var sourceName = source == null ? "null" : source.name;
            var targetName = target == null ? "null" : target.name;
            return $"TransitionCreationData(source = {sourceName}, target = {targetName}, stage = {pendingStage})";
        }
    }

}
using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {

        public static Job Action(ITimeSource timeSource, Action action) {
            return CreateJob<JobSystemAction, Action>(timeSource, action);
        }

    }

}

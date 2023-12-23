﻿namespace MisterGames.Common.Conditions {

    public interface ICondition {
        bool IsMatched { get; }
    }

    public interface ICondition<in TContext> {
        bool IsMatch(TContext context);
    }

}

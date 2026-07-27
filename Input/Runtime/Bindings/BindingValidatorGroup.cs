using System;
using MisterGames.Common.Conditions;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class BindingValidatorGroup : ConditionGroup<IBindingValidator, string> { }
    
}
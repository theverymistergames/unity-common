using System;
using MisterGames.Common.Lists;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class AxisBindingListValidator : IBindingValidator {

        public AxisBinding[] allowAxes;
        
        public bool IsMatch(string context) {
            var (axis, dir) = InputBindingExtensions.ToAxisBinding(context);
            return axis != AxisBinding.None && allowAxes.Contains(axis);
        }
    }
    
}
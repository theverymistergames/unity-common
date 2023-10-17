using System;
using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints {

    public static class PortExtensions {

        public static Port Hidden(this Port port, bool isHidden) {
            if (isHidden) port.options |= PortOptions.Hidden;
            else port.options &= ~PortOptions.Hidden;

            return port;
        }

        public static Port Layout(this Port port, PortLayout layout) {
            switch (layout) {
                case PortLayout.Default:
                    port.mode &= ~(PortMode.LayoutLeft | PortMode.LayoutRight);
                    break;

                case PortLayout.Left:
                    port.mode &= ~PortMode.LayoutRight;
                    port.mode |= PortMode.LayoutLeft;
                    break;

                case PortLayout.Right:
                    port.mode &= ~PortMode.LayoutLeft;
                    port.mode |= PortMode.LayoutRight;
                    break;
            }

            return port;
        }

        public static Port Capacity(this Port port, PortCapacity capacity) {
            switch (capacity) {
                case PortCapacity.Default:
                    port.mode &= ~(PortMode.CapacitySingle | PortMode.CapacityMultiple);
                    break;

                case PortCapacity.Single:
                    port.mode &= ~PortMode.CapacityMultiple;
                    port.mode |= PortMode.CapacitySingle;
                    break;

                case PortCapacity.Multiple:
                    port.mode &= ~PortMode.CapacitySingle;
                    port.mode |= PortMode.CapacityMultiple;
                    break;
            }

            return port;
        }

        internal static bool IsInput(this Port port) {
            return port.mode.HasFlag(PortMode.Input);
        }

        internal static bool IsData(this Port port) {
            return port.mode.HasFlag(PortMode.Data);
        }

        internal static bool IsExternal(this Port port) {
            return port.options.HasFlag(PortOptions.External);
        }

        internal static bool IsHidden(this Port port) {
            return port.options.HasFlag(PortOptions.Hidden);
        }

        internal static bool AcceptSubclass(this Port port) {
            return port.options.HasFlag(PortOptions.AcceptSubclass);
        }

        internal static bool IsMultiple(this Port port) {
            return !port.mode.HasFlag(PortMode.CapacitySingle) && !port.mode.HasFlag(PortMode.CapacityMultiple)
                ? !port.IsInput() || !port.IsData()
                : port.mode.HasFlag(PortMode.CapacityMultiple);
        }

        internal static bool IsLeftLayout(this Port port) {
            return !port.mode.HasFlag(PortMode.LayoutLeft) && !port.mode.HasFlag(PortMode.LayoutRight)
                ? port.mode.HasFlag(PortMode.Input)
                : port.mode.HasFlag(PortMode.LayoutLeft);
        }

        internal static Port External(this Port port, bool isExternal) {
            if (isExternal) port.options |= PortOptions.External;
            else port.options &= ~PortOptions.External;

            return port;
        }

        internal static int GetSignature(this Port port) {
            return HashCode.Combine(port.mode, port.Name, port.dataType);
        }
    }

}

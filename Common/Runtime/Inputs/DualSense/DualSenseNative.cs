using System.Runtime.InteropServices;

namespace MisterGames.Common.Inputs.DualSense
{
   
   internal static class DualSenseNative
   {
      [DllImport("DualSenseWindowsNative")]
      public static extern uint GetControllerCount();

      [DllImport("DualSenseWindowsNative")]
      public static extern ControllerInputState GetControllerInputState(uint controllerIndex);

      [DllImport("DualSenseWindowsNative")]
      public static extern bool SetControllerOutputState(uint controllerIndex, ControllerOutputState outputState);
   }
   
}
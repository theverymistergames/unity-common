using System.Runtime.InteropServices;

namespace MisterGames.Common.Inputs.DualSense
{
   
   internal static class DualSenseNative
   {
      private const string Dll = "DualSenseWindowsNative";
      
      /// <summary>
      /// Performs a SetupAPI device enumeration internally, takes milliseconds.
      /// Must never be called from the main thread.
      /// </summary>
      [DllImport(Dll, ExactSpelling = true)]
      public static extern uint GetControllerCount();

      [DllImport(Dll, ExactSpelling = true)]
      public static extern ControllerInputState GetControllerInputState(uint controllerIndex);

      /// <summary>
      /// Performs a blocking HID WriteFile internally: sub-millisecond over USB,
      /// up to several milliseconds over Bluetooth. Must never be called from the main thread.
      /// </summary>
      [DllImport(Dll, ExactSpelling = true)]
      [return: MarshalAs(UnmanagedType.I1)]
      public static extern bool SetControllerOutputState(uint controllerIndex, ControllerOutputState outputState);
   }
   
}

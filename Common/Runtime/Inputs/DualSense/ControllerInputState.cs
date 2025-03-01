using System;

namespace MisterGames.Common.Inputs.DualSense
{
   
   [Serializable]
   public struct ControllerInputState {
      
      public bool IsValid;

      public TriggerState LeftTrigger;
      public TriggerState RightTrigger;

      public ButtonState RightBumper;
      public ButtonState LeftBumper;

      public StickState LeftStick;
      public StickState RightStick;

      public ButtonState DPadUpButton;
      public ButtonState DPadDownButton;
      public ButtonState DPadLeftButton;
      public ButtonState DPadRightButton;

      public ButtonState TriangleButton;
      public ButtonState CircleButton;
      public ButtonState CrossButton;
      public ButtonState SquareButton;

      public ButtonState PSButton;
      public ButtonState CreateButton;
      public ButtonState OptionsButton;
      public ButtonState MicrophoneButton;

      public TouchPadState TouchPad;

      public VectorState Accelerometer;
      public VectorState Gyroscope;

      public bool HeadPhonesConnected;
   }
   
   public enum ButtonState {
      Up = 0,
      Down = 1,
      NewUp = 2,
      NewDown = 3,
   }

   [Serializable]
   public struct StickState {
      public double XAxis;
      public double YAxis;
      public ButtonState PushButton;
   }

   [Serializable]
   public struct TriggerState {
      public double TriggerValue;
      public ButtonState ActiveState;
   }

   [Serializable]
   public struct TouchPointState {
      public double X;
      public double Y;
   }

   [Serializable]
   public struct TouchPadState {
      public TouchPointState TouchPoint1;
      public TouchPointState TouchPoint2;
   }

   public struct VectorState {
      public double XAxis;
      public double YAxis;
      public double ZAxis;
   }
   
}
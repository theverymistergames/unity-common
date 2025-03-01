using System;
using System.Runtime.InteropServices;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;

namespace MisterGames.Common.Inputs.DualSense
{
   
   [Serializable]
   public struct ControllerOutputState {
      
      public TriggerEffect LeftTriggerEffect;
      public TriggerEffect RightTriggerEffect;
      
      public double LeftRumbleIntensity;
      public double RightRumbleIntensity;
      
      public bool LightBarEnabled;
      public double LightBarIntensity;
      public double LightBarR;
      public double LightBarG;
      public double LightBarB;
      
      [MarshalAs(UnmanagedType.I1)]
      public bool LeftPlayerLightEnabled;
      [MarshalAs(UnmanagedType.I1)]
      public bool MiddleLeftPlayerLightEnabled;
      [MarshalAs(UnmanagedType.I1)]
      public bool MiddlePlayerLightEnabled;
      [MarshalAs(UnmanagedType.I1)]
      public bool MiddleRightPlayerLightEnabled;
      [MarshalAs(UnmanagedType.I1)]
      public bool RightPlayerLightEnabled;
      [MarshalAs(UnmanagedType.I1)]
      public bool FadePlayerLight;
   }
   
   public enum TriggerEffectType {
      NoResistance = 0,
      ContinuousResistance = 1,
      SectionResistance = 2,
      EffectEx = 3,
   }

   [Serializable]
   public struct TriggerEffect {
      
      public TriggerEffectType EffectType;
      [VisibleIf(nameof(EffectType), value: 0, CompareMode.Greater)]
      public double StartPosition;
      [VisibleIf(nameof(EffectType), value: 2)]
      public double EndPosition;
      [VisibleIf(nameof(EffectType), value: 0, CompareMode.Greater)]
      public double BeginForce;
      [VisibleIf(nameof(EffectType), value: 3)]
      public double MiddleForce;
      [VisibleIf(nameof(EffectType), value: 3)]
      public double EndForce;
      [VisibleIf(nameof(EffectType), value: 3)]
      public double Frequency;
      [VisibleIf(nameof(EffectType), value: 3)]
      [MarshalAs(UnmanagedType.I1)] public bool KeepEffect;
      
      public void InitializeNoResistanceEffect() {
         EffectType = TriggerEffectType.NoResistance;
      }

      public void InitializeContinuousResistanceEffect(float startPosition, float force) {
         EffectType = TriggerEffectType.ContinuousResistance;
         StartPosition = startPosition;
         BeginForce = force;
      }

      public void InitializeSectionResistanceEffect(float startPosition, float endPosition, float force) {
         EffectType = TriggerEffectType.SectionResistance;
         StartPosition = startPosition;
         EndPosition = endPosition;
         BeginForce = force;
      }

      public void InitializeExtendedEffect(float startPosition, float beginForce, float middleForce, float endForce, float frequency, bool keepEffect) {
         EffectType = TriggerEffectType.EffectEx;
         StartPosition = startPosition;
         BeginForce = beginForce;
         MiddleForce = middleForce;
         EndForce = endForce;
         Frequency = frequency;
         KeepEffect = keepEffect;
      }
   }
   
}
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;

namespace MisterGames.Common.Inputs.DualSense
{
   
   [Serializable]
   [StructLayout(LayoutKind.Sequential)]
   public struct ControllerOutputState : IEquatable<ControllerOutputState> {
      
      public TriggerEffect LeftTriggerEffect;
      public TriggerEffect RightTriggerEffect;
      
      public double LeftRumbleIntensity;
      public double RightRumbleIntensity;
      
      [MarshalAs(UnmanagedType.I1)]
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

      public bool Equals(ControllerOutputState other) {
         return LeftRumbleIntensity.Equals(other.LeftRumbleIntensity) &&
                RightRumbleIntensity.Equals(other.RightRumbleIntensity) &&
                LightBarEnabled == other.LightBarEnabled &&
                LightBarIntensity.Equals(other.LightBarIntensity) &&
                LightBarR.Equals(other.LightBarR) &&
                LightBarG.Equals(other.LightBarG) &&
                LightBarB.Equals(other.LightBarB) &&
                LeftPlayerLightEnabled == other.LeftPlayerLightEnabled &&
                MiddleLeftPlayerLightEnabled == other.MiddleLeftPlayerLightEnabled &&
                MiddlePlayerLightEnabled == other.MiddlePlayerLightEnabled &&
                MiddleRightPlayerLightEnabled == other.MiddleRightPlayerLightEnabled &&
                RightPlayerLightEnabled == other.RightPlayerLightEnabled &&
                FadePlayerLight == other.FadePlayerLight &&
                LeftTriggerEffect.Equals(other.LeftTriggerEffect) &&
                RightTriggerEffect.Equals(other.RightTriggerEffect);
      }

      public override bool Equals(object obj) => obj is ControllerOutputState other && Equals(other);

      public override int GetHashCode() {
         var hash = new HashCode();
         hash.Add(LeftTriggerEffect);
         hash.Add(RightTriggerEffect);
         hash.Add(LeftRumbleIntensity);
         hash.Add(RightRumbleIntensity);
         hash.Add(LightBarEnabled);
         hash.Add(LightBarIntensity);
         hash.Add(LightBarR);
         hash.Add(LightBarG);
         hash.Add(LightBarB);
         return hash.ToHashCode();
      }

      public static bool operator ==(ControllerOutputState a, ControllerOutputState b) => a.Equals(b);
      public static bool operator !=(ControllerOutputState a, ControllerOutputState b) => !a.Equals(b);
   }
   
   public enum TriggerEffectType {
      NoResistance = 0,
      ContinuousResistance = 1,
      SectionResistance = 2,
      EffectEx = 3,
   }

   [Serializable]
   [StructLayout(LayoutKind.Sequential)]
   public struct TriggerEffect : IEquatable<TriggerEffect> {
      
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

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool Equals(TriggerEffect other) {
         return EffectType == other.EffectType &&
                StartPosition.Equals(other.StartPosition) &&
                EndPosition.Equals(other.EndPosition) &&
                BeginForce.Equals(other.BeginForce) &&
                MiddleForce.Equals(other.MiddleForce) &&
                EndForce.Equals(other.EndForce) &&
                Frequency.Equals(other.Frequency) &&
                KeepEffect == other.KeepEffect;
      }

      public override bool Equals(object obj) => obj is TriggerEffect other && Equals(other);

      public override int GetHashCode() {
         var hash = new HashCode();
         hash.Add((int) EffectType);
         hash.Add(StartPosition);
         hash.Add(EndPosition);
         hash.Add(BeginForce);
         hash.Add(MiddleForce);
         hash.Add(EndForce);
         hash.Add(Frequency);
         hash.Add(KeepEffect);
         return hash.ToHashCode();
      }

      public static bool operator ==(TriggerEffect a, TriggerEffect b) => a.Equals(b);
      public static bool operator !=(TriggerEffect a, TriggerEffect b) => !a.Equals(b);
   }
   
}

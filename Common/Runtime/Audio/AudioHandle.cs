using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public readonly struct AudioHandle : IEquatable<AudioHandle> {

        public static readonly AudioHandle Invalid = default;
        
        public float Volume {
            get => IsValid(out var e) ? e.Source.volume : 0f;
            set { if (IsValid(out var e)) e.Source.volume = value; }
        }

        public float PitchMul {
            get => IsValid(out var e) ? e.PitchMul : 0f;
            set { if (IsValid(out var e)) e.PitchMul = value; }
        }
        
        public float StereoPan {
            get => IsValid(out var e) ? e.Source.panStereo : 0f;
            set { if (IsValid(out var e)) e.Source.panStereo = value; }
        }
        
        public float AttenuationMul {
            get => IsValid(out var e) ? e.AttenuationMul : 0f;
            set { if (IsValid(out var e)) e.AttenuationMul = value; }
        }

        public Vector3 Position {
            get => IsValid(out var e) ? e.Transform.position : default;
            set { if (IsValid(out var e)) e.Transform.position = value; }
        }

        public Vector3 LocalPosition {
            get => IsValid(out var e) ? e.Transform.localPosition : default;
            set { if (IsValid(out var e)) e.Transform.localPosition = value; }
        }

        private readonly IAudioPool _pool;
        private readonly int _id;

        public AudioHandle(IAudioPool pool, int id) {
            _pool = pool;
            _id = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Play() {
            if (IsValid(out var e)) e.Source.Play();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pause() {
            if (IsValid(out var e)) e.Source.Pause();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop() {
            if (IsValid(out var e)) e.Source.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release() {
            _pool?.ReleaseAudioHandle(_id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid() {
            return _pool?.TryGetAudioElement(_id, out _) ?? false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid(out IAudioElement audioElement) {
            audioElement = null;
            return _pool?.TryGetAudioElement(_id, out audioElement) ?? false;
        }
        
        public bool Equals(AudioHandle other) => _id == other._id;
        public override bool Equals(object obj) => obj is AudioHandle other && Equals(other);
        public override int GetHashCode() => _id;

        public static bool operator ==(AudioHandle left, AudioHandle right) => left.Equals(right);
        public static bool operator !=(AudioHandle left, AudioHandle right) => !left.Equals(right);

        public override string ToString() {
            return $"AudioHandle({(IsValid() ? _id.ToString() : "invalid")})";
        }
    }
    
}
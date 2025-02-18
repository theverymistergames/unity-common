using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public readonly struct AudioHandle : IEquatable<AudioHandle> {

        public static readonly AudioHandle Invalid = default;
        
        public float Volume {
            get => IsValid(out var source) ? source.volume : 0f;
            set { if (IsValid(out var source)) source.volume = value; }
        }

        public float Pitch {
            get => IsValid(out var source) ? source.pitch : 0f;
            set => _pool?.SetAudioHandlePitch(_id, value);
        }
        
        public float StereoPan {
            get => IsValid(out var source) ? source.panStereo : 0f;
            set { if (IsValid(out var source)) source.panStereo = value; }
        }
        
        public Vector3 Position {
            get => IsValid(out var source) ? source.transform.position : default;
            set { if (IsValid(out var source)) source.transform.position = value; }
        }

        public Vector3 LocalPosition {
            get => IsValid(out var source) ? source.transform.localPosition : default;
            set { if (IsValid(out var source)) source.transform.localPosition = value; }
        }

        private readonly IAudioPool _pool;
        private readonly int _id;

        public AudioHandle(IAudioPool pool, int id) {
            _pool = pool;
            _id = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Play() {
            if (IsValid(out var source)) source.Play();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pause() {
            if (IsValid(out var source)) source.Pause();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop() {
            if (IsValid(out var source)) source.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release() {
            _pool?.ReleaseAudioHandle(_id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid() {
            return _pool?.TryGetAudioSource(_id, out _) ?? false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid(out AudioSource source) {
            source = null;
            return _pool?.TryGetAudioSource(_id, out source) ?? false;
        }
        
        public bool Equals(AudioHandle other) => _id == other._id;
        public override bool Equals(object obj) => obj is AudioHandle other && Equals(other);
        public override int GetHashCode() => _id;

        public static bool operator ==(AudioHandle left, AudioHandle right) => left.Equals(right);
        public static bool operator !=(AudioHandle left, AudioHandle right) => !left.Equals(right);
    }
    
}
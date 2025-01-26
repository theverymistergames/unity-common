using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public readonly struct AudioHandle : IEquatable<AudioHandle> {

        public static readonly AudioHandle Invalid = default;
        
        public float Volume {
            get => IsValid() ? _audioSource.volume : 0f;
            set { if (IsValid()) _audioSource.volume = value; }
        }

        public float Pitch {
            get => IsValid() ? _audioSource.pitch : 0f;
            set { if (IsValid()) _audioSource.pitch = value; }
        }
        
        public float StereoPan {
            get => IsValid() ? _audioSource.panStereo : 0f;
            set { if (IsValid()) _audioSource.panStereo = value; }
        }
        
        public Vector3 Position {
            get => IsValid() ? _transform.position : Vector3.zero;
            set { if (IsValid()) _transform.position = value; }
        }

        public Vector3 LocalPosition {
            get => IsValid() ? _transform.localPosition : Vector3.zero;
            set { if (IsValid()) _transform.localPosition = value; }
        }

        private readonly int _id;
        private readonly AudioSource _audioSource;
        private readonly Transform _transform;
        private readonly IAudioPool _pool;

        public AudioHandle(int id, AudioSource audioSource, IAudioPool pool) {
            _id = id;
            _audioSource = audioSource;
            _transform = audioSource.transform;
            _pool = pool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Play() {
            if (IsValid()) _audioSource.Play();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pause() {
            if (IsValid()) _audioSource.Pause();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop() {
            if (IsValid()) _audioSource.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release() {
            _pool?.ReleaseAudioHandle(_id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid() {
            return _pool?.IsValidAudioHandle(_id) ?? false;
        }
        
        public bool Equals(AudioHandle other) => _id == other._id;
        public override bool Equals(object obj) => obj is AudioHandle other && Equals(other);
        public override int GetHashCode() => _id;

        public static bool operator ==(AudioHandle left, AudioHandle right) => left.Equals(right);
        public static bool operator !=(AudioHandle left, AudioHandle right) => !left.Equals(right);
    }
    
}
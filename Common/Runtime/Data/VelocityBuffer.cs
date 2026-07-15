using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Data {

    public sealed class VelocityBuffer {
        
        private (Vector3 diff, float dt)[] _buffer;
        private int _bufferSize;
        private int _bufferPointer;

        public VelocityBuffer(int bufferSize = 4) {
            _bufferSize = bufferSize;
            _buffer = new (Vector3, float)[_bufferSize];
        }

        public void SetBufferSize(int bufferSize) {
            _bufferSize = bufferSize;
            if (_buffer == null || _buffer.Length != _bufferSize) _buffer = new (Vector3, float)[_bufferSize];
        }

        public void WriteIntoBuffer(Vector3 diff, float dt) {
            int i = _bufferPointer.ReturnThenIncrementUncheckedRef();
            _buffer[i % _bufferSize] = (diff, dt);
        }

        public void ClearBuffer() {
            _bufferPointer = 0;
        }

        public Vector3 GetVelocity() {
            int count = Mathf.Min(_bufferPointer, _bufferSize);
            var diff = Vector3.zero;
            float t = 0f;

            for (int i = 0; i < count; i++) {
                (var d, float dt) = _buffer[i];
                diff += d;
                t += dt;
            }

            return t > 0f ? diff * (1f / t) : default;
        }
    }
    
}
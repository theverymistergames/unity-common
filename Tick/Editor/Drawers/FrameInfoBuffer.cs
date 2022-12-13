namespace MisterGames.Tick.Editor.Drawers {

    public class AverageBuffer {

        public float Result => _averageValue;

        private readonly float[] _buffer;
        private int _pointer;

        private float _averageValue;

        public AverageBuffer(int size) {
            _buffer = new float[size];
        }

        public void AddValue(float value) {
            int bufferSize = _buffer.Length;

            for (int i = 0; i < bufferSize; i++) {
                _buffer[i] = i < bufferSize - 1 ? _buffer[i + 1] : value;
            }

            if (_pointer++ > bufferSize - 1) {
                _pointer = 0;

                float sum = 0f;
                for (int i = 0; i < bufferSize; i++) {
                    sum += _buffer[i];
                }

                _averageValue = bufferSize > 0 ? sum / bufferSize : 0f;
            }
        }
    }

}

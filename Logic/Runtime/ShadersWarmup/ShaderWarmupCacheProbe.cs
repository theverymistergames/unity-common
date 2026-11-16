using System;
using System.Globalization;
using UnityEngine;

namespace MisterGames.Logic.ShadersWarmup {

    internal sealed class ShaderWarmupCacheProbe {

        private const string FingerprintPrefsKey = "ShaderWarmup.WarmedUpFingerprint";
        private const string ColdStageSecondsPrefsKey = "ShaderWarmup.ColdStageSeconds";
        private const string WarmStageSecondsPrefsKey = "ShaderWarmup.WarmStageSeconds";

        private const float MaxWarmProbeToFullRunRatio = 4f;

        private readonly string _fingerprint;
        private readonly float _minColdToWarmRatio;
        private readonly float _warmStageThresholdSeconds;

        private readonly float _storedColdStageSeconds;
        private readonly float _storedWarmStageSeconds;

        private float _measuredStageSeconds = -1f;
        private bool _isWarm;
        private string _decidedBy = "-";
        private float _fullRunStageSeconds = -1f;

        public bool HasMachineData { get; }
        public string CommitNote { get; private set; } = "not committed";

        public ShaderWarmupCacheProbe(string fingerprint, float minColdToWarmRatio, float warmStageThresholdSeconds) {
            _fingerprint = fingerprint;
            _minColdToWarmRatio = Mathf.Max(1f, minColdToWarmRatio);
            _warmStageThresholdSeconds = warmStageThresholdSeconds;

            bool sameFingerprint = string.Equals(
                PlayerPrefs.GetString(FingerprintPrefsKey, string.Empty),
                fingerprint,
                StringComparison.Ordinal);

            _storedColdStageSeconds = sameFingerprint ? ReadSeconds(ColdStageSecondsPrefsKey) : -1f;
            _storedWarmStageSeconds = sameFingerprint ? ReadSeconds(WarmStageSecondsPrefsKey) : -1f;

            HasMachineData = _storedColdStageSeconds > 0f || _storedWarmStageSeconds > 0f;
        }

        public float MachineThresholdStageSeconds {
            get {
                bool hasCold = _storedColdStageSeconds > 0f;
                bool hasWarm = _storedWarmStageSeconds > 0f;

                if (hasCold && hasWarm) {
                    return Mathf.Min(
                        _storedColdStageSeconds / _minColdToWarmRatio,
                        Mathf.Sqrt(_storedColdStageSeconds * _storedWarmStageSeconds));
                }

                if (hasCold) return _storedColdStageSeconds / _minColdToWarmRatio;
                if (hasWarm) return _storedWarmStageSeconds * _minColdToWarmRatio;

                return -1f;
            }
        }

        public bool Decide(float stageSeconds) {
            _measuredStageSeconds = Mathf.Max(0f, stageSeconds);

            if (_measuredStageSeconds < _warmStageThresholdSeconds) {
                _isWarm = true;
                _decidedBy = "absolute threshold";
            }
            else if (HasMachineData && _measuredStageSeconds < MachineThresholdStageSeconds) {
                _isWarm = true;
                _decidedBy = "machine threshold";
            }
            else {
                _isWarm = false;
                _decidedBy = HasMachineData ? "above both thresholds" : "above absolute threshold";
            }

            return _isWarm;
        }

        public void SetFullRun(float stageSeconds) {
            _fullRunStageSeconds = stageSeconds;
        }

        public void Commit() {
            float cold = _storedColdStageSeconds;
            float warm = _storedWarmStageSeconds;

            if (_measuredStageSeconds < 0f) {
                CommitNote = "no probe, nothing written";
            }
            else if (_isWarm) {
                warm = warm > 0f ? Mathf.Min(warm, _measuredStageSeconds) : _measuredStageSeconds;
                CommitNote = $"warm by probe, warm {FormatMs(warm)}";
            }
            else if (_fullRunStageSeconds >= _warmStageThresholdSeconds) {
                cold = Mathf.Max(cold, _measuredStageSeconds);
                CommitNote = $"cold confirmed by full run {FormatMs(_fullRunStageSeconds)} per stage, cold {FormatMs(cold)}";
            }
            else if (_fullRunStageSeconds >= 0f && _measuredStageSeconds <= _fullRunStageSeconds * MaxWarmProbeToFullRunRatio) {
                warm = warm > 0f ? Mathf.Min(warm, _measuredStageSeconds) : _measuredStageSeconds;
                CommitNote = $"cold by probe, but full run {FormatMs(_fullRunStageSeconds)} per stage is warm: " +
                             $"probe written as warm {FormatMs(warm)}";
            }
            else if (_fullRunStageSeconds >= 0f) {
                CommitNote = $"cold by probe, full run {FormatMs(_fullRunStageSeconds)} per stage is warm, but probe is " +
                             "much costlier (probe hit changed shaders?), nothing written";
            }
            else {
                CommitNote = "cold by probe, full run not measured, nothing written";
            }

            CommitNote += $"; stored cold {FormatMs(cold)}, warm {FormatMs(warm)}";

            PlayerPrefs.SetString(FingerprintPrefsKey, _fingerprint);
            WriteSeconds(ColdStageSecondsPrefsKey, cold);
            WriteSeconds(WarmStageSecondsPrefsKey, warm);
            PlayerPrefs.Save();
        }

        public override string ToString() {
            return $"stage {FormatMs(_measuredStageSeconds)}, warm cache {_isWarm} by {_decidedBy}; " +
                   $"absolute threshold {FormatMs(_warmStageThresholdSeconds)}; " +
                   $"machine cold {FormatMs(_storedColdStageSeconds)}, warm {FormatMs(_storedWarmStageSeconds)}, " +
                   $"threshold {(HasMachineData ? FormatMs(MachineThresholdStageSeconds) : "-")}";
        }

        private static string FormatMs(float seconds) {
            return seconds >= 0f ? $"{seconds * 1000f:0.00} ms" : "-";
        }

        private static float ReadSeconds(string key) {
            string value = PlayerPrefs.GetString(key, string.Empty);

            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float seconds)
                ? seconds
                : -1f;
        }

        private static void WriteSeconds(string key, float seconds) {
            if (seconds > 0f) PlayerPrefs.SetString(key, seconds.ToString("R", CultureInfo.InvariantCulture));
            else PlayerPrefs.DeleteKey(key);
        }
    }

}

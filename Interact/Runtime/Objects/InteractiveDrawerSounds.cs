using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Interact.Objects {
    
    [RequireComponent(typeof(InteractiveDrawer))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class InteractiveDrawerSounds : MonoBehaviour {
        
        [SerializeField] private InteractiveDrawerConfig _config;
        
        private AudioSource _source;
        private InteractiveDrawer _drawer;

        private float _currentProcess;
        private float _prevProcess;
        
        private float _lastOpenCloseTime;
        private float _lastSlideTime;
        private float _lastClickTime;
        
        private float _slideProcessDelta;
        private float _clickProcessDelta;
        private float _clickProcessSection;

        private void Awake() {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 1f;
            
            _drawer = GetComponent<InteractiveDrawer>();
            _clickProcessSection = 1f / _config.clickersAmount;
        }

        private void OnEnable() {
            _drawer.OnMove += OnMove;
        }

        private void OnDisable() {
            _drawer.OnMove -= OnMove;
        }

        private void OnMove(float process, float speedFactor) {
            if (process.IsNearlyEqual(_currentProcess)) return;
            
            _prevProcess = _currentProcess;
            _currentProcess = process;
            
            float volume = GetVolume(speedFactor);

            if (_config.enableClickSounds) CheckClicker(volume);
            if (_config.enableSlideSounds) CheckSlides(volume);
            if (_config.enableOpenCloseSounds) CheckRebound(volume);
        }

        private void CheckSlides(float volume) {
            float timeSinceStartup = Time.realtimeSinceStartup;
            float timeSinceLastSlide = timeSinceStartup - _lastSlideTime;

            float minTime = Random.Range(_config.minDelayBetweenSlideSounds, _config.maxDelayBetweenSlideSounds);
            if (timeSinceLastSlide < minTime) return;
            
            _slideProcessDelta += Mathf.Abs(_currentProcess - _prevProcess);
            if (_slideProcessDelta < _config.minSlideProcessDelta) return;

            _lastSlideTime = timeSinceStartup;
            
            _source.PlayOneShot(_config.slideSounds.GetRandom(), volume * _config.slideSoundsVolumeMultiplier);
        }

        private void CheckRebound(float volume) {
            float timeSinceStartup = Time.realtimeSinceStartup;
            float timeSinceLastRebound = timeSinceStartup - _lastOpenCloseTime;

            if (timeSinceLastRebound < _config.minOpenCloseDelayBetweenSounds) return;
            if (0f < _currentProcess && _currentProcess < 1f && _prevProcess > 0f) return;
            
            _lastOpenCloseTime = timeSinceStartup;

            var clip = _currentProcess <= 0
                ? _config.closeSounds.GetRandom() 
                : _currentProcess < 1f && _prevProcess <= 0f 
                    ? _config.openSounds.GetRandom() 
                    : _config.finishOpenSounds.GetRandom();
            
            _source.PlayOneShot(clip, volume * _config.openCloseSoundsVolumeMultiplier);
        }

        private void CheckClicker(float volume) {
            _clickProcessDelta += Mathf.Abs(_currentProcess - _prevProcess);
            if (_clickProcessDelta < _clickProcessSection) return;

            float timeSinceStartup = Time.realtimeSinceStartup;
            float timeSinceLastRebound = timeSinceStartup - _lastClickTime;

            if (timeSinceLastRebound < _config.minDelayBetweenClickSounds) return;

            _lastClickTime = timeSinceStartup;
            _clickProcessDelta = 0f;
            
            _source.PlayOneShot(_config.clickSounds.GetRandom(), volume * _config.clickSoundsVolumeMultiplier);
        }

        private float GetVolume(float speedFactor) {
            return _config.volumeBySpeed.Evaluate(speedFactor) * _config.volumeBySpeedMultiplier;
        }
        
    }
    
}
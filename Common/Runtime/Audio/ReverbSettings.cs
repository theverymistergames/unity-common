using System;
using UnityEngine;

namespace MisterGames.Common.Audio {
 
    [CreateAssetMenu(fileName = nameof(ReverbSettings), menuName = "MisterGames/Audio/" + nameof(ReverbSettings))]
    public sealed class ReverbSettings : ScriptableObject, IReverbSettings {

        [SerializeField] private AudioReverbPreset _preset = AudioReverbPreset.User;
        [SerializeField] [Range(-10000f, 0f)] private float _roomHf;
        [SerializeField] [Range(-10000f, 0f)] private float _roomLf;
        [SerializeField] [Range(0.1f, 20f)] private float _decayTime;
        [SerializeField] [Range(0.1f, 2f)] private float _decayHfRatio;
        [SerializeField] [Range(-10000f, 1000f)] private float _reflectionsLevel;
        [SerializeField] [Range(0f, 0.3f)] private float _reflectionsDelay;
        [SerializeField] [Range(-10000f, 2000f)] private float _reverbLevel;
        [SerializeField] [Range(0f, 0.1f)] private float _reverbDelay;
        [SerializeField] [Range(1000f, 20000f)] private float _hfReference;
        [SerializeField] [Range(20f, 1000f)] private float _lfReference;
        [SerializeField] [Range(0f, 100f)] private float _diffusion;
        [SerializeField] [Range(0f, 100f)] private float _density;

        public float RoomHf => _roomHf;
        public float RoomLf => _roomLf;
        public float DecayTime => _decayTime;
        public float DecayHfRatio => _decayHfRatio;
        public float ReflectionsLevel => _reflectionsLevel;
        public float ReflectionsDelay => _reflectionsDelay;
        public float ReverbLevel => _reverbLevel;
        public float ReverbDelay => _reverbDelay;
        public float HfReference => _hfReference;
        public float LfReference => _lfReference;
        public float Diffusion => _diffusion;
        public float Density => _density;

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private AudioReverbPreset _lastPreset = AudioReverbPreset.User;

        private void OnValidate() {
            if (_preset == _lastPreset) return;

            _lastPreset = _preset;
            ApplyPreset(_preset);
        }  
#endif

        private void ApplyPreset(AudioReverbPreset preset) {
            switch (preset) {
                case AudioReverbPreset.Generic: SetGeneric(); break;
                case AudioReverbPreset.PaddedCell: SetPaddedCell(); break;
                case AudioReverbPreset.Room: SetRoom(); break;
                case AudioReverbPreset.Bathroom: SetBathroom(); break;
                case AudioReverbPreset.Livingroom: SetLivingroom(); break;
                case AudioReverbPreset.Stoneroom: SetStoneroom(); break;
                case AudioReverbPreset.Auditorium: SetAuditorium(); break;
                case AudioReverbPreset.Concerthall: SetConcerthall(); break;
                case AudioReverbPreset.Cave: SetCave(); break;
                case AudioReverbPreset.Arena: SetArena(); break;
                case AudioReverbPreset.Hangar: SetHangar(); break;
                case AudioReverbPreset.CarpetedHallway: SetCarpettedHallway(); break;
                case AudioReverbPreset.Hallway: SetHallway(); break;
                case AudioReverbPreset.StoneCorridor: SetStoneCorridor(); break;
                case AudioReverbPreset.Alley: SetAlley(); break;
                case AudioReverbPreset.Forest: SetForest(); break;
                case AudioReverbPreset.City: SetCity(); break;
                case AudioReverbPreset.Mountains: SetMountains(); break;
                case AudioReverbPreset.Quarry: SetQuarry(); break;
                case AudioReverbPreset.Plain: SetPlain(); break;
                case AudioReverbPreset.ParkingLot: SetParkingLot(); break;
                case AudioReverbPreset.SewerPipe: SetSewerPipe(); break;
                case AudioReverbPreset.Underwater: SetUnderwater(); break;
                case AudioReverbPreset.Drugged: SetDrugged(); break;
                case AudioReverbPreset.Dizzy: SetDizzy(); break;
                case AudioReverbPreset.Psychotic: SetPsychotic(); break;
                case AudioReverbPreset.Off: SetOff(); break;
                case AudioReverbPreset.User: break;
                default: throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }

        private void SetGeneric() {
            _roomHf = -100f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.83f;
            _reflectionsLevel = -2602f;
            _reflectionsDelay = 0f;
            _reverbLevel = 200f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetPaddedCell() {
            _roomHf = -6000f;
            _roomLf = 0f;
            _decayTime = 0.17f;
            _decayHfRatio = 0.1f;
            _reflectionsLevel = -1204f;
            _reflectionsDelay = 0f;
            _reverbLevel = 207f;
            _reverbDelay = 0.002f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetRoom() {
            _roomHf = -454f;
            _roomLf = 0f;
            _decayTime = 0.4f;
            _decayHfRatio = 0.83f;
            _reflectionsLevel = -1646f;
            _reflectionsDelay = 0f;
            _reverbLevel = 53f;
            _reverbDelay = 0.003f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetBathroom() {
            _roomHf = -1200f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.54f;
            _reflectionsLevel = -370f;
            _reflectionsDelay = 0f;
            _reverbLevel = 1030f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 60f;
        }

        private void SetLivingroom() {
            _roomHf = -6000f;
            _roomLf = 0f;
            _decayTime = 0.5f;
            _decayHfRatio = 0.1f;
            _reflectionsLevel = -1376f;
            _reflectionsDelay = 0f;
            _reverbLevel = -1104f;
            _reverbDelay = 0.004f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetStoneroom() {
            _roomHf = -300f;
            _roomLf = 0f;
            _decayTime = 2.31f;
            _decayHfRatio = 0.64f;
            _reflectionsLevel = -711f;
            _reflectionsDelay = 0f;
            _reverbLevel = 83f;
            _reverbDelay = 0.017f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetAuditorium() {
            _roomHf = -476f;
            _roomLf = 0f;
            _decayTime = 4.32f;
            _decayHfRatio = 0.59f;
            _reflectionsLevel = -789f;
            _reflectionsDelay = 0f;
            _reverbLevel = -289f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetConcerthall() {
            _roomHf = -500f;
            _roomLf = 0f;
            _decayTime = 3.92f;
            _decayHfRatio = 0.7f;
            _reflectionsLevel = -1230f;
            _reflectionsDelay = 0f;
            _reverbLevel = -2f;
            _reverbDelay = 0.029f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetCave() {
            _roomHf = 0f;
            _roomLf = 0f;
            _decayTime = 2.91f;
            _decayHfRatio = 1.3f;
            _reflectionsLevel = -602f;
            _reflectionsDelay = 0f;
            _reverbLevel = -302f;
            _reverbDelay = 0.022f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetArena() {
            _roomHf = -698f;
            _roomLf = 0f;
            _decayTime = 7.24f;
            _decayHfRatio = 0.33f;
            _reflectionsLevel = -1166f;
            _reflectionsDelay = 0f;
            _reverbLevel = 16f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetHangar() {
            _roomHf = -1000f;
            _roomLf = 0f;
            _decayTime = 10.05f;
            _decayHfRatio = 0.23f;
            _reflectionsLevel = -602f;
            _reflectionsDelay = 0f;
            _reverbLevel = 198f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetCarpettedHallway() {
            _roomHf = -4000f;
            _roomLf = 0f;
            _decayTime = 0.3f;
            _decayHfRatio = 0.1f;
            _reflectionsLevel = -1831f;
            _reflectionsDelay = 0f;
            _reverbLevel = -1630f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetHallway() {
            _roomHf = -300f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.59f;
            _reflectionsLevel = -1219f;
            _reflectionsDelay = 0f;
            _reverbLevel = 441f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetStoneCorridor() {
            _roomHf = -237f;
            _roomLf = 0f;
            _decayTime = 2.7f;
            _decayHfRatio = 0.79f;
            _reflectionsLevel = -1214f;
            _reflectionsDelay = 0f;
            _reverbLevel = 395f;
            _reverbDelay = 0.02f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetAlley() {
            _roomHf = -270f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.86f;
            _reflectionsLevel = -1204f;
            _reflectionsDelay = 0f;
            _reverbLevel = -4f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetForest() {
            _roomHf = -3300f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.54f;
            _reflectionsLevel = -2560f;
            _reflectionsDelay = 0f;
            _reverbLevel = -229f;
            _reverbDelay = 0.088f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 79f;
            _density = 100f;
        }

        private void SetCity() {
            _roomHf = -800f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.67f;
            _reflectionsLevel = -2273f;
            _reflectionsDelay = 0f;
            _reverbLevel = -1691f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 50f;
            _density = 100f;
        }

        private void SetMountains() {
            _roomHf = -2500f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.21f;
            _reflectionsLevel = -2780f;
            _reflectionsDelay = 0f;
            _reverbLevel = -1434f;
            _reverbDelay = 0.1f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 27f;
            _density = 100f;
        }

        private void SetQuarry() {
            _roomHf = -1000f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.83f;
            _reflectionsLevel = -10000f;
            _reflectionsDelay = 0f;
            _reverbLevel = 500f;
            _reverbDelay = 0.025f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetPlain() {
            _roomHf = -2000f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.5f;
            _reflectionsLevel = -2466f;
            _reflectionsDelay = 0f;
            _reverbLevel = -1926f;
            _reverbDelay = 0.1f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 21f;
            _density = 100f;
        }

        private void SetParkingLot() {
            _roomHf = 0f;
            _roomLf = 0f;
            _decayTime = 1.65f;
            _decayHfRatio = 1.5f;
            _reflectionsLevel = -1363f;
            _reflectionsDelay = 0f;
            _reverbLevel = -1153f;
            _reverbDelay = 0.012f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetSewerPipe() {
            _roomHf = -1000f;
            _roomLf = 0f;
            _decayTime = 2.81f;
            _decayHfRatio = 0.14f;
            _reflectionsLevel = 429f;
            _reflectionsDelay = 0f;
            _reverbLevel = 1023f;
            _reverbDelay = 0.021f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 80f;
            _density = 60f;
        }

        private void SetUnderwater() {
            _roomHf = -4000f;
            _roomLf = 0f;
            _decayTime = 1.49f;
            _decayHfRatio = 0.1f;
            _reflectionsLevel = -449f;
            _reflectionsDelay = 0f;
            _reverbLevel = 1700f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetDrugged() {
            _roomHf = 0f;
            _roomLf = 0f;
            _decayTime = 8.39f;
            _decayHfRatio = 1.39f;
            _reflectionsLevel = -115f;
            _reflectionsDelay = 0f;
            _reverbLevel = 985f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetDizzy() {
            _roomHf = -400f;
            _roomLf = 0f;
            _decayTime = 17.23f;
            _decayHfRatio = 0.56f;
            _reflectionsLevel = -1713f;
            _reflectionsDelay = 0f;
            _reverbLevel = -613f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetPsychotic() {
            _roomHf = -151f;
            _roomLf = 0f;
            _decayTime = 7.56f;
            _decayHfRatio = 0.91f;
            _reflectionsLevel = -626f;
            _reflectionsDelay = 0f;
            _reverbLevel = 774f;
            _reverbDelay = 0.03f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 100f;
            _density = 100f;
        }

        private void SetOff() {
            _roomHf = -10000f;
            _roomLf = 0f;
            _decayTime = 1f;
            _decayHfRatio = 1f;
            _reflectionsLevel = -2602f;
            _reflectionsDelay = 0f;
            _reverbLevel = 200f;
            _reverbDelay = 0.011f;
            _hfReference = 5000f;
            _lfReference = 250f;
            _diffusion = 0f;
            _density = 0f;
        }
    }
    
}
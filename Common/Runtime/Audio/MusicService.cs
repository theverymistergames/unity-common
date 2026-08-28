using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Strings;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {

    public sealed class MusicService : IMusicService, IUpdate, IDisposable {

        private static readonly string LogPrefix = nameof(MusicService).FormatColorOnlyForEditor(Color.white);

        private readonly Dictionary<EntityId, MusicGroup> _mixerGroupToMusicGroupMap = new();
        // Settings only live between the start call and the track creation inside it, so a single instance
        // is refilled on each start instead of being allocated or copied around.
        private readonly MusicSettings _musicSettings = new();
        private IAudioPool _audioPool;
        private float _fadeIn;
        private float _fadeOut;

        private sealed class MusicGroup {
            // Tracks of the mixer group, the last one is the current music, the rest of them are fading out.
            public readonly List<Music> queue = new();
            // No track of the queue is current: the whole queue is fading out, to be stopped or replaced.
            public bool fadeOutAll;
            public bool hasPendingMusic;
            // Track that is started and paused until the queue above has faded out completely.
            // It is kept out of the queue, so that it is not faded in while the queue is still audible.
            public Music pendingMusic;
        }

        private struct Music {
            public AudioHandle handle;
            // Volume of the track when it is faded in completely.
            public float volume;
            // Fade progress: 0 is silence, 1 is the volume above.
            public float weight;
            public float fadeIn;
            public float fadeOut;
        }

        private sealed class MusicSettings {
            public AudioClip clip;
            public float volume;
            public float pitch;
            public float startTime;
            public bool loop;
            public bool affectedByTimescale;
            public AudioMixerGroup mixerGroup;
            public float fadeIn;
            public float fadeOut;
            public CancellationToken cancellationToken;
        }

        public void Initialize(MusicServiceConfig config, IAudioPool audioPool) {
            if (config == null || audioPool == null) {
                Debug.LogError($"{LogPrefix}: f {Time.frameCount}, can not initialize with " +
                               $"config [{config}] and audio pool [{audioPool}], music is not managed.");
                return;
            }

            _fadeIn = config.fadeIn;
            _fadeOut = config.fadeOut;
            _audioPool = audioPool;

            PlayerLoopStage.UnscaledUpdate.Subscribe(this);
        }

        public void Dispose() {
            PlayerLoopStage.UnscaledUpdate.Unsubscribe(this);

            StopAllMusic(immediate: true);

            _mixerGroupToMusicGroupMap.Clear();
            _audioPool = null;
        }

        public AudioHandle StartMusic(
            AudioClip clip,
            float volume,
            float pitch,
            float startTime,
            bool loop,
            bool affectedByTimescale,
            AudioMixerGroup mixerGroup,
            float fadeIn = -1f,
            float fadeOut = -1f,
            bool waitForPreviousFadeOut = false,
            CancellationToken cancellationToken = default)
        {
            if (clip == null || _audioPool == null) {
                Debug.LogError($"{LogPrefix}: f {Time.frameCount}, can not start music " +
                               $"clip [{clip}] with audio pool [{_audioPool}].");
                return AudioHandle.Invalid;
            }

            _musicSettings.clip = clip;
            _musicSettings.volume = volume;
            _musicSettings.pitch = pitch;
            _musicSettings.startTime = startTime;
            _musicSettings.loop = loop;
            _musicSettings.affectedByTimescale = affectedByTimescale;
            _musicSettings.mixerGroup = mixerGroup;
            _musicSettings.fadeIn = fadeIn < 0f ? _fadeIn : fadeIn;
            _musicSettings.fadeOut = fadeOut < 0f ? _fadeOut : fadeOut;
            _musicSettings.cancellationToken = cancellationToken;

            var musicGroup = GetOrCreateMusicGroup(mixerGroup);

            // Music is already playing on the mixer group and must become silent before the new track is audible.
            bool wait = waitForPreviousFadeOut && musicGroup.queue.Count > 0;

            // A track that was waiting for its turn never became audible, so it is dropped without a fade out.
            ReleasePendingMusic(musicGroup);

            // Track is started in any case, so that a valid handle can be returned right away.
            var music = StartTrack(_musicSettings, paused: wait);

            if (wait) {
                musicGroup.fadeOutAll = true;
                musicGroup.hasPendingMusic = true;
                musicGroup.pendingMusic = music;
            }
            else {
                musicGroup.fadeOutAll = false;
                musicGroup.queue.Add(music);
            }

            return music.handle;
        }

        public AudioHandle GetCurrentMusic(AudioMixerGroup mixerGroup) {
            if (!TryGetMusicGroup(mixerGroup, out var musicGroup)) return AudioHandle.Invalid;

            // A pending track is the music of the group already: it is started, and it is the one to become audible.
            if (musicGroup.hasPendingMusic) return musicGroup.pendingMusic.handle;

            return !musicGroup.fadeOutAll && musicGroup.queue.Count > 0
                ? musicGroup.queue[^1].handle
                : AudioHandle.Invalid;
        }

        public void StopMusic(AudioMixerGroup mixerGroup, bool immediate = false) {
            if (!TryGetMusicGroup(mixerGroup, out var musicGroup)) return;

            StopMusicGroup(musicGroup, immediate);
        }

        public void StopAllMusic(bool immediate = false) {
            foreach (var musicGroup in _mixerGroupToMusicGroupMap.Values) {
                StopMusicGroup(musicGroup, immediate);
            }
        }

        void IUpdate.OnUpdate(float dt) {
            foreach (var musicGroup in _mixerGroupToMusicGroupMap.Values) {
                UpdateMusicGroup(musicGroup, dt);
            }
        }

        private void UpdateMusicGroup(MusicGroup musicGroup, float dt) {
            var queue = musicGroup.queue;

            // Tracks released from the outside are dropped first, so that the last one left is the current music.
            for (int i = queue.Count - 1; i >= 0; i--) {
                if (!queue[i].handle.IsValid()) queue.RemoveAt(i);
            }

            // Nothing in the queue is current while the whole queue is being faded out.
            int current = musicGroup.fadeOutAll ? -1 : queue.Count - 1;

            for (int i = queue.Count - 1; i >= 0; i--) {
                var music = queue[i];
                bool isCurrent = i == current;

                float fade = isCurrent ? music.fadeIn : music.fadeOut;
                float step = fade > 0f ? dt / fade : 1f;

                music.weight = Mathf.MoveTowards(music.weight, isCurrent ? 1f : 0f, step);
                music.handle.Volume = music.weight * music.volume;

                // Track is silent and is not the current music anymore: it is not needed.
                if (!isCurrent && music.weight <= 0f) {
                    music.handle.Release(immediate: true);
                    queue.RemoveAt(i);
                    continue;
                }

                queue[i] = music;
            }

            if (!musicGroup.hasPendingMusic || queue.Count > 0) return;

            ResumePendingMusic(musicGroup);
        }

        private Music StartTrack(MusicSettings settings, bool paused) {
            var options = AudioOptions.None;
            options |= settings.loop ? AudioOptions.Loop : AudioOptions.None;
            options |= settings.affectedByTimescale ? AudioOptions.AffectedByTimeScale : AudioOptions.None;

            // Music is 2d, so the position only matters for the distance based parameters, which it has none of.
            var position = _audioPool.TryGetListenerPosition(out var listenerPosition) ? listenerPosition : default;

            float weight = settings.fadeIn > 0f ? 0f : 1f;

            // Pool fades are zero on purpose: the volume of the track is written by the service.
            // A paused track is started silent as well, to make no sound in between the start and the pause.
            var handle = _audioPool.Play(
                settings.clip,
                position,
                volume: paused ? 0f : weight * settings.volume,
                fadeIn: 0f,
                fadeOut: 0f,
                settings.pitch,
                spatialBlend: 0f,
                settings.startTime,
                attenuationMul: 1f,
                settings.mixerGroup,
                options,
                settings.cancellationToken
            );

            if (paused) handle.Pause();

            return new Music {
                handle = handle,
                volume = settings.volume,
                weight = weight,
                fadeIn = settings.fadeIn,
                fadeOut = settings.fadeOut,
            };
        }

        private static void ResumePendingMusic(MusicGroup musicGroup) {
            var music = musicGroup.pendingMusic;

            musicGroup.hasPendingMusic = false;
            musicGroup.pendingMusic = default;
            musicGroup.fadeOutAll = false;

            // The track has been released from the outside while it was waiting for its turn.
            if (!music.handle.IsValid()) return;

            music.handle.Volume = music.weight * music.volume;
            music.handle.Play();

            musicGroup.queue.Add(music);
        }

        private static void ReleasePendingMusic(MusicGroup musicGroup) {
            if (!musicGroup.hasPendingMusic) return;

            musicGroup.pendingMusic.handle.Release(immediate: true);

            musicGroup.hasPendingMusic = false;
            musicGroup.pendingMusic = default;
        }

        private static void StopMusicGroup(MusicGroup musicGroup, bool immediate) {
            ReleasePendingMusic(musicGroup);

            if (!immediate) {
                // Tracks keep their volumes and are faded out by the update, each with its own fade out.
                musicGroup.fadeOutAll = true;
                return;
            }

            var queue = musicGroup.queue;

            for (int i = 0; i < queue.Count; i++) {
                queue[i].handle.Release(immediate: true);
            }

            queue.Clear();
            musicGroup.fadeOutAll = false;
        }

        private MusicGroup GetOrCreateMusicGroup(AudioMixerGroup mixerGroup) {
            var mixerGroupId = GetMixerGroupId(mixerGroup);

            if (!_mixerGroupToMusicGroupMap.TryGetValue(mixerGroupId, out var musicGroup)) {
                musicGroup = new MusicGroup();
                _mixerGroupToMusicGroupMap[mixerGroupId] = musicGroup;
            }

            return musicGroup;
        }

        private bool TryGetMusicGroup(AudioMixerGroup mixerGroup, out MusicGroup musicGroup) {
            return _mixerGroupToMusicGroupMap.TryGetValue(GetMixerGroupId(mixerGroup), out musicGroup);
        }

        // Null mixer group means the default group of the audio pool, which is the same group for every track.
        private static EntityId GetMixerGroupId(AudioMixerGroup mixerGroup) {
            return mixerGroup == null ? EntityId.None : mixerGroup.GetEntityId();
        }
    }

}

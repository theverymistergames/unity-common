using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Audio;
using MisterGames.Common.Pooling;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace MisterGames.Dbg.Behaviours {

    /// <summary>
    /// Writes one memory usage line into the log with a fixed period.
    /// Tells a growing managed heap from a growing amount of native objects: the first one means garbage
    /// that the gc heap never gives back, the second one means objects that are never released.
    /// Every value is printed with two deltas: since the previous line and since the first one.
    /// </summary>
    public sealed class MemoryUsageLogger : MonoBehaviour {

        [Header("Log")]
        [SerializeField] [Min(1f)] private float _period = 30f;
        [SerializeField] private bool _logOnStart = true;

        [Tooltip("Counts of live unity objects: shows what exactly is leaking, but iterates every loaded " +
                 "object, so each log costs a hitch. Turn off when measuring frame time.")]
        [SerializeField] private bool _logObjectCounts = true;

        private struct Snapshot {
            public long process;
            public long totalAllocated;
            public long monoUsed;
            public long monoHeap;
            public long gc;
            public long gfx;

            public int gameObjects;
            public int materials;
            public int textures;
            public int meshes;
            public int audioClips;

            public int soundsActive;
            public int soundsReleasing;

            public int pools;
            public int poolActive;
            public int poolInactive;
            public int poolTracked;
        }

        private readonly StringBuilder _stringBuilder = new();
        private CancellationTokenSource _enableCts;
        private Snapshot _first;
        private Snapshot _prev;
        private bool _hasFirst;
        private float _startTime;
        private bool _canReadProcessMemory = true;

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            StartLogs(_enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        private async UniTask StartLogs(CancellationToken cancellationToken) {
            _startTime = Time.realtimeSinceStartup;

            if (_logOnStart) Log();

            while (!cancellationToken.IsCancellationRequested) {
                await AsyncExt.DelayUnscaled(_period, cancellationToken);

                if (cancellationToken.IsCancellationRequested) return;

                Log();
            }
        }

        private void Log() {
            var snapshot = TakeSnapshot();

            if (!_hasFirst) {
                _first = snapshot;
                _prev = snapshot;
                _hasFirst = true;
            }

            _stringBuilder.Clear();
            _stringBuilder.Append($"{nameof(MemoryUsageLogger)}: t {Time.realtimeSinceStartup - _startTime:0} s");

            AppendMemory("process", snapshot.process, _prev.process, _first.process);
            AppendMemory("total", snapshot.totalAllocated, _prev.totalAllocated, _first.totalAllocated);
            AppendMemory("mono", snapshot.monoUsed, _prev.monoUsed, _first.monoUsed);
            AppendMemory("mono heap", snapshot.monoHeap, _prev.monoHeap, _first.monoHeap);
            AppendMemory("gc", snapshot.gc, _prev.gc, _first.gc);
            AppendMemory("gfx", snapshot.gfx, _prev.gfx, _first.gfx);

            if (_logObjectCounts) {
                AppendCount("go", snapshot.gameObjects, _prev.gameObjects, _first.gameObjects);
                AppendCount("mat", snapshot.materials, _prev.materials, _first.materials);
                AppendCount("tex", snapshot.textures, _prev.textures, _first.textures);
                AppendCount("mesh", snapshot.meshes, _prev.meshes, _first.meshes);
                AppendCount("clip", snapshot.audioClips, _prev.audioClips, _first.audioClips);
            }

            AppendCount("sounds", snapshot.soundsActive, _prev.soundsActive, _first.soundsActive);
            AppendCount("sounds releasing", snapshot.soundsReleasing, _prev.soundsReleasing, _first.soundsReleasing);

            AppendCount("pools", snapshot.pools, _prev.pools, _first.pools);
            AppendCount("pooled active", snapshot.poolActive, _prev.poolActive, _first.poolActive);
            AppendCount("pooled inactive", snapshot.poolInactive, _prev.poolInactive, _first.poolInactive);
            AppendCount("pooled tracked", snapshot.poolTracked, _prev.poolTracked, _first.poolTracked);

            _prev = snapshot;

            Debug.Log(_stringBuilder.ToString());
        }

        private Snapshot TakeSnapshot() {
            var snapshot = new Snapshot {
                process = GetProcessMemory(),

                // Profiler values are zero in a non development build, process and gc memory are always valid.
                totalAllocated = Profiler.GetTotalAllocatedMemoryLong(),
                monoUsed = Profiler.GetMonoUsedSizeLong(),
                monoHeap = Profiler.GetMonoHeapSizeLong(),
                gfx = Profiler.GetAllocatedMemoryForGraphicsDriver(),
                gc = GC.GetTotalMemory(forceFullCollection: false),
            };

            if (_logObjectCounts) {
                snapshot.gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Length;
                snapshot.materials = Resources.FindObjectsOfTypeAll<Material>().Length;
                snapshot.textures = Resources.FindObjectsOfTypeAll<Texture>().Length;
                snapshot.meshes = Resources.FindObjectsOfTypeAll<Mesh>().Length;
                snapshot.audioClips = Resources.FindObjectsOfTypeAll<AudioClip>().Length;
            }

            if (AudioPool.Main is { } audioPool) {
                snapshot.soundsActive = audioPool.ActiveSoundsCount;
                snapshot.soundsReleasing = audioPool.ReleasingSoundsCount;
            }

            if (PrefabPool.Main is { } prefabPool) {
                snapshot.pools = prefabPool.PoolCount;
                snapshot.poolActive = prefabPool.ActiveInstancesCount;
                snapshot.poolInactive = prefabPool.InactiveInstancesCount;
                snapshot.poolTracked = prefabPool.TrackedInstancesCount;
            }

            return snapshot;
        }

        private void AppendMemory(string name, long value, long prev, long first) {
            _stringBuilder.Append(" | ").Append(name).Append(' ').Append($"{ToMb(value):0.0} MB");

            if (_hasFirst) {
                _stringBuilder.Append($" ({ToMb(value - prev):+0.0;-0.0;0} | {ToMb(value - first):+0.0;-0.0;0})");
            }
        }

        private void AppendCount(string name, int value, int prev, int first) {
            _stringBuilder.Append(" | ").Append(name).Append(' ').Append(value);

            if (_hasFirst) {
                _stringBuilder.Append($" ({value - prev:+0;-0;0} | {value - first:+0;-0;0})");
            }
        }

        private static float ToMb(long bytes) {
            return bytes / (1024f * 1024f);
        }

        private long GetProcessMemory() {
            if (!_canReadProcessMemory) return 0L;

            try {
                using var process = Process.GetCurrentProcess();
                return process.WorkingSet64;
            }
            catch (Exception) {
                // Not every platform allows reading process memory, other values are enough to see the trend.
                _canReadProcessMemory = false;
                return 0L;
            }
        }
    }

}

using System;
using System.Diagnostics;
using System.Threading;
using MisterGames.Common.Service;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MisterGames.Common.Inputs.DualSense {

    /// <summary>
    /// Both native calls of the DualSense library are blocking IO:
    /// SetControllerOutputState performs a HID WriteFile (up to several ms over Bluetooth),
    /// GetControllerCount performs a SetupAPI device enumeration.
    /// So neither is called from the main thread here: setters only write into a desired state
    /// and raise a dirty bit, and a single background worker pushes the latest state to the hardware.
    /// Writes of an unchanged state are dropped, and writes issued within one frame
    /// are coalesced into a single HID report.
    /// </summary>
    public sealed class DualSenseAdapter : MonoBehaviour, IDualSenseAdapter {

        [SerializeField] private bool _replicateOutputStateForAllControllers = true;
        [Tooltip("Period of controller count checks, seconds.")]
        [SerializeField] [Min(0.05f)] private float _controllerCountCheckPeriod = 1f;
        [Tooltip("Upper bound of HID output reports per second per controller. DualSense accepts up to ~250 Hz.")]
        [SerializeField] [Range(1, 250)] private int _maxOutputWritesPerSecond = 125;

        private const int MaxControllers = 8;
        private const int AllControllersMask = (1 << MaxControllers) - 1;
        private const int WorkerPollTimeoutMs = 50;

        private readonly object _lock = new();

        /// <summary>
        /// What the game wants the controllers to do. Written by the main thread under <see cref="_lock"/>.
        /// </summary>
        private readonly ControllerOutputState[] _desiredStates = new ControllerOutputState[MaxControllers];

        /// <summary>
        /// Snapshot of <see cref="_desiredStates"/> taken by the worker, so native calls run outside the lock.
        /// </summary>
        private readonly ControllerOutputState[] _writeStates = new ControllerOutputState[MaxControllers];

        private IDeviceService _deviceService;
        private Thread _worker;
        private AutoResetEvent _signal;

        private int _dirtyMask;
        private bool _outputMuted;
        private volatile int _controllerCount;
        private volatile bool _running;

        private void Awake() {
            _signal = new AutoResetEvent(initialState: false);
            _running = true;

            _worker = new Thread(OutputWorker) {
                Name = nameof(DualSenseAdapter),
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.BelowNormal,
            };

            _worker.Start();
        }

        private void OnDestroy() {
            _running = false;
            _signal?.Set();

            // Worker resets the hardware output state before exiting, so rumble does not stick after shutdown.
            _worker?.Join(millisecondsTimeout: 1000);
            _worker = null;

            _signal?.Dispose();
            _signal = null;
        }

        private void OnEnable() {
            if (TryGetDeviceService(out var deviceService)) deviceService.OnDeviceChanged += OnDeviceChanged;
        }

        private void OnDisable() {
            if (TryGetDeviceService(out var deviceService)) deviceService.OnDeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(InputDeviceType device) {
            switch (device) {
                case InputDeviceType.KeyboardMouse:
                    ResetAllGamepadsOutputState();
                    break;

                case InputDeviceType.Gamepad:
                    ActualizeAllGamepadsOutputState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(device), device, null);
            }
        }

        /// <summary>
        /// Resume pushing the desired output state to the hardware.
        /// </summary>
        public void ActualizeAllGamepadsOutputState() {
            SetOutputMuted(muted: false);
        }

        /// <summary>
        /// Silence the hardware without losing the desired output state,
        /// so <see cref="ActualizeAllGamepadsOutputState"/> can restore it.
        /// </summary>
        public void ResetAllGamepadsOutputState() {
            SetOutputMuted(muted: true);
        }

        public ControllerInputState GetInputState(int index = 0) {
            if (index < 0 || index >= _controllerCount) return default;

            return DualSenseNative.GetControllerInputState((uint) index);
        }

        public bool HasController(int index = 0) {
            return index >= 0 && index < _controllerCount &&
                   TryGetDeviceService(out var deviceService) &&
                   deviceService.CurrentDevice == InputDeviceType.Gamepad;
        }

        public void SetRumble(Vector2 rumble, int index = 0) {
            if (!HasController(index)) return;

            double left = rumble.x;
            double right = rumble.y;
            int dirtyMask = 0;

            lock (_lock) {
                if (_replicateOutputStateForAllControllers) {
                    for (int i = 0; i < _controllerCount; i++) {
                        dirtyMask |= ApplyRumble(i, left, right);
                    }
                }
                else {
                    dirtyMask = ApplyRumble(index, left, right);
                }

                _dirtyMask |= dirtyMask;
            }

            if (dirtyMask != 0) _signal?.Set();
        }

        public void SetTriggerEffect(GamepadSide side, TriggerEffect effect, int index = 0) {
            if (!HasController(index)) return;

            if (side is not (GamepadSide.Left or GamepadSide.Right)) {
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            int dirtyMask = 0;

            lock (_lock) {
                if (_replicateOutputStateForAllControllers) {
                    for (int i = 0; i < _controllerCount; i++) {
                        dirtyMask |= ApplyTriggerEffect(i, side, effect);
                    }
                }
                else {
                    dirtyMask = ApplyTriggerEffect(index, side, effect);
                }

                _dirtyMask |= dirtyMask;
            }

            if (dirtyMask != 0) _signal?.Set();
        }

        /// <summary>
        /// Returns a dirty bit for the controller if the state actually changed, 0 otherwise.
        /// Must be called under <see cref="_lock"/>.
        /// </summary>
        private int ApplyRumble(int index, double left, double right) {
            if (index < 0 || index >= MaxControllers) return 0;

            ref var state = ref _desiredStates[index];
            if (state.LeftRumbleIntensity.Equals(left) && state.RightRumbleIntensity.Equals(right)) return 0;

            state.LeftRumbleIntensity = left;
            state.RightRumbleIntensity = right;

            return 1 << index;
        }

        /// <summary>
        /// Returns a dirty bit for the controller if the state actually changed, 0 otherwise.
        /// Must be called under <see cref="_lock"/>.
        /// </summary>
        private int ApplyTriggerEffect(int index, GamepadSide side, in TriggerEffect effect) {
            if (index < 0 || index >= MaxControllers) return 0;

            ref var state = ref _desiredStates[index];

            if (side == GamepadSide.Left) {
                if (state.LeftTriggerEffect.Equals(effect)) return 0;
                state.LeftTriggerEffect = effect;
            }
            else {
                if (state.RightTriggerEffect.Equals(effect)) return 0;
                state.RightTriggerEffect = effect;
            }

            return 1 << index;
        }

        private void SetOutputMuted(bool muted) {
            lock (_lock) {
                if (_outputMuted == muted) return;

                _outputMuted = muted;
                _dirtyMask |= AllControllersMask;
            }

            _signal?.Set();
        }

        private bool TryGetDeviceService(out IDeviceService deviceService) {
            if (_deviceService != null) {
                deviceService = _deviceService;
                return true;
            }

            bool found = Services.TryGet(out deviceService);
            if (found) _deviceService = deviceService;

            return found;
        }

        private void OutputWorker() {
            var timer = Stopwatch.StartNew();

            long countCheckPeriodMs = (long) (_controllerCountCheckPeriod * 1000f);
            long minWriteIntervalMs = 1000L / (_maxOutputWritesPerSecond > 0 ? _maxOutputWritesPerSecond : 1);

            long nextCountCheckMs = 0L;
            long nextWriteMs = 0L;

            try {
                while (_running) {
                    _signal.WaitOne(WorkerPollTimeoutMs);
                    if (!_running) break;

                    if (timer.ElapsedMilliseconds >= nextCountCheckMs) {
                        nextCountCheckMs = timer.ElapsedMilliseconds + countCheckPeriodMs;
                        UpdateControllerCount();
                    }

                    // Dirty bits stay set, the next iteration picks them up.
                    if (timer.ElapsedMilliseconds < nextWriteMs) continue;

                    if (!TryFlushOutputStates()) continue;

                    nextWriteMs = timer.ElapsedMilliseconds + minWriteIntervalMs;
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            finally {
                ResetHardwareOutputState();
            }
        }

        private void UpdateControllerCount() {
            int count = (int) DualSenseNative.GetControllerCount();
            _controllerCount = count > MaxControllers ? MaxControllers : count;
        }

        /// <summary>
        /// Pushes the desired state of every dirty controller to the hardware.
        /// Returns true if at least one native write was performed.
        /// </summary>
        private bool TryFlushOutputStates() {
            int dirtyMask;
            bool muted;

            lock (_lock) {
                dirtyMask = _dirtyMask;
                _dirtyMask = 0;
                muted = _outputMuted;

                if (dirtyMask != 0) Array.Copy(_desiredStates, _writeStates, MaxControllers);
            }

            if (dirtyMask == 0) return false;

            int count = _controllerCount;
            bool written = false;

            for (int i = 0; i < count; i++) {
                if ((dirtyMask & (1 << i)) == 0) continue;

                DualSenseNative.SetControllerOutputState((uint) i, muted ? default : _writeStates[i]);
                written = true;
            }

            return written;
        }

        private void ResetHardwareOutputState() {
            int count = _controllerCount;

            for (int i = 0; i < count; i++) {
                DualSenseNative.SetControllerOutputState((uint) i, default);
            }
        }
    }

}

# MisterGames Common v0.5.0

## Features

### Data
- `Blackboard` - multiple type objects storage with custom editor
- `ScriptableSingleton` - for implementing singleton scriptable objects (for Editor or debug purposes) 
- `ScriptableObjectStorage` - for accessing any `ScriptableObject` within runtime (for debug purposes) and 
   in Editor (faster than `Resources.Load()` or `AssetDatabase.Find()`)
- `[ReadOnly]`, `[BeginReadOnlyGroup]`, `[EndReadOnlyGroup]` attribute for serializable properties and lists
- `[Serializable] Map<TKey, TValue>` - serializable dictionary with custom editor
- `[Serializable] Observable<TData>` - observable field with custom editor
- `[Serializable] Optional<TData>` - optional field with custom editor 
- `[Serializable] Pair<TDataA, TDataB>` - pair of types with custom editor
- `ObjectDataMap<TData>` - runtime dictionary for `UnityEngine.Object` as keys
- Type serializer
- Tree structure
- Unity Editor toolbar extender

### Processing
- Time domains - replacing for `MonoBehaviour.Update()` message.
  TimeDomain is a `ScriptableObject`, to which one can subscribe and receive Update/LateUpdate/FixedUpdate ticks 
  with provided delta time value. TimeDomain has custom editor, where user can tweak time scales and pause/resume domain.

```
public class MonoBehaviourWithUpdate : MonoBehaviour, IUpdate {
    [SerializeField] private TimeDomain _timeDomain;
            
    private void OnEnable() => _timeDomain.SubscribeUpdate(this);
    private void OnDisable() => _timeDomain.UnsubscribeUpdate(this);
    
    void IUpdate.OnUpdate(float dt) {
        // some frame actions...
    }        
}
```

- Jobs - extension for TimeDomain and replacing for Unity Coroutines.
  Job can be paused, sequenced and supports current TimeDomain settings.

```
public class MonoBehaviourWithJobs : MonoBehaviour {
    [SerializeField] private TimeDomain _timeDomain;
    private IJob _job;
    
    void StartJob() {
        _job = Jobs
            .Do(() => Debug.Log("Sequence started"))
            .Then(_timeDomain.Delay(4f))
            .Then(() => Debug.Log("Delay finished"))
            .Then(_timeDomain.WaitFrame())
            .Then(() => Debug.Log("WaitFrame finished"))
            .Then(_timeDomain.ScheduleTimes(startDelaySec: 1f, periodSec: 2f, repeatTimes: 5, () => Debug.Log("ScheduleTimes invoked")))
            .Then(Jobs
                .Do(() => Debug.Log("Nested Sequence started"))
                .Then(_timeDomain.Delay(1f))
                .Then(() => Debug.Log("Nested Sequence finished"))
            )
            .Then(() => Debug.Log("JobSequence finished"));
            
        _job.Start();
    }
    
    void PauseJob() {
        _job.Pause();
    }
}
```

- Collision and material detection base classes
- Editor coroutines extensions - API for coroutine operations in the editor:

```
IEnumerator NextFrame(Action action) { ... }
IEnumerator EachFrameWhile(Func<bool> actionWhile, Action onFinish = null) { ... }
IEnumerator Delay(float sec, Action action) { ... }
IEnumerator ScheduleWhile(float startDelaySec, float periodSec, Func<bool> actionWhile, Action onFinish = null) { ... }
IEnumerator ScheduleTimes(float startDelaySec, float periodSec, int repeatTimes, Action action, Action onFinish = null) { ... }
IEnumerator ScheduleTimesWhile(float startDelaySec, float periodSec, int repeatTimes, Func<bool> actionWhile, Action onFinish = null) { ... }
```

### Helpers
- MisterGames packages window: add, update and remove packages. Unity Editor top menu **MisterGames/Packages**

## Assembly definitions
- MisterGames.Common
- MisterGames.Common.Editor
- MisterGames.Common.RuntimeTests

## Dependencies
- Unity.EditorCoroutines.Editor (embedded)

## Installation 
- Open Unity Editor
- Top menu Window -> Package Manager 
- Click "+" button and select "Add package from git URL..."
- Paste there `https://gitlab.com/theverymistergames/common.git`
- If git executable is not found
  - Add lines to your PATH system variable:
    - **\[Your git directory\]\bin\git.exe**
    - **\[Your git directory\]\cmd**
  - Restart Unity Editor

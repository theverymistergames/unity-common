# MisterGames Scenario v0.1.0

## Usage
- `ScenarioEvent` - scriptable object that represents event. Event can be emitted and listened.
```
class ScenarioEventEmitterOnStart : MonoBehaviour {
  [SerializeField] ScenarioEvent scenarioEvent;

  void Start() {
    scenarioEvent.Emit();
  }
}

class ScenarioEventListener : MonoBehaviour {
  
  [SerializeField] ScenarioEvent scenarioEvent;

  void OnEnable() {
    scenarioEvent.OnEmit += OnScenarioEventEmitted;
  }

  void OnDisable() {
    scenarioEvent.OnEmit -= OnScenarioEventEmitted;
  }

  void OnScenarioEventEmitted() {
    // ...
  }
}
```

## Assembly definitions
- `MisterGames.Scenario`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)

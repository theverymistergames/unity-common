﻿# MisterGames Fsm v1.0.3

## Usage
1. Create `StateMachine` asset: right click menu: MisterGames/State Machine

2. Implement state (or use SimpleState) - base class `FsmState` is a scriptable object with `Enter/Exit` interface. 
   State has a list of transitions.
   
3. Implement transitions - base class `FsmTransition` is a scriptable object with method `Transit`: 
   if state machine is in the state that transition belongs to, it will be applied. 
   Transitions and states can be of any type that derives from their base type, so to perform transition you
   need some access function, e.g:
   
   ```
   class MyTransition : FsmTransition {
       
       [SerializeField] float dataWhenNeedTransit;
       
       void CheckNeedTransit(float data) {
           if (data == dataWhenNeedTransit) Transit();
       }
   }
   ```
   
   Now we need to call that `CheckNeedTransit` method:
   
   ```
   class MyStateMachineBehaviour : MonoBehaviour {
       
       [SerializeField] StateMachineRunner runner;
       
       void OnConditionChanged(float data) {
           var state = runner.Instance.CurrentState;
           foreach (var transition in state.transitions) {
               if (transition is MyTransition myTransition) {
                   myTransition.CheckNeedTransit(data);
               }
           }
       }
       
   }
   ```
    
4. Add `StateMachineRunner` to the gameobject and set `StateMachine` asset. Note that runner uses
   runtime copy of the asset that is created during Awake stage, so you can do stuff with it on Start
   stage at least.

5. You can select gameobject that has StateMachineRunner during play mode and see active states in State Machine Editor.

## Use-case: implementing pose state machine for character movement controller

Let's implement pose behaviour for character movement controller. But first, we need to describe what do we want:
- Stand state
- Crouch state
- Animated transitions between them
  
Okay, next step is to create state machine asset. Go to creation menu -> MisterGames -> State Machine:

![Create State Machine](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/1_create_fsm.gif)

To create state, RMB -> Create node -> Choose ```Simple State```, because we don't need logic in our states for now,
they will be just data containers. Create Stand and Crouch states. 

![Create Node](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/2_create_node.gif)

Note, that first state that you created is red, but second is green: red color indicates that state is initial. 
You can reassign initial state later by clicking RMB -> Select as initial state.

Pick Stand state, you will see variable Data, that must be a ```ScriptableObject```, in the state machine inspector.
Here we can store height of the character for each state. To do so, let's create a script:

```
[CreateAssetMenu]
public class PoseData : ScriptableObject {
    public float colliderHeight;
}
```

Then we can create two objects of ```PoseData``` for each state:
Assign collider height into data and set both objects to corresponding states:

![Pose Data](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/pose_data.png)

![Pose Data Assign](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/3_pose_data_assign.gif)

Now we need to implement transitions. To perform transition from stand to crouch state,
several conditions must be satisfied:

- We are in the stand state
- Crouch input is active

The same way for crouch to stand state transition:

- We are in crouch state
- Crouch input is not active

Create script for transitions:

```
public class PoseTransition : SimpleTransition {
    public bool crouchInputActive;
        
    public void CheckIfNeedTransit(bool isCrouchInputActive) {
        if (isCrouchInputActive == crouchInputActive) {
            Transit();
        }
    }  
}
```

```SimpleTransition``` is transition that has no interaction with ```StateMachineRunner``` - ```MonoBehaviour``` that runs 
state machine asset.

Create transition of type ```PoseTransition``` for our state machine:

![Create Transitions](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/4_create_transitions.gif)

And set needed conditions:

![Set Conditions](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/5_set_conditions.gif)

Next step is to create transition data, that will be used to perform animated transition between states. 
Let's create scriptable object and its instances, and set them into state machine transitions:

```
[CreateAssetMenu]
public class PoseTransitionData : ScriptableObject {
    public float duration;
}
``` 

![Transition Data](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/transition_data.png)

![Set Transitions](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/6_set_transitions.gif)

Now we need to make script that propagates conditions to state machine transitions:

```
public class PoseConditions : MonoBehaviour {
    public StateMachineRunner fsm;
    
    public void SetCrouchInput(bool isActive) {
        var state = fsm.Instance.CurrentState;
        foreach (var transition in state.transitions) {
            if (transition is PoseTransition poseTransition) {
                poseTransition.CheckIfNeedTransit(isActive);
            }
        }
    }
}
``` 

Suppose that ```SetCrouchInput``` method is called from some input script.

Okay, that was the last script for out state machine. To finish the task, there is last thing to do:
pose logic script, let's implement it:

``` 
public class PoseProcessor : MonoBehaviour {
    public StateMachineRunner fsm;
    PoseData lastData;
    
    void OnEnable() {
        fsm.OnEnterState += HandleState;
    }
    
    void OnDisable() {
        fsm.OnEnterState -= HandleState;
    }
    
    void Start() {
        lastData = fsm.Instance.CurrentState.data as PoseData;
    }
    
    void HandleState(FsmState state) {
        var data = state.data as PoseData;
        var transitionData = fsm.Instance.LastTransition.data as PoseTransitionData;
        
        var fromHeight = lastData.colliderHeight;
        var toHeight = data.colliderHeight;
        var duration = transitionData.duration;
        
        lastData = data;
        
        ChangeColliderHeight(fromHeight, toHeight, duration);
    }
    
    void ChangeColliderHeight(float fromHeight, float toHeight, float duration) { ... }
}
```

I skipped the implementation of ```ChangeColliderHeight``` method, because that is not the topic.

Add components to your character gameobject:
- ```StateMachineRunner``` with our ```PoseStateMachine``` asset
- ```PoseConditions```
- ```PoseProcessor```

Now we can do stand and crouch with state machine, and if we need to debug, we can watch our states in runtime. 
Start play mode, open State Machine Editor and click on the gameobject with ```StateMachineRunner``` component on it:

![Set Transitions](https://gitlab.com/theverymistergames/readme-data/-/blob/master/fsm/7_runtime.gif)

## Assembly definitions
- MisterGames.Fsm
- MisterGames.Fsm.Editor

## Dependencies
- MisterGames.Common
- MisterGames.Common.Editor

## Installation
- Add [MisterGames Common](https://gitlab.com/theverymistergames/common) package
- Open Unity Editor
- Top menu MisterGames -> Packages, add packages: 
  - [Fsm](https://gitlab.com/theverymistergames/fsm/)
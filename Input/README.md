# MisterGames Input v0.5.0

Package is a wrapper around `Unity.InputSystem`, that allows to add new input action or scheme (map) by creating corresponding scriptable objects. 

## Input actions
- `InputActionKey` - single action with `OnPress`, `OnRelease` and `OnUse` events.
  `OnUse` event is called by key activation strategy in the input action.
- `InputActionVector2` and `InputActionAxis` - continuous actions with `OnChanged` event.  

## Bindings
- Key/axis bindings are listed in `enum KeyBinding` and `enum AxisBinding`
- Static class `GlobalInput` with extension-style methods for bindings:
  - `bool GlobalInput.IsActive(this KeyBinding key)` to get if `KeyBinding` is active during current frame   
  - `Vector2 GlobalInput.GetValue(this AxisBinding axis)` to get an `AxisBinding` value during current frame   
- `InputBindingKey`/`InputBindingKeyCombo` for key input actions
- `InputBindingVector2Axis`/`InputBindingVector2Key` for vector2 input actions
- `InputBindingAxisKey` for axis input actions

## Key activation
- To resolve key overlaps some of the key activation strategies can be selected
- If no strategy selected, `OnUse` event of the key input action will not be invoked
- Create strategy via `Create menu -> MisterGames -> Input -> Activation`
- Strategy `Press`: `OnUse` is invoked on key pressed 
- Strategy `Release`: `OnUse` is invoked on key released
- Strategy `Hold`: `OnUse` is invoked after key being held for the hold time
- Strategy `Tap`: `OnUse` is invoked in the activation time after key has been pressed if no interruptions occured.
  Interruption can be caused by key overlap, eg.: 
  - `InputActionKey` `Action` with binding `G`
  - `InputActionKey` `ComboAction` with binding `Alt + G`
  - Tap strategy waits for the activation time after user pressed `G`, and if there was no combo action
    during this time, then activate `Action`

## Setup
- Create scriptable object `InputAction` (`Create menu -> MisterGames/Input/Action/...`), setup its bindings, activation strategy (`Create menu -> MisterGames/Input/Activation/...`) if needed.
- Create scriptable object `InputScheme` (`Create menu -> MisterGames/Input/InputScheme`) with input actions. Input scheme is a group of input actions, that can be activated and deactivated together.
- Create scriptable object `InputChannel` (`Create menu -> MisterGames/Input/InputChannel`) with input schemes. Input channel manages a list of input schemes. 
- Setup `InputUpdater` component on any game object in the scene and select input channel to update. It considered that there is only one `InputChannel`, but it is not a singleton.

## Assembly definitions
- `MisterGames.Input`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`Unity.InputSystem`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)

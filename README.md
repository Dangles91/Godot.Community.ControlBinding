# Godot.Community.ControlBinding
A WPF-style control binding implementation for Godot Mono
<br/>
![sliders](https://github.com/Dangles91/Godot.Community.ControlBinding/assets/9249458/9c600eba-a35a-4fc2-92a6-610bd5101897)

## Packages
A Nuget package is currently available here: https://www.nuget.org/packages/Godot.Community.ControlBinding

## Further development
Though functional, this project is in the early stages of development. More advanced features could still yet be developed, including:
- Control validation
- Control style formatting
- Formatting target objects as scenes
- Adding formatted scenes as children of a control

## Features
### Property binding
Simple property binding from Godot controls to C# properties

### List binding
A simple implementation of ObservableList to support list bindings to OptionButton and ItemList controls.
If the list objects inherit from `ObservableObject` the list items will receive update notifications.

### Enum list binding
A very specific list binding implementation to bind Enums to an OptionButton with support a target property to store the selected option.

### Two-way binding
Some controls support two-way binding by subscribing to their update signals for supported properties.
Supported properties:
- LineEdit.Text
- TextEdit.Text
- CodeEdit.Text
- Slider, Progress, SpinBox, and ScrollBar.Value
- CheckBox.ButtonPressed
- OptionButton.Selected

### Automatic type conversion
Automatic type conversion for most common types. Eg, binding string value "123" to int

### Custom formatters
Specify a custom `IValueFormatter` to format/convert values to and from the bound control and target property

### Custom list item formatters
List items can be further customised during binding by implementing a custom `IValueFormatter` that returns a `ListItem` with the desired properties

### Deep binding
Binding to target properties is implemented using a path syntax. eg. `MyClass.MyClassName` will bind to the `MyClassName` property on the `MyClass` object.

### Automatic rebinding
If any objects along the path are updated, the binding will be refreshed. Objects along the path must inherit from `ObservableObject` and implement `PropertyChanged`.

## Usage
The main components of control binding are the `ObservableObject` class and `OnPropertyChanged` method.

The script which backs your scene must inherit from `ObservableObejct`

See the example project for some bindings in action!
![image](https://github.com/Dangles91/Godot.Community.ControlBinding/assets/9249458/e0071bff-133a-4b49-be7d-7dfefd84616e)


### Property binding
Create a property with a backing field and trigger `OnPropertyChanged` in the setter
```c#
private int spinBoxValue;
public int SpinBoxValue
{
    get { return spinBoxValue; }
    set { spinBoxValue = value; OnPropertyChanged(nameof(SpinBoxValue)); }
}
```

Add a binding in `_Ready()`. This binding targets a control in the scene with the unique name **%SpinBox** with the `BindingMode` __TwoWay__. A BindingMode of TwoWay states that we want the spinbox value to be set into the target property and vice-versa.
```c#
BindProperty("%SpinBox", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
```

### Deep property binding
We can also bind to properties on other objects. These objects and properties must be relative to the current scene script.

```c#
// Bind to SelectedPlayerData.Health
BindProperty("%LineEdit", nameof(LineEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}", BindingMode.TwoWay);

// Alternatively represent this as a string path instead
BindProperty("%LineEdit", nameof(LineEdit.Text), "SelectedPlayerData.Health", BindingMode.TwoWay);
```

The property `SelectedPlayerData` must notify about changes to automatically rebind the control. TwoWay binding also requires that the PlayerData class implements `ObservableObject` and notify of property changes.
```c#
private PlayerData selectedPlayerData = new();
public PlayerData SelectedPlayerData
{
    get { return selectedPlayerData; }
    set { selectedPlayerData = value; OnPropertyChanged(nameof(SelectedPlayerData)); }
}

```

### Formatters
TODO: Write this doco
### List Binding
TODO: Write this doco

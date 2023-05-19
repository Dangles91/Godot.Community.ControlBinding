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
- Creating an editor plugin to specify bindings in the editor
- Code generation to implement OnPropertyChanged via an attribute decorator

## Features
### Property binding
Simple property binding from Godot controls to C# properties

### List binding
A simple implementation of `ObservableList` to support list bindings to `OptionButton` and `ItemList` controls.
If the list objects inherit from `ObservableList` the list items will receive update notifications.

### Enum list binding
A very specific list binding implementation to bind Enums to an OptionButton with support a target property to store the selected option.

### Two-way binding
Some controls support two-way binding by subscribing to their update signals for supported properties.
Supported properties:
- LineEdit.Text
- TextEdit.Text
- CodeEdit.Text
- Slider.Value, Progress.Value, SpinBox.Value, and ScrollBar.Value
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

### Scene list binding
Bindg a list to a control and provide a scene to instiate as a child of the target control. Modification (add/remove) are reflected in the contorls child list.

## Usage
The main components of control binding are the `ObservableObject` and `ObservableNode` classes which implement a `PropertyChanged` event and `OnPropertyChanged` method.

The script which backs your scene must inherit from `ObservableNode`. Other observable objects that are not added to the scene tree should inherit from `ObservableObject`. This prevents orphaned nodes.

See the example project for some bindings in action!
![image](https://github.com/Dangles91/Godot.Community.ControlBinding/assets/9249458/e0071bff-133a-4b49-be7d-7dfefd84616e)


### Property binding
Create a property with a backing field and trigger `OnPropertyChanged` in the setter
```c#
private int spinBoxValue;
public int SpinBoxValue
{
    get { return spinBoxValue; }
    set { spinBoxValue = value; OnPropertyChanged(); }
}
```

Alternatively, use the `SetValue` method to update the backing field and trigger `OnPropertyChanged`
```c#
private int spinBoxValue;
public int SpinBoxValue
{
    get { return spinBoxValue; }
    set { SetValue(ref spinBoxValue, value); }
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
    set { SetValue(ref selectedPlayerData, value); }
}

```

### Formatters
Binding can be declared with an optional formatter to format the value between your control and the property or implement custom type conversion as well as format list items
```c#
public class PlayerHealthFormatter : IValueFormatter
{
    public Func<object, object> FormatControl => (v) =>
    {
        return $"Player health: {v}";
    };

    public Func<object, object> FormatTarget => (v) =>
    {
        throw new NotImplementedException();
    };
}

BindProperty("%SpinBox", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay, new PlayerHealthFormatter());
```

This formatter will set a string value into the target control using the input value substituted into a string. `FormatControl` is not implemented here so the value would be passed back as-is in the case of a two-way binding.

### List Binding
List bindings must bound to an `ObservableList` to benefit from adding and removing items

```c#
public ObservableList<PlayerData> PlayerDatas {get;set;} = new(){
    new PlayerData{Health = 500},
};

BindListProperty("%ItemList2", nameof(PlayerDatas), formatter: new PlayerDataListFormatter());
```

The `PlayerDataListFormatter` formats the PlayerData entry into a usable string value using a `ListItem` to also provided conditional formatting to the control

```c#
public class PlayerDataListFormatter : IValueFormatter
{
    public Func<object, object> FormatControl => (v) =>
    {
        var pData = v as PlayerData;
        var listItem = new ListItem
        {
            DisplayValue = $"Health: {pData.Health}",
            Icon = ResourceLoader.Load<Texture2D>("uid://bfdb75li0y86u"),
            Disabled = pData.Health < 1,
            Tooltip = pData.Health == 0 ? "Health must be greater than 0" : null,

        };
        return listItem;
    };

    public Func<object, object> FormatTarget => throw new NotImplementedException();
}
```

### Scene List Binding
Bind an `ObservableList` to a controls child list to add/remove children. The target scene must have a script attached and inherit from `ObservableNode`. It must also provide an implementation for `SetViewModeldata()`

**Bind the control to a list and provide a path to the scene to instiate**
```c#
BindSceneList("%VBoxContainer", nameof(PlayerDatas), "uid://die1856ftg8w8");
```

**Scene implementation**
```c#
public partial class PlayerDataListItem : ObservableNode
{

	private PlayerData ViewModelData { get; set; }

    public override void SetViewModelData(object viewModelData)
    {
		ViewModelData = viewModelData as PlayerData;
        base.SetViewModelData(viewModelData);
    }	

	public override void _Ready()
	{
		BindProperty("%TextEdit", "Text", "ViewModelData.Health", BindingMode.TwoWay);
		base._Ready();
	}
}
```

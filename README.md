<div align="center">
<img src="https://github.com/Dangles91/Godot.Community.ControlBinding/assets/9249458/9c600eba-a35a-4fc2-92a6-610bd5101897" />
<h1>ControlBinding</h1>
<p>A WPF-style control binding implementation for Godot Mono</p>
</div>


## :package: Packages
<div align="center">
  <a href="https://www.nuget.org/packages/Godot.Community.ControlBinding">
    <img src="https://img.shields.io/nuget/v/Godot.Community.ControlBinding?label=nuget%20latest"/>
  </a>
  <a href="https://www.nuget.org/packages/Godot.Community.ControlBinding">
    <img alt="Nuget" src="https://img.shields.io/nuget/dt/Godot.Community.ControlBinding">
  </a>
</div>

## Demo
<details>
  <summary>:clapper: Movie</summary>
  
https://github.com/Dangles91/Godot.Community.ControlBinding/assets/9249458/0983acb5-fe14-46ae-9c9c-38a073bf3b7c

</details>

## :train: Further development
Though functional, this project is in the early stages of development. More advanced features could still yet be developed, including:
* [x] Binding control children
* [x] Instantiate scenes as control children
* [x] Control validation
* [ ] Control style formatting
* [ ] Creating an editor plugin to specify bindings in the editor
* [ ] Code generation to implement OnPropertyChanged via an attribute decorator

##  :dart: Features
### Property binding
Simple property binding from Godot controls to C# properties

### List binding
Bind list controls to an `ObservableList<T>`. List bindings support `OptionButton` and `ItemList` controls.
If the list objects inherit from `ObservableList` the controls will be updated to reflect changes made to the backing list.

### Enum list binding
A very specific list binding implementation to bind Enums to an OptionButton with support for a target property to store the selected option.

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
Bind a `ObservableList<T>` to any control and provide a scene to instiate as child nodes. Modifications (add/remove) are reflected in the control's child list.

Scene list bindings have limited TwoWay binding support. Child items removed from the tree will also be removed from the bound list.

![Animation](https://github.com/Dangles91/Godot.Community.ControlBinding/assets/9249458/8c21a527-8326-4ace-b4b3-8035b6c25ac6)

## :toolbox: Usage
The main components of control binding are the `ObservableObject` and `ObservableNode` classes which implement a `PropertyChanged` event and `OnPropertyChanged` method.

The script which backs your scene must inherit from `ObservableNode`. Other observable objects that are not added to the scene tree should inherit from `ObservableObject`. This prevents orphaned nodes.

See the ![example project](/examples/basic-bindings) for some bindings in action!

### Property binding
Create a property with a backing field and trigger `OnPropertyChanged` in the setter
<details>
  <summary>details</summary>

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

</details>

### Deep property binding
Bind to property members on other objects. These objects and properties must be relative to the current scene script.

<details>
<summary>details</summary>

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
</details>

### Formatters
A binding can be declared with an optional formatter to format the value between your control and the target property or implement custom type conversion. Formatters can also be used to modify list items properties by returning a `ListItem` object.

Formatter also have access to the target property value. In the example below, the `v` parameter is the value from the source property and `p` is the value of the target property.

<details>
<summary>details</summary>

```c#
public class PlayerHealthFormatter : IValueFormatter
{
    public Func<object, object, object> FormatControl => (v, p) =>
    {
        return $"Player health: {v}";
    };

    public Func<object, object, object> FormatTarget => (v, p) =>
    {
        throw new NotImplementedException();
    };
}

BindProperty("%SpinBox", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay, new PlayerHealthFormatter());
```

This formatter will set a string value into the target control using the input value substituted into a string. `FormatControl` is not implemented here so the value would be passed back as-is in the case of a two-way binding.

</details>

### List Binding
List bindings can be bound to an `ObservableList<T>` to benefit from adding and removing items

<details>
<summary>details</summary>

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

</details>

### Scene List Binding
Bind an `ObservableList<T>` to a control's child list to add/remove children. The target scene must have a script attached and inherit from `ObservableNode`. It must also provide an implementation for `SetViewModeldata()`

<details>
<summary>details</summary>

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
</details>

### Control input validation
Control bindings can be validated by either:
* Adding validation function to the binding
* Throwing a `ValidationException` from a formatter

There also two main ways of subscribing to validation changed events:
* Subscribe to the `ControlValidationChanged` event on the `ObservableNode` your bindings reside on
* Add a validation handler to the control binding

You can also use the `HasErrors` property on an `ObservableNode` to notify your UI of errors and review a full list of validation errors using the `GetValidationMessages()` method.


<details>
<summary>details</summary>
<br/>

**Adding validators and validation callbacks**

Property bindings implement a fluent builder pattern for modify the binding upon creation to add validators and a validator callback. 

You can have any number of validators but only one validation callback.

Validators are run the in the order they are registered and validation will stop at the first validator to return a non-empty string. Validators are run before formatters. The formatter will not be executed if a validation error occurs.

This example adds two validators and a callback to modulate the control and set the tooltip text.

```c#
BindProperty("%LineEdit", nameof(LineEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}", BindingMode.TwoWay)
    .AddValidator(v => int.TryParse((string)v, out int value) ? null : "Health must be a number")
    .AddValidator(v => int.TryParse((string)v, out int value) && value > 0 ? null : "Health must be greater than 0")
    .AddValidationHandler((control, isValid, message) => { 
        control.Modulate = isValid ? Colors.White : Colors.Red;
        control.TooltipText = message;
    });
```

**Subscribing to `ControlValidationChanged` events**

If you want to have common behaviour for many or all controls, you can subscribe to the `ControlValidationChanged` event and get updates about all control validations.

This example subscribes to all validation changed events to modulate the target control and set the tooltip text.

The last validation error message is also stored in the local ErrorMessage property to be bound to a UI label.

```csharp
public partial class MyClass : ObservableNode
{
    private string errorMessage;
    public string ErrorMessage
    {
        get { return errorMessage; }
        set { SetValue(ref errorMessage, value); }
    }

    public override void _Ready()
    {
        BindProperty("%ErrorLabel", nameof(Label.Visible), nameof(HasErrors), BindingMode.OneWay);
        BindProperty("%ErrorLabel", nameof(Label.Text), nameof(ErrorMessage), BindingMode.OneWay);
        ControlValidationChanged += OnControlValidationChanged;
    }

    private void OnControlValidationChanged(control, propertyName, message, isValid)
    {
        control.Modulate = isValid ? Colors.White : Colors.Red;
        control.TooltipText = message;

        // set properties to bind to
        ErrorMessage = message;
        ValidationSummary = GetValidationMessages();
    };
}
```

</details>

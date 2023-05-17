using ControlBinding.EventArgs;
using Godot;
using System;
using System.Collections.Generic;

namespace ControlBinding.ControlBinders;
public partial class CheckBoxControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoWayBindingProperties = new List<string>(){
            nameof(CheckBox.ButtonPressed)
        };

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if (_allowedTwoWayBindingProperties.Contains(bindingConfiguration.BoundPropertyName) &&
             (bindingConfiguration.BindingMode == BindingMode.TwoWay || bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
        {
            CheckBox boundControl = bindingConfiguration.BoundControl.Target as CheckBox;
            if (bindingConfiguration.BoundPropertyName == nameof(CheckBox.ButtonPressed))
                boundControl.Toggled += onToggledChanged;

        }
        base.BindControl(bindingConfiguration);
    }

    public void onToggledChanged(bool value)
    {
        EmitSignal(nameof(ControlValueChanged), _bindingConfiguration.BoundControl.Target as GodotObject, "ButtonPressed");
    }

    public override bool CanBindFor(object control)
    {
        return control is CheckBox;
    }

    public override IControlBinder CreateInstance()
    {
        return new CheckBoxControlBinder();
    }

    public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        throw new NotImplementedException();
    }

    public override void ClearEventBindings()
    {
        if (_allowedTwoWayBindingProperties.Contains(_bindingConfiguration.BoundPropertyName) &&
             (_bindingConfiguration.BindingMode == BindingMode.TwoWay || _bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
        {
            CheckBox boundControl = _bindingConfiguration.BoundControl.Target as CheckBox;
            if (_bindingConfiguration.BoundPropertyName == nameof(CheckBox.ButtonPressed))
                boundControl.Toggled -= onToggledChanged;

        }
    }

    public override void OnListItemChanged(object entry)
    {
        throw new NotImplementedException();
    }
}
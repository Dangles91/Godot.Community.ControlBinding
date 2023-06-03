using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Godot.Community.ControlBinding.ControlBinders;
internal partial class CheckBoxControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoWayBindingProperties = new(){
            nameof(CheckBox.ButtonPressed)
        };

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if (_allowedTwoWayBindingProperties.Contains(bindingConfiguration.BoundPropertyName) &&
             (bindingConfiguration.BindingMode == BindingMode.TwoWay || bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
        {
            CheckBox boundControl = bindingConfiguration.BoundControl.Target as CheckBox;
            if (bindingConfiguration.BoundPropertyName == nameof(CheckBox.ButtonPressed))
                boundControl.Toggled += OnToggledChanged;
        }
        base.BindControl(bindingConfiguration);
    }

    public void OnToggledChanged(bool value)
    {
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "ButtonPressed");
    }

    public override bool CanBindFor(object control)
    {
        return control is CheckBox;
    }

    public override IControlBinder CreateInstance()
    {
        return new CheckBoxControlBinder();
    }
}
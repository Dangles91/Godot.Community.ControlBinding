using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Godot.Community.ControlBinding.ControlBinders;
internal partial class LineEditControlBinder : ControlBinder
{
    private readonly List<string> _allowedTwoBindingProperties = new(){
            nameof(LineEdit.Text)
        };

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if (_allowedTwoBindingProperties.Contains(bindingConfiguration.BoundPropertyName) &&
             (bindingConfiguration.BindingMode == BindingMode.TwoWay || bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
        {
            LineEdit boundControl = bindingConfiguration.BoundControl.Target as LineEdit;

            if (bindingConfiguration.BoundPropertyName == nameof(LineEdit.Text))
            {
                boundControl.TextChanged += OnTextChanged;
            }
        }

        base.BindControl(bindingConfiguration);
    }

    public void OnTextChanged(string value)
    {
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "Text");
    }

    public override IControlBinder CreateInstance()
    {
        return new LineEditControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is LineEdit;
    }
}

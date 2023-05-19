using Godot.Community.ControlBinding.EventArgs;
using Godot;
using System;
using System.Collections.Generic;
using Range = Godot.Range;

namespace Godot.Community.ControlBinding.ControlBinders;

public partial class RangeControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoBindingProperties = new List<string>()
        {
            nameof(Range.Value)
        };

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if ((bindingConfiguration.BindingMode == BindingMode.OneWayToTarget || bindingConfiguration.BindingMode == BindingMode.TwoWay)
            && _allowedTwoBindingProperties.Contains(bindingConfiguration.BoundPropertyName))
        {
            Godot.Range boundControl = bindingConfiguration.BoundControl.Target as Range;

            if (bindingConfiguration.BoundPropertyName == nameof(Range.Value))
                boundControl.ValueChanged += onValueChanged;
        }

        base.BindControl(bindingConfiguration);
    }

    public void onValueChanged(double value)
    {        
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "Value");
    }

    public override void ClearEventBindings()
    {
        if ((_bindingConfiguration.BindingMode == BindingMode.OneWayToTarget || _bindingConfiguration.BindingMode == BindingMode.TwoWay)
            && _allowedTwoBindingProperties.Contains(_bindingConfiguration.BoundPropertyName))
        {
            Godot.Range boundControl = _bindingConfiguration.BoundControl.Target as Range;

            if (_bindingConfiguration.BoundPropertyName == nameof(Range.Value))
                boundControl.ValueChanged -= onValueChanged;
        }
    }

    public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        throw new NotImplementedException();
    }

    public override bool CanBindFor(object control)
    {
        return control is Range;
    }

    public override IControlBinder CreateInstance()
    {
        return new RangeControlBinder();
    }

    public override void OnListItemChanged(object entry)
    {
        throw new NotImplementedException();
    }
}
using System.Collections.Generic;

namespace Godot.Community.ControlBinding.ControlBinders;

internal partial class RangeControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoBindingProperties = new()
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
                boundControl.ValueChanged += OnValueChanged;
        }

        base.BindControl(bindingConfiguration);
    }

    public void OnValueChanged(double value)
    {
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "Value");
    }

    public override bool CanBindFor(object control)
    {
        return control is Range;
    }

    public override IControlBinder CreateInstance()
    {
        return new RangeControlBinder();
    }
}
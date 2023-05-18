using Godot.Community.ControlBinding.EventArgs;
using Godot;
using System;
using System.Collections.Generic;

namespace Godot.Community.ControlBinding.ControlBinders;

public partial class TextEditControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoBindingProperties = new List<string>(){
            nameof(TextEdit.Text)
        };

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if ((bindingConfiguration.BindingMode == BindingMode.OneWayToTarget || bindingConfiguration.BindingMode == BindingMode.TwoWay)
            && _allowedTwoBindingProperties.Contains(bindingConfiguration.BoundPropertyName))
        {
            TextEdit boundControl = bindingConfiguration.BoundControl.Target as TextEdit;
            if (bindingConfiguration.BoundPropertyName == "Text")
            {
                boundControl.TextChanged += onTextChanged;
            }
        }

        base.BindControl(bindingConfiguration);
    }

    public void onTextChanged()
    {
        EmitSignal(nameof(ControlValueChanged), _bindingConfiguration.BoundControl.Target as GodotObject, "Text");
    }

    public override void ClearEventBindings()
    {
        if ((_bindingConfiguration.BindingMode == BindingMode.OneWayToTarget || _bindingConfiguration.BindingMode == BindingMode.TwoWay)
            && _allowedTwoBindingProperties.Contains(_bindingConfiguration.BoundPropertyName))
        {
            TextEdit boundControl = _bindingConfiguration.BoundControl.Target as TextEdit;
            if (_bindingConfiguration.BoundPropertyName == "Text")
            {
                boundControl.TextChanged -= onTextChanged;
            }
        }
    }

    public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        throw new NotImplementedException();
    }

    public override IControlBinder CreateInstance()
    {
        return new TextEditControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is TextEdit;
    }

    public override void OnListItemChanged(object entry)
    {
        throw new NotImplementedException();
    }
}

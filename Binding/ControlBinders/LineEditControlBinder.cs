using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Godot.Community.ControlBinding.ControlBinders;
internal partial class LineEditControlBinder : ControlBinderBase
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
                boundControl.TextChanged += onTextChanged;
            }
        }

        base.BindControl(bindingConfiguration);
    }

    public void onTextChanged(string value)
    {
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "Text");
    }

    public override void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        throw new NotImplementedException();
    }

    public override IControlBinder CreateInstance()
    {
        return new LineEditControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is LineEdit;
    }

    public override void ClearEventBindings()
    {
        if (_allowedTwoBindingProperties.Contains(_bindingConfiguration.BoundPropertyName) &&
             (_bindingConfiguration.BindingMode == BindingMode.TwoWay || _bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
        {
            LineEdit boundControl = _bindingConfiguration.BoundControl.Target as LineEdit;

            if (_bindingConfiguration.BoundPropertyName == nameof(LineEdit.Text))
            {
                boundControl.TextChanged -= onTextChanged;
            }
        }
    }

    public override void OnListItemChanged(object entry)
    {
        throw new NotImplementedException();
    }
}

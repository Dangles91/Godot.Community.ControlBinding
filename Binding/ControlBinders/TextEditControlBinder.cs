using System.Collections.Generic;

namespace Godot.Community.ControlBinding.ControlBinders;

internal partial class TextEditControlBinder : ControlBinder
{
    private readonly List<string> _allowedTwoBindingProperties = new(){
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
                boundControl.TextChanged += OnTextChanged;
            }
        }

        base.BindControl(bindingConfiguration);
    }

    public void OnTextChanged()
    {
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "Text");
    }

    public override IControlBinder CreateInstance()
    {
        return new TextEditControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is TextEdit;
    }
}

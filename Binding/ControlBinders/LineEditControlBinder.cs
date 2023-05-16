using System;
using System.Collections.Generic;
using ControlBinding.Binding.EventArgs;
using ControlBinding.Binding.Interfaces;
using Godot;

namespace ControlBinding.Binding.ControlBinders
{
    public partial class LineEditControlBinder : ControlBinderBase
    {
        private readonly List<string> _allowedTwoBindingProperties = new List<string>(){
            nameof(LineEdit.Text)
        };        

        public override void BindControl(BindingConfiguration bindingConfiguration)
        {
            if(IsBound)
                return;

            if(_allowedTwoBindingProperties.Contains(bindingConfiguration.BoundPropertyName) &&
                 (bindingConfiguration.BindingMode == BindingMode.TwoWay || bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
            {
                LineEdit boundControl = bindingConfiguration.BoundControl.Target as LineEdit;

                if(bindingConfiguration.BoundPropertyName == nameof(LineEdit.Text))
                {                    
                    GD.Print($"Connecting on text changed: {boundControl}");                    
                    boundControl.TextChanged += onTextChanged;                    
                }
            }

            base.BindControl(bindingConfiguration);
        }

        public void onTextChanged(string value)
        {
            EmitSignal(nameof(ControlValueChanged), _bindingConfiguration.BoundControl.Target as GodotObject, "Text");
        }

        public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
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
            if(_allowedTwoBindingProperties.Contains(_bindingConfiguration.BoundPropertyName) &&
                 (_bindingConfiguration.BindingMode == BindingMode.TwoWay || _bindingConfiguration.BindingMode == BindingMode.OneWayToTarget))
            {
                LineEdit boundControl = _bindingConfiguration.BoundControl.Target as LineEdit;

                if(_bindingConfiguration.BoundPropertyName == nameof(LineEdit.Text))
                {
                    GD.Print($"Clearing on text changed: {boundControl}");
                    //boundControl.TextChanged -= onTextChanged;
                }
            }            
        }

        public override void OnListItemChanged(object entry)
        {
            throw new NotImplementedException();
        }
    }
}
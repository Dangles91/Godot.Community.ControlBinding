using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding.EventArgs;
using ControlBinding.Binding.Interfaces;
using Godot;

namespace ControlBinding.Binding.ControlBinders
{
    public partial class GenericControlBinder : ControlBinderBase
    {        
        public override void BindControl(BindingConfiguration bindingConfiguration)
        {   
            // no special bindings for these controls         
            base.BindControl(bindingConfiguration);           
        }

        public override bool CanBindFor(object control)
        {
            return control is Label || control is Button;
        }

        public override void ClearEventBindings()
        {
            throw new NotImplementedException();
        }

        public override IControlBinder CreateInstance()
        {
            return new GenericControlBinder();
        }

        public override void OnListItemChanged(object entry)
        {
            throw new NotImplementedException();
        }

        public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
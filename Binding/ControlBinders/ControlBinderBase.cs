using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlBinding.Binding.EventArgs;
using ControlBinding.Binding.Interfaces;
using ControlBinding.Binding.Utilities;
using ControlBinding.Binding.Services;
using Godot;

namespace ControlBinding.Binding.ControlBinders
{
    public abstract partial class ControlBinderBase : GodotObject, IControlBinder
    {
        [Signal]
        public delegate void ControlValueChangedEventHandler(Godot.Control control, string propertyName);

        internal BindingConfiguration _bindingConfiguration;

        public bool IsBound {get; set;}

        public virtual void BindControl(BindingConfiguration bindingConfiguration)
        {
            _bindingConfiguration = bindingConfiguration;
            IsBound = true;
        }

        #region Abstract methods
        public abstract IControlBinder CreateInstance();
        public abstract bool CanBindFor(object control);

        public abstract void OnListItemChanged(object entry);

        public abstract void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
        public abstract void ClearEventBindings();

        #endregion
    }
}
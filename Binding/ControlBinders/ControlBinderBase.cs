using System;
using System.Collections.Generic;
using Godot.Community.ControlBinding.EventArgs;
using Godot;

namespace Godot.Community.ControlBinding.ControlBinders
{
    public abstract partial class ControlBinderBase : IControlBinder
    {        
        public delegate void ControlValueChangedEventHandler(Godot.Control control, string propertyName);
        public event ControlValueChangedEventHandler ControlValueChanged;

        public void OnControlValueChanged(Godot.Control control, string propertyName)
        {
            ControlValueChanged?.Invoke(control, propertyName);
        }

        internal BindingConfiguration _bindingConfiguration;

        public bool IsBound { get; set; }
        public int Priority => 1;

        public virtual void BindControl(BindingConfiguration bindingConfiguration)
        {
            _bindingConfiguration = bindingConfiguration;
            IsBound = true;            
        }
        
        #region Abstract methods
        public abstract IControlBinder CreateInstance();
        public abstract bool CanBindFor(object control);

        public abstract void OnListItemChanged(object entry);
        
        public abstract void ClearEventBindings();
        public abstract void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);

        #endregion


    }
}
using System.Collections.Specialized;
using System.ComponentModel;

namespace Godot.Community.ControlBinding.ControlBinders
{
    internal abstract partial class ControlBinder : IControlBinder
    {
        public bool IsBound { get; set; }
        public int Priority => 1;
        internal BindingConfiguration _bindingConfiguration;

        public delegate void ControlValueChangedEventHandler(Godot.Control control, PropertyChangedEventArgs args);
        public event ControlValueChangedEventHandler ControlValueChanged;
        public void OnControlValueChanged(Godot.Control control, string propertyName)
        {
            ControlValueChanged?.Invoke(control, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void BindControl(BindingConfiguration bindingConfiguration)
        {
            _bindingConfiguration = bindingConfiguration;
            IsBound = true;
        }

        #region Abstract methods

        public abstract IControlBinder CreateInstance();
        public abstract bool CanBindFor(object control);

        #endregion
    }
}
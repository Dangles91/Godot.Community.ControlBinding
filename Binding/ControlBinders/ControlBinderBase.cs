using System.Collections.Specialized;
using System.ComponentModel;
using Godot.Community.ControlBinding.EventArgs;

namespace Godot.Community.ControlBinding.ControlBinders
{
    public abstract partial class ControlBinderBase : IControlBinder
    {
        public delegate void ControlValueChangedEventHandler(Godot.Control control, PropertyChangedEventArgs args);
        public event ControlValueChangedEventHandler ControlValueChanged;

        public void OnControlValueChanged(Godot.Control control, string propertyName)
        {
            ControlValueChanged?.Invoke(control, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void ControlChildListChangedEventHandler(Godot.Control control, ObservableListChangedEventArgs args);
        public event ControlChildListChangedEventHandler ControlChildListChanged;
        public void OnControlChildListChanged(Godot.Control control, ObservableListChangedEventArgs args)
        {
            ControlChildListChanged?.Invoke(control, args);
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
        public abstract void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs);

        #endregion

    }
}
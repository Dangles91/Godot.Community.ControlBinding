using System.Collections.Generic;
using ControlBinding.Binding.EventArgs;
using Godot;

namespace ControlBinding.Binding.Interfaces
{
    public interface IControlBinder
    {
        bool CanBindFor(System.Object control);
        void BindControl(BindingConfiguration bindingConfiguration);
        void OnListItemChanged(object entry);         
        IControlBinder CreateInstance();
        void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
        void ClearEventBindings();
        bool IsBound {get; set;}
    }
}
using Godot.Community.ControlBinding.EventArgs;

namespace Godot.Community.ControlBinding.ControlBinders;
public interface IControlBinder
{
    bool CanBindFor(System.Object control);
    void BindControl(BindingConfiguration bindingConfiguration);
    void OnListItemChanged(object entry);
    IControlBinder CreateInstance();
    void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
    void ClearEventBindings();
    bool IsBound { get; set; }
    public int Priority { get; }
}
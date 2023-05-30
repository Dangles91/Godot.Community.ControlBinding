using System.Collections.Specialized;

namespace Godot.Community.ControlBinding.ControlBinders;
internal interface IControlBinder
{
    bool CanBindFor(System.Object control);
    void BindControl(BindingConfiguration bindingConfiguration);
    void OnListItemChanged(object entry);
    IControlBinder CreateInstance();
    void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs);
    void ClearEventBindings();
    bool IsBound { get; set; }
    int Priority { get; }
}
using System.Collections.Specialized;

namespace Godot.Community.ControlBinding.ControlBinders;
internal interface IControlBinder
{
    bool CanBindFor(System.Object control);
    void BindControl(BindingConfiguration bindingConfiguration);
    IControlBinder CreateInstance();
    bool IsBound { get; set; }
    int Priority { get; }
}
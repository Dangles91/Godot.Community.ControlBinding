using ControlBinding.EventArgs;
using Godot;
using System;

namespace ControlBinding.ControlBinders;

public partial class GenericControlBinder : ControlBinderBase
{
    public new int Priority => 0;
    public override bool CanBindFor(object control)
    {
        return control is Godot.Control;
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
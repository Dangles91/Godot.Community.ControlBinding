using System.Collections.Generic;
using Godot.Community.ControlBinding.EventArgs;

namespace Godot.Community.ControlBinding.Collections;
public interface IObservableList
{
    public IList<object> GetBackingList();
    void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
}

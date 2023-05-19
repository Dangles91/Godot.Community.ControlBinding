using Godot.Community.ControlBinding.EventArgs;
using System.Collections.Generic;

namespace Godot.Community.ControlBinding.Collections;
public interface IObservableList
{
    public IList<object> GetBackingList();
    void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
    event ObservableListChangedEventHandler ObservableListChanged;
}

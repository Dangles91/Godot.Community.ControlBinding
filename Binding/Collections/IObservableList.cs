using System.Collections.Generic;
using ControlBinding.EventArgs;

namespace ControlBinding.Collections;
public interface IObservableList
{
    public IList<object> GetBackingList();
    void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
}

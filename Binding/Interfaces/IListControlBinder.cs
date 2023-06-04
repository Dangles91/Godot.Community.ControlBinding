using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Godot.Community.ControlBinding.ControlBinders
{
    public interface IListControlBinder
    {
        public delegate void ControlChildListChangedEventHandler(Godot.Control control, NotifyCollectionChangedEventArgs args);
        public event ControlChildListChangedEventHandler ControlChildListChanged;
        public void OnControlChildListChanged(Godot.Control control, NotifyCollectionChangedEventArgs args);
        public void OnListItemChanged(object entry);
        public void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs);
    }
}
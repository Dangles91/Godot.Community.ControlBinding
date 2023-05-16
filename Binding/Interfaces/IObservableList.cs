using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding.EventArgs;

namespace ControlBinding.Binding.Interfaces
{
    public interface IObservableList
    {
        public IList<object> GetBackingList();
        void OnObservableListChanged(ObservableListChangedEventArgs eventArgs);
    }
}
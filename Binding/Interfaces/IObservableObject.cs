using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding.Interfaces;

public interface IObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// Raise OnPropertyChanged when a bound property on this object changes
    /// </summary>
    /// <param name="name"></param>
    void OnPropertyChanged([CallerMemberName] string name = "not a property");
}
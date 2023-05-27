using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding.Interfaces;

public interface IObservableObject
{
    /// <summary>
    /// Raise OnPropertyChanged when a bound property on this object changes
    /// </summary>
    /// <param name="name"></param>
    void OnPropertyChanged([CallerMemberName] string name = "not a property");
    event PropertyChangedEventHandler PropertyChanged;
}
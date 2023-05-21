using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding.Interfaces;

public interface IObservableObject
{
    /// <summary>
    /// Raise OnPropertyChanged when a bound property on this object changes
    /// </summary>
    /// <param name="name"></param>
    void OnPropertyChanged(string name);
    event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Sets a value to the backing field of a property and triggers <see cref="OnPropertyChanged"/>
    /// </summary>
    /// <param name="field">The backing field of the property</param>
    /// <param name="value">The value that should be set</param>
    /// <param name="name">Name of the property</param>
    /// <typeparam name="T">Type of the property</typeparam>
    public void SetValue<T>(ref T field, T value, [CallerMemberName] string name = "not a property");
}
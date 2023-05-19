using Godot.Community.ControlBinding.Interfaces;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding;

public partial class ObservableObject : IObservableObject
{
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raise OnPropertyChanged when a bound property on this object changes
    /// </summary>
    /// <param name="name"></param>
#pragma warning disable S1006 // Method overrides should not change parameter defaults
    public void OnPropertyChanged([CallerMemberName] string name = "not a property")
#pragma warning restore S1006 // Method overrides should not change parameter defaults
    {
        if (name == "not a property")
            return;

        PropertyChanged?.Invoke(this, name);
    }
}

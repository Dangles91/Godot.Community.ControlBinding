using Godot.Community.ControlBinding.Interfaces;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding;

public partial class ObservableObject : IObservableObject
{
    public event PropertyChangedEventHandler PropertyChanged;
    
#pragma warning disable S1006 // Method overrides should not change parameter defaults
    
    /// <inheritdoc />
    public void OnPropertyChanged([CallerMemberName] string name = "not a property")
#pragma warning restore S1006 // Method overrides should not change parameter defaults
    {
        if (name == "not a property")
            return;

        PropertyChanged?.Invoke(this, name);
    }

    /// <inheritdoc />
    public void SetValue<T>(ref T field, T value, [CallerMemberName] string name = "not a property")
    {
        field = value;
        OnPropertyChanged(name);
    }
}

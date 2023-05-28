using Godot.Community.ControlBinding.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding;

public partial class ObservableObject : IObservableObject
{
    public event PropertyChangedEventHandler PropertyChanged;
    /// <inheritdoc />
    public void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        if (name == "not a property")
            return;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

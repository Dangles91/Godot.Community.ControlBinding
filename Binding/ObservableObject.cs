using Godot.Community.ControlBinding.Interfaces;
using System.Collections.Generic;
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
    public virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string name = "not a property")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(name);
    }
}

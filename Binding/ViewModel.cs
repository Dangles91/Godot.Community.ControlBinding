using Godot.Community.ControlBinding.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding;


public abstract partial class NodeViewModel : Node, IViewModel
{
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public abstract void SetViewModelData(object viewModelData);

    public virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string name = "not a property")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(name);
    }
}

public abstract partial class ControlViewModel : Control, IViewModel
{
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public abstract void SetViewModelData(object viewModelData);

    public virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string name = "not a property")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(name);
    }
}
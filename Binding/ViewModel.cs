using Godot.Community.ControlBinding.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot.Community.ControlBinding.Extensions;
using System.ComponentModel;

namespace Godot.Community.ControlBinding;


public abstract partial class NodeViewModel : Node, IViewModel
{
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public abstract void SetViewModelData(object viewModelData);
}

public abstract partial class ControlViewModel : Control, IViewModel
{
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public abstract void SetViewModelData(object viewModelData);
}
using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.ControlBinders;
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;
using Godot;
using Godot.Community.ControlBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding;

public partial class ObservableObject : GodotObject, IObservableObject
{
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raise OnPropertyChanged when a bound property on this object changes
    /// </summary>
    /// <param name="name"></param>
    public void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        if (name == "not a property")
            return;
        
        PropertyChanged?.Invoke(this, name);
    }
}

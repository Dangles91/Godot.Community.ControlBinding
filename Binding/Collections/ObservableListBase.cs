using ControlBinding.EventArgs;
using Godot;

namespace ControlBinding.Collections;

public partial class ObservableListBase : GodotObject
{
    [Signal]
    public delegate void PropertyChangedEventHandler(GodotObject sender);

    [Signal]
    public delegate void ObservableListChangedEventHandler(ObservableListChangedEventArgs args);
}
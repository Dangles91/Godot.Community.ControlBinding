using Godot.Community.ControlBinding.EventArgs;

namespace Godot.Community.ControlBinding
{
    public delegate void ObservableListChangedEventHandler(ObservableListChangedEventArgs args);
    public delegate void ValidationChangedEventHandler(Godot.Control control, string targetPropertyName, string message, bool isValid);
}
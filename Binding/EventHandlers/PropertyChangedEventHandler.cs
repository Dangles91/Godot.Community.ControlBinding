using Godot.Community.ControlBinding.EventArgs;

namespace Godot.Community.ControlBinding
{
    public delegate void PropertyChangedEventHandler(object owner, string propertyName);
    public delegate void ObservableListChangedEventHandler(ObservableListChangedEventArgs args);
}
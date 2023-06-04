using System;

namespace Godot.Community.ControlBinding;

internal class WeakBackReference
{
    public WeakReference ObjectReference { get; set; }
    public string PropertyName { get; set; }
}
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;
using System;
using System.Collections.Generic;

namespace Godot.Community.ControlBinding;

public enum BindingMode
{
    OneWay,
    TwoWay,
    OneWayToTarget,
}

public class BindingConfiguration
{
    public string BoundPropertyName { get; set; }
    public string TargetPropertyName { get; set; }
    public BindingMode BindingMode { get; set; }
    public bool IsListBinding { get; set; }
    public IObservableNode Owner { get; init; }
    public WeakReference BoundControl { get; set; }
    public WeakReference TargetObject { get; set; }
    public IValueFormatter Formatter { get; set; }
    public List<WeakBackReference> BackReferences { get; set; }
    public ISceneFormatter SceneFormatter { get; set; }
    public List<Func<object, string>> Validators { get; set; } = new();
    public string Path { get; set; }
}
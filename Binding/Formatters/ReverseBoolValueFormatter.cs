using System;

namespace Godot.Community.ControlBinding.Formatters;

public class ReverseBoolValueFormatter : IValueFormatter
{
    public Func<object, object, object> FormatControl => (v, _) => !(bool)v;

    public Func<object, object, object> FormatTarget => (v, _) => !(bool)v;
}

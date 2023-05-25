using System;

namespace Godot.Community.ControlBinding.Formatters;

public class ReverseBoolValueFormatter : IValueFormatter
{
    public Func<object, object, object> FormatControl => (v, pv) => !(bool)v;

    public Func<object, object, object> FormatTarget => (v, pv) => !(bool)v;
}

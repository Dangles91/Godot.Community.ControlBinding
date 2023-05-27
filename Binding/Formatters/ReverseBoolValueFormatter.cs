using System;

namespace Godot.Community.ControlBinding.Formatters;

public class ReverseBoolValueFormatter : IValueFormatter<bool, bool>
{
    public Func<bool, bool, bool> FormatControl => (v, pv) => !v;

    public Func<bool, bool, bool> FormatTarget => (v, pv) => !v;
}

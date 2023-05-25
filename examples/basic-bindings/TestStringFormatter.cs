

using Godot.Community.ControlBinding.Formatters;
using System;

namespace ControlBinding;

public class PlayerHealthFormatter : IValueFormatter
{
    public Func<object, object, object> FormatControl => (v,p) =>
    {
        var input = (string)v;
        return $"Player health: {v}";
    };

    public Func<object, object, object> FormatTarget => (v, p) => throw new NotImplementedException();
}
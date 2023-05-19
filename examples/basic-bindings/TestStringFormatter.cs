

using Godot.Community.ControlBinding.Formatters;
using System;

namespace ControlBinding;

public class PlayerHealthFormatter : IValueFormatter
{
    public Func<object, object> FormatControl => (v) =>
    {
        var input = (string)v;
        return $"Player health: {v}";
    };

    public Func<object, object> FormatTarget => (v) =>
    {
        throw new NotImplementedException();
    };
}
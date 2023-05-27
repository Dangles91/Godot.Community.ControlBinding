

using Godot.Community.ControlBinding.Formatters;
using System;

namespace ControlBinding;

public class PlayerHealthFormatter : IValueFormatter<string, string>
{
    public Func<string, string, string> FormatControl => (v,p) =>
    {
        var input = (string)v;
        return $"Player health: {v}";
    };

    public Func<string, string, string> FormatTarget => (v, p) => throw new NotImplementedException();
}
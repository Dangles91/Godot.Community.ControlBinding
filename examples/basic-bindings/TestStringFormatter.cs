

using ControlBinding.Formatters;
using System;

namespace ControlBinding;

public class TestStringFormatter : IValueFormatter
{
    public Func<object, object> FormatControl => (v) =>
    {
        var input = (string)v;
        return input.PadRight(10, '.');
    };

    public Func<object, object> FormatTarget => (v) =>
    {
        var input = (string)v;
        return input.Replace(",", "");
    };
}
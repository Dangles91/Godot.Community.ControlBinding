using System;

namespace ControlBinding.Formatters;

public class ReverseBoolValueFormatter : IValueFormatter
{
    public Func<object, object> FormatControl => (v) => {
        return !(bool)v;
    };

    public Func<object, object> FormatTarget => (v) => {
        return !(bool)v;
    };
}

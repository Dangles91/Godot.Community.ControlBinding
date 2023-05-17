using System;

namespace ControlBinding.Formatters;

/// <summary>
/// The interface to implement to create a value formatter that formats data before being set on the target control or object
/// </summary>
public interface IValueFormatter
{
    public Func<object, object> FormatControl { get; }
    public Func<object, object> FormatTarget { get; }
}

/// <summary>
/// Formats data before being set on the target control or object
/// </summary>
public class ValueFormatter : IValueFormatter
{
    public Func<object, object> FormatControl { get; init; }
    public Func<object, object> FormatTarget { get; init; }
}
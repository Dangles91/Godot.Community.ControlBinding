using System;

namespace Godot.Community.ControlBinding.Formatters;

/// <summary>
/// The interface to implement to create a value formatter that formats data before being set on the target control or object
/// </summary>
public interface IValueFormatter<TSource, TTarget> 
{
    public Func<TSource, TTarget, TTarget> FormatControl { get;}
    public Func<TTarget, TSource, TSource> FormatTarget { get;  }
}

/// <summary>
/// Formats data before being set on the target control or object
/// </summary>
public class ValueFormatter<TSource, TOutput> : IValueFormatter<TSource, TOutput>
{
    public Func<TSource, TOutput, TOutput> FormatControl { get; init; }
    public Func<TOutput, TSource, TSource> FormatTarget { get; init; }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot.Community.ControlBinding.Interfaces;

namespace Godot.Community.ControlBinding.Extensions;
public static class ObservableExtensions
{
    /// <summary>
    /// Sets a value to the backing field of a property and triggers <see cref="OnPropertyChanged"/>
    /// </summary>
    /// <param name="field">The backing field of the property</param>
    /// <param name="value">The value that should be set</param>
    /// <param name="name">Name of the property</param>
    /// <typeparam name="T">Type of the property</typeparam>
    public static void SetValue<T>(this IObservableObject observable, ref T field, T value, [CallerMemberName] string name = "not a property")
    {
        field = value;
        observable.OnPropertyChanged(name);
    }
}
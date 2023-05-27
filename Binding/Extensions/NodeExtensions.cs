using System;
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;

namespace Godot.Community.ControlBinding.Extensions;
public static class NodeExtensions
{
    /// <summary>
    /// Binds an emum to an OptionButton control with optional path for the selected value
    /// </summary>
    /// <param name="controlPath">The path of the Godot control in the scene.</param>
    /// <param name="selectedItemPath">The path of the property to bind to. Relative to this object.</param>
    /// <typeparam name="T">The enum type to bind the OptionButton to</typeparam>
    public static void BindEnumProperty<T>(this OptionButton node, BindingContainer bindingContainer, string selectedItemPath = null) where T : Enum
    {
        bindingContainer.BindEnumProperty<T>(node, selectedItemPath);
    }

    public static void BindSceneList(
        this Node node,
        BindingContainer bindingContainer,
        string path,
        string scenePath,
        BindingMode bindingMode = BindingMode.OneWay)
    {
        bindingContainer.BindSceneList(node, path, scenePath, bindingMode);
    }

    /// <summary>
    /// Bind a list control to an IObservableList or IList property
    /// Note: list controls include OptionButton and ItemList
    /// </summary>
    /// <param name="controlPath">The path of the Godot control in the scene.</param>
    /// <param name="path">The path of the property to bind to. Relative to this object.</param>
    /// <param name="bindingMode">The binding mode to use</param>
    /// <param name="formatter">The IValueFormatter to use to format the list item and target property. Return a <see cref="ControlBinding.Collections.ListItem"/> for greater formatting control.</param>
    public static void BindListProperty(
        this Node node,
        BindingContainer bindingContainer,

        string path,
        BindingMode bindingMode = BindingMode.OneWay,
        IValueFormatter formatter = null
        )
    {
        bindingContainer.BindListProperty(node, path, bindingMode, formatter);
    }

    /// <summary>
    /// Bind a control property to an object property
    /// </summary>
    /// <param name="node">The node to bind to</param>
    /// <param name="sourceProperty">The property of the Godot control to bind to</param>
    /// <param name="path">The path of the property to bind to. Relative to this object</param>
    /// <param name="bindingMode">The binding mode to use</param>
    /// <param name="formatter">The <see cref="ControlBinding.Formatters.IValueFormatter" /> to use to format the Control property and target property</param>
    public static Factories.BindingBuilder BindProperty(
        this Node node,
        BindingContainer bindingContainer,
        string sourceProperty,
        string path,
        BindingMode bindingMode = BindingMode.OneWay,
        IValueFormatter formatter = null
        )
    {
        return bindingContainer.BindProperty(node, sourceProperty, path, bindingMode, formatter);
    }
}
